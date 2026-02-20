// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Domain.WorkItems;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Propely.AiApi.Application.WorkItems.Commands.ParseWorkItem;

/// <summary>
/// Handles parsing of natural language text into structured work item fields using IOpenAiService.
/// </summary>
public sealed class ParseWorkItemCommandHandler : IRequestHandler<ParseWorkItemCommand, ParseWorkItemResult>
{
    private readonly IOpenAiService _openAiService;
    private readonly ILogger<ParseWorkItemCommandHandler> _logger;

    public ParseWorkItemCommandHandler(IOpenAiService openAiService, ILogger<ParseWorkItemCommandHandler> logger)
    {
        _openAiService = openAiService;
        _logger = logger;
    }

    public async Task<ParseWorkItemResult> Handle(ParseWorkItemCommand request, CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt(request.Text);

        string rawResponse;
        try
        {
            rawResponse = await _openAiService.GenerateTextAsync(prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI parsing failed for work item text. Falling back to raw text as title.");
            return new ParseWorkItemResult(
                Title: request.Text.Length > 200 ? request.Text[..200] : request.Text,
                Description: null,
                Status: "Pending",
                Priority: "Medium",
                Type: "Task",
                DueDateUtc: null,
                EstimatedEffort: "M",
                Confidence: 0.0);
        }

        return ParseResponse(rawResponse, request.Text);
    }

    private static string BuildPrompt(string text)
    {
        var statusNames = string.Join(", ", Enum.GetNames(typeof(WorkItemStatus)).Select(s => $"\"{s}\""));
        var priorityNames = string.Join(", ", Enum.GetNames(typeof(WorkItemPriority)).Select(s => $"\"{s}\""));
        var typeNames = string.Join(", ", Enum.GetNames(typeof(WorkItemType)).Select(s => $"\"{s}\""));
        var effortNames = string.Join(", ", Enum.GetNames(typeof(WorkItemEffort)).Select(s => $"\"{s}\""));
        return $"""
            Parse the following natural language text into structured work item fields.
            Return ONLY a JSON object with these fields:
            - "title": a concise title (max 200 chars)
            - "description": a longer description if there are additional details, or null
            - "status": one of {statusNames} (default to "Pending" if unclear)
            - "priority": one of {priorityNames} (default to "Medium" if unclear)
            - "type": one of {typeNames} (default to "Task" if unclear)
            - "dueDate": an ISO 8601 date string (YYYY-MM-DD) if a deadline is mentioned, or null
            - "estimatedEffort": one of {effortNames} based on complexity (default to "M" if unclear)
            - "confidence": a number between 0.0 and 1.0 indicating how confident you are in the parsing

            Text: "{text}"

            Respond with ONLY the JSON object, no markdown, no explanation.
            """;
    }

    private ParseWorkItemResult ParseResponse(string rawResponse, string originalText)
    {
        try
        {
            // Strip markdown fences if present
            var json = rawResponse.Trim();
            if (json.StartsWith("```"))
            {
                var firstNewline = json.IndexOf('\n');
                if (firstNewline >= 0) json = json[(firstNewline + 1)..];
                if (json.EndsWith("```")) json = json[..^3];
                json = json.Trim();
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var title = root.TryGetProperty("title", out var titleEl)
                ? titleEl.GetString() ?? originalText
                : originalText;

            var description = root.TryGetProperty("description", out var descEl) && descEl.ValueKind != JsonValueKind.Null
                ? descEl.GetString()
                : null;

            var status = root.TryGetProperty("status", out var statusEl)
                ? statusEl.GetString() ?? "Pending"
                : "Pending";

            // Validate status is one of the allowed values
            var validStatuses = Enum.GetNames(typeof(WorkItemStatus));
            if (!validStatuses.Contains(status))
            {
                status = "Pending";
            }

            var priority = root.TryGetProperty("priority", out var priorityEl) && priorityEl.ValueKind != JsonValueKind.Null
                ? priorityEl.GetString() ?? "Medium"
                : "Medium";

            // Validate priority is one of the allowed values
            var validPriorities = Enum.GetNames(typeof(WorkItemPriority));
            if (!validPriorities.Contains(priority))
            {
                priority = "Medium";
            }

            var type = root.TryGetProperty("type", out var typeEl) && typeEl.ValueKind != JsonValueKind.Null
                ? typeEl.GetString() ?? "Task"
                : "Task";

            // Validate type is one of the allowed values
            var validTypes = Enum.GetNames(typeof(WorkItemType));
            if (!validTypes.Contains(type))
            {
                type = "Task";
            }

            var dueDate = root.TryGetProperty("dueDate", out var dueDateEl) && dueDateEl.ValueKind != JsonValueKind.Null
                ? dueDateEl.GetString()
                : null;

            var estimatedEffort = root.TryGetProperty("estimatedEffort", out var effortEl) && effortEl.ValueKind != JsonValueKind.Null
                ? effortEl.GetString() ?? "M"
                : "M";

            // Validate estimatedEffort is one of the allowed values
            var validEfforts = Enum.GetNames(typeof(WorkItemEffort));
            if (!validEfforts.Contains(estimatedEffort))
            {
                estimatedEffort = "M";
            }

            var confidence = root.TryGetProperty("confidence", out var confEl)
                ? confEl.TryGetDouble(out var c) ? c : 0.5
                : 0.5;

            if (title.Length > 200) title = title[..200];

            return new ParseWorkItemResult(title, description, status, priority, type, dueDate, estimatedEffort, Math.Clamp(confidence, 0.0, 1.0));
        }
        catch (Exception)
        {
            _logger.LogWarning("Failed to parse AI response as JSON. Using raw text as title.");
            return new ParseWorkItemResult(
                Title: originalText.Length > 200 ? originalText[..200] : originalText,
                Description: null,
                Status: "Pending",
                Priority: "Medium",
                Type: "Task",
                DueDateUtc: null,
                EstimatedEffort: "M",
                Confidence: 0.0);
        }
    }
}
