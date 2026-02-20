// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Application.Auth.Commands.ForgotPassword;
using Propely.OrgsApi.Application.Auth.Commands.ResetPassword;
using Propely.OrgsApi.Application.Common.Email;
using Propely.OrgsApi.Application.Users.Commands.ChangePassword;
using Propely.OrgsApi.Application.Users.Commands.DeleteAccount;
using Propely.OrgsApi.Application.Users.Commands.LoginUser;
using Propely.OrgsApi.Application.Users.Commands.RefreshAccessToken;
using Propely.OrgsApi.Application.Users.Commands.RegisterUser;
using Propely.OrgsApi.Application.Users.Commands.ResendVerification;
using Propely.OrgsApi.Application.Users.Commands.UpdateProfile;
using Propely.OrgsApi.Application.Users.Commands.VerifyEmail;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users.Queries.GetCurrentUser;
using Propely.OrgsApi.Api.Extensions;
using Propely.OrgsApi.Api.Services;

namespace Propely.OrgsApi.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<VerifyEmailRequest> _verifyEmailValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly IValidator<DeleteAccountRequest> _deleteAccountValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly bool _useSecureCookies;
    private readonly SameSiteMode _sameSiteMode;

    public AuthController(
        IMediator mediator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<VerifyEmailRequest> verifyEmailValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<UpdateProfileRequest> updateProfileValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator,
        IValidator<DeleteAccountRequest> deleteAccountValidator,
        IValidator<ForgotPasswordRequest> forgotPasswordValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        CookieSettings cookieSettings)
    {
        _mediator = mediator;
        _registerValidator = registerValidator;
        _verifyEmailValidator = verifyEmailValidator;
        _loginValidator = loginValidator;
        _updateProfileValidator = updateProfileValidator;
        _changePasswordValidator = changePasswordValidator;
        _deleteAccountValidator = deleteAccountValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _useSecureCookies = cookieSettings.UseSecureCookies;
        _sameSiteMode = cookieSettings.SameSiteMode;
    }

    [HttpGet("csrf")]
    [AllowAnonymous]
    public IActionResult GetCsrfToken()
    {
        var csrfSecret = _configuration["Csrf:Secret"]
            ?? _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException(
                "CSRF secret is not configured. Set Csrf:Secret or Jwt:Secret in configuration.");
        var token = CsrfValidator.GenerateToken(csrfSecret);

        Response.Cookies.Append("csrf_token", token, new CookieOptions
        {
            HttpOnly = false,
            SameSite = _sameSiteMode,
            Secure = _useSecureCookies,
            Path = "/"
        });

        return Ok(new { csrfToken = token });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!CsrfValidator.Validate(Request))
        {
            return StatusCode(403, new { error = "Invalid CSRF token." });
        }

        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var frontendBaseUrl = _configuration["Auth:FrontendBaseUrl"] ?? "http://localhost:3000";
        var locale = ResolveRequestLocale(request.Locale);
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.Name,
            frontendBaseUrl,
            locale);
        var result = await _mediator.Send(command, cancellationToken);

        Response.SetAccessTokenCookie(result.Token, _useSecureCookies, _sameSiteMode);
        Response.SetRefreshTokenCookie(result.RefreshToken, _useSecureCookies, _sameSiteMode);
        return StatusCode(201, new { userId = result.UserId, accessToken = result.Token });
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        if (!CsrfValidator.Validate(Request))
        {
            return StatusCode(403, new { error = "Invalid CSRF token." });
        }

        var validationResult = await _verifyEmailValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var command = new VerifyEmailCommand(request.Token);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new { ok = result.EmailVerified });
    }

    [HttpPost("resend-verification")]
    [Authorize]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest? request,
        CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!CsrfValidator.Validate(Request))
        {
            return StatusCode(403, new { error = "Invalid CSRF token." });
        }

        var frontendBaseUrl = _configuration["Auth:FrontendBaseUrl"] ?? "http://localhost:3000";
        var command = new ResendVerificationCommand(
            userId.Value,
            frontendBaseUrl,
            ResolveRequestLocale(request?.Locale));
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new { ok = true, sent = result.Sent, alreadyVerified = result.AlreadyVerified });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!CsrfValidator.Validate(Request))
        {
            return StatusCode(403, new { error = "Invalid CSRF token." });
        }

        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var command = new LoginUserCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);

        Response.SetAccessTokenCookie(result.Token, _useSecureCookies, _sameSiteMode);
        Response.SetRefreshTokenCookie(result.RefreshToken, _useSecureCookies, _sameSiteMode);
        return Ok(new { userId = result.UserId, accessToken = result.Token });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new { error = "MISSING_REFRESH_TOKEN" });
        }

        var command = new RefreshAccessTokenCommand(refreshToken);
        var result = await _mediator.Send(command, cancellationToken);

        Response.SetAccessTokenCookie(result.AccessToken, _useSecureCookies, _sameSiteMode);
        Response.SetRefreshTokenCookie(result.RefreshToken, _useSecureCookies, _sameSiteMode);
        return Ok(new { ok = true, accessToken = result.AccessToken });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        if (!CsrfValidator.Validate(Request))
        {
            return StatusCode(403, new { error = "Invalid CSRF token." });
        }

        var validationResult = await _forgotPasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var frontendBaseUrl = _configuration["Auth:FrontendBaseUrl"] ?? "http://localhost:3000";
        var command = new ForgotPasswordCommand(request.Email, frontendBaseUrl, ResolveRequestLocale(request.Locale));
        await _mediator.Send(command, cancellationToken);

        return Ok(new { ok = true });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (!CsrfValidator.Validate(Request))
        {
            return StatusCode(403, new { error = "Invalid CSRF token." });
        }

        var validationResult = await _resetPasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var command = new ResetPasswordCommand(request.Token, request.NewPassword);
        await _mediator.Send(command, cancellationToken);

        return Ok(new { ok = true });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var query = new GetCurrentUserQuery(userId.Value);
        var user = await _mediator.Send(query, cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            user = new UserResponse(user.Id, user.Email, user.Name, user.EmailVerified, user.IsSystemAdmin)
        });
    }

    [HttpPatch("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!CsrfValidator.Validate(Request))
        {
            return StatusCode(403, new { error = "Invalid CSRF token." });
        }

        var validationResult = await _updateProfileValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var command = new UpdateProfileCommand(userId.Value, request.Name);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new { id = result.Id, email = result.Email, name = result.Name });
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!CsrfValidator.Validate(Request))
        {
            return StatusCode(403, new { error = "Invalid CSRF token." });
        }

        var validationResult = await _changePasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var command = new ChangePasswordCommand(userId.Value, request.CurrentPassword, request.NewPassword);
        await _mediator.Send(command, cancellationToken);

        return Ok(new { ok = true });
    }

    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!CsrfValidator.Validate(Request))
        {
            return StatusCode(403, new { error = "Invalid CSRF token." });
        }

        var validationResult = await _deleteAccountValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { error = validationResult.Errors[0].ErrorMessage });
        }

        var command = new DeleteAccountCommand(userId.Value, request.Password);
        await _mediator.Send(command, cancellationToken);

        Response.DeleteAccessTokenCookie(_useSecureCookies, _sameSiteMode);
        Response.DeleteRefreshTokenCookie(_useSecureCookies, _sameSiteMode);

        return Ok(new { ok = true });
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is not null)
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(userId.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        Response.DeleteAccessTokenCookie(_useSecureCookies, _sameSiteMode);
        Response.DeleteRefreshTokenCookie(_useSecureCookies, _sameSiteMode);

        return Ok(new { ok = true });
    }

    private string ResolveRequestLocale(string? requestedLocale)
    {
        if (!string.IsNullOrWhiteSpace(requestedLocale))
        {
            return InvitationEmailLocalization.NormalizeLocale(requestedLocale);
        }

        var acceptLanguage = Request.Headers.AcceptLanguage.FirstOrDefault();
        return InvitationEmailLocalization.NormalizeLocale(acceptLanguage);
    }
}
