// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.AiApi.Api.Configuration;
using Propely.AiApi.Api.Dtos;
using Propely.AiApi.Application.Actions.Commands.ExecuteAction;
using Propely.AiApi.Application.Common.Interfaces;

namespace Propely.AiApi.Api.Controllers;

/// <summary>
/// API controller for voice input (speech-to-text) and voice-driven action execution.
/// Accepts audio files, transcribes them via OpenAI, and optionally executes the transcribed text as an AI action.
/// </summary>
[ApiController]
[Route("v1/voice")]
[Authorize(Policy = AuthorizationPolicies.RequireAgent)]
public sealed class VoiceController : ControllerBase
{
    private readonly IVoiceTranscriptionService _transcriptionService;
    private readonly IMediator _mediator;
    private readonly ILogger<VoiceController> _logger;

    /// <summary>
    /// Maximum allowed audio file size in bytes (25 MB).
    /// </summary>
    public const long MaxFileSizeBytes = 25 * 1024 * 1024;

    /// <summary>
    /// Deduplication window — reject identical audio from the same user within this period.
    /// Prevents client-side bugs (e.g. missing effect cleanup) from flooding the pipeline.
    /// </summary>
    private static readonly TimeSpan DeduplicationWindow = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Tracks recent audio hashes per user to detect duplicate submissions.
    /// Key: "{userId}:{sha256-hex}", Value: timestamp.
    /// </summary>
    private static readonly ConcurrentDictionary<string, DateTimeOffset> RecentAudioHashes = new();

    /// <summary>
    /// Supported audio content types.
    /// </summary>
    public static readonly HashSet<string> SupportedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "audio/webm",
        "audio/wav",
        "audio/x-wav",
        "audio/wave",
        "audio/mpeg",
        "audio/mp3",
        "audio/mp4",
        "audio/m4a",
        "audio/x-m4a",
        "audio/ogg",
        "audio/flac",
        "audio/x-flac"
    };

    /// <summary>
    /// Supported audio file extensions.
    /// </summary>
    public static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".webm",
        ".wav",
        ".mp3",
        ".m4a",
        ".ogg",
        ".flac"
    };

    public VoiceController(
        IVoiceTranscriptionService transcriptionService,
        IMediator mediator,
        ILogger<VoiceController> logger)
    {
        _transcriptionService = transcriptionService;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Transcribes an audio file to text using OpenAI speech-to-text.
    /// </summary>
    /// <param name="audio">The audio file to transcribe (WebM, WAV, MP3, M4A, OGG, FLAC).</param>
    /// <param name="language">Optional BCP-47 language hint (e.g., "es", "en").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The transcription result with text, language, and duration.</returns>
    [HttpPost("transcribe")]
    [ProducesResponseType(typeof(TranscriptionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [RequestSizeLimit(MaxFileSizeBytes + 1024)] // Add a small buffer for form data overhead
    public async Task<IActionResult> Transcribe(
        [FromForm] IFormFile audio,
        [FromForm] string? language,
        CancellationToken ct)
    {
        var validationError = ValidateAudioFile(audio);
        if (validationError != null)
        {
            return validationError;
        }

        if (!User.TryGetUserId(out _))
        {
            return Unauthorized();
        }

        if (!User.TryGetOrgId(out _))
        {
            return Forbid();
        }

        using var stream = audio.OpenReadStream();
        var result = await _transcriptionService.TranscribeAsync(stream, audio.FileName, language, ct);

        return Ok(new TranscriptionResultDto(result.Text, result.Language, result.DurationMs));
    }

    /// <summary>
    /// Transcribes an audio file and immediately executes the transcribed text as an AI action.
    /// This is the combined voice-to-action endpoint.
    /// </summary>
    /// <param name="audio">The audio file to transcribe (WebM, WAV, MP3, M4A, OGG, FLAC).</param>
    /// <param name="language">Optional BCP-47 language hint (e.g., "es", "en").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The combined transcription and action execution result.</returns>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(VoiceExecuteResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [RequestSizeLimit(MaxFileSizeBytes + 1024)]
    public async Task<IActionResult> Execute(
        [FromForm] IFormFile audio,
        [FromForm] string? language,
        [FromForm] string? sessionId,
        CancellationToken ct)
    {
        var validationError = ValidateAudioFile(audio);
        if (validationError != null)
        {
            return validationError;
        }

        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        if (!User.TryGetOrgId(out var orgId))
        {
            return Forbid();
        }

        // Deduplicate: reject same audio from same user within the deduplication window.
        // This is a server-side guard against client bugs that re-submit the same blob.
        var audioHash = await ComputeAudioHashAsync(audio, ct);
        var dedupeKey = $"{userId}:{audioHash}";
        var now = DateTimeOffset.UtcNow;

        if (RecentAudioHashes.TryGetValue(dedupeKey, out var lastSeen) && now - lastSeen < DeduplicationWindow)
        {
            _logger.LogWarning(
                "[voice-pipeline] Duplicate audio detected for user {UserId} (hash={Hash}), last seen {Ago}ms ago — returning 429",
                userId, audioHash[..12], (now - lastSeen).TotalMilliseconds);
            return StatusCode(StatusCodes.Status429TooManyRequests, new ProblemDetails
            {
                Title = "Duplicate audio submission",
                Detail = "The same audio was already processed recently. Please wait before retrying.",
                Status = StatusCodes.Status429TooManyRequests,
            });
        }

        RecentAudioHashes[dedupeKey] = now;
        EvictStaleEntries();

        // Step 1: Transcribe
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("[voice-pipeline] Step 1: Transcribing audio ({FileSize} bytes, lang={Language})",
            audio.Length, language ?? "auto");
        using var stream = audio.OpenReadStream();
        var transcription = await _transcriptionService.TranscribeAsync(stream, audio.FileName, language, ct);
        _logger.LogInformation("[voice-pipeline] Step 1 done: \"{Text}\" ({CharCount} chars) in {ElapsedMs}ms",
            transcription.Text, transcription.Text.Length, sw.ElapsedMilliseconds);

        if (string.IsNullOrWhiteSpace(transcription.Text))
        {
            _logger.LogWarning("[voice-pipeline] Empty transcription — returning early");
            return Ok(new VoiceExecuteResultDto(
                TranscribedText: string.Empty,
                Language: transcription.Language,
                DurationMs: transcription.DurationMs,
                Action: new ActionResultDto(
                    Success: false,
                    ActionType: "Unknown",
                    Data: null,
                    Message: "No speech detected in the audio.",
                    Errors: ["Transcription returned empty text."],
                    Confidence: 0)));
        }

        // Step 2: Execute the transcribed text as an action
        sw.Restart();
        _logger.LogInformation("[voice-pipeline] Step 2: Executing action for \"{Text}\"", transcription.Text);
        var command = new ExecuteActionCommand(transcription.Text, orgId, userId, sessionId);
        var actionResult = await _mediator.Send(command, ct);
        _logger.LogInformation("[voice-pipeline] Step 2 done: success={Success}, type={ActionType}, confidence={Confidence} in {ElapsedMs}ms",
            actionResult.Success, actionResult.ActionType, actionResult.Confidence, sw.ElapsedMilliseconds);

        var actionDto = new ActionResultDto(
            Success: actionResult.Success,
            ActionType: actionResult.ActionType.ToString(),
            Data: actionResult.Data,
            Message: actionResult.Message,
            Errors: actionResult.Errors,
            Confidence: actionResult.Confidence);

        _logger.LogInformation("[voice-pipeline] Returning VoiceExecuteResultDto: success={Success}, message=\"{Message}\"",
            actionResult.Success, actionResult.Message);

        return Ok(new VoiceExecuteResultDto(
            TranscribedText: transcription.Text,
            Language: transcription.Language,
            DurationMs: transcription.DurationMs,
            Action: actionDto));
    }

    private IActionResult? ValidateAudioFile(IFormFile? audio)
    {
        if (audio == null || audio.Length == 0)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["audio"] = ["An audio file is required."]
                }));
        }

        if (audio.Length > MaxFileSizeBytes)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["audio"] = [$"Audio file exceeds the maximum size of 25 MB. File size: {audio.Length / (1024 * 1024.0):F1} MB."]
                }));
        }

        var extension = Path.GetExtension(audio.FileName);
        var contentType = audio.ContentType;

        if (!SupportedContentTypes.Contains(contentType) && !SupportedExtensions.Contains(extension))
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["audio"] = [$"Unsupported audio format. Content type: '{contentType}', extension: '{extension}'. Supported formats: WebM, WAV, MP3, M4A, OGG, FLAC."]
                }));
        }

        return null;
    }

    private static async Task<string> ComputeAudioHashAsync(IFormFile audio, CancellationToken ct)
    {
        using var stream = audio.OpenReadStream();
        var hash = await SHA256.HashDataAsync(stream, ct);
        return Convert.ToHexStringLower(hash);
    }

    private static void EvictStaleEntries()
    {
        var cutoff = DateTimeOffset.UtcNow - DeduplicationWindow - DeduplicationWindow; // 2x window
        foreach (var kvp in RecentAudioHashes)
        {
            if (kvp.Value < cutoff)
            {
                RecentAudioHashes.TryRemove(kvp.Key, out _);
            }
        }
    }
}
