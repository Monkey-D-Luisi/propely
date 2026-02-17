// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Reflection;
using SaasTemplate.OrgsApi.Application.Common.Email;
using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using RazorLight;

namespace SaasTemplate.OrgsApi.Infrastructure.Email;

public sealed class RazorEmailTemplateRenderer : IEmailTemplateRenderer
{
    private const string DefaultLocale = InvitationEmailLocalization.DefaultLocale;
    private const string TemplateRoot = "SaasTemplate.OrgsApi.Infrastructure.Email.Templates";
    private const string LayoutTemplateName = "_Layout";

    private readonly Assembly _templateAssembly;
    private readonly RazorLightEngine _engine;
    private readonly ILogger<RazorEmailTemplateRenderer> _logger;

    public RazorEmailTemplateRenderer(ILogger<RazorEmailTemplateRenderer> logger)
    {
        _logger = logger;
        _templateAssembly = typeof(RazorEmailTemplateRenderer).Assembly;
        _engine = new RazorLightEngineBuilder()
            .SetOperatingAssembly(_templateAssembly)
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderAsync<TModel>(
        string templateName,
        TModel model,
        string locale = DefaultLocale,
        CancellationToken cancellationToken = default)
        where TModel : BaseEmailModel
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
        ArgumentNullException.ThrowIfNull(model);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedLocale = InvitationEmailLocalization.NormalizeLocale(locale);
        var template = await GetTemplateWithFallbackAsync(normalizedLocale, templateName, cancellationToken);
        var bodyHtml = await _engine.CompileRenderStringAsync(template.ResourceName, template.Content, model);

        var layoutResourceName = BuildResourceName(LayoutTemplateName);
        var layoutContent = await GetTemplateContentAsync(layoutResourceName, cancellationToken)
            ?? throw new InvalidOperationException($"Email layout template not found: {layoutResourceName}.");

        var layoutModel = BuildLayoutModel(model, bodyHtml, normalizedLocale);
        return await _engine.CompileRenderStringAsync(layoutResourceName, layoutContent, layoutModel);
    }

    private async Task<ResolvedTemplate> GetTemplateWithFallbackAsync(
        string locale,
        string templateName,
        CancellationToken cancellationToken)
    {
        var primaryResourceName = BuildResourceName(templateName, locale);
        var primaryContent = await GetTemplateContentAsync(primaryResourceName, cancellationToken);
        if (primaryContent is not null)
        {
            return new ResolvedTemplate(primaryResourceName, primaryContent);
        }

        if (string.Equals(locale, DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Email template not found: {primaryResourceName}.");
        }

        _logger.LogWarning(
            "Email template {TemplateName} not found for locale {Locale}. Falling back to {FallbackLocale}.",
            templateName,
            locale,
            DefaultLocale);

        var fallbackResourceName = BuildResourceName(templateName, DefaultLocale);
        var fallbackContent = await GetTemplateContentAsync(fallbackResourceName, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Email template not found for locale {locale} or fallback locale {DefaultLocale}: {templateName}.");

        return new ResolvedTemplate(fallbackResourceName, fallbackContent);
    }

    private async Task<string?> GetTemplateContentAsync(string resourceName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var stream = _templateAssembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static string BuildResourceName(string templateName, string? locale = null)
    {
        var normalizedTemplateName = templateName
            .Trim()
            .Replace("/", ".", StringComparison.Ordinal)
            .Replace("\\", ".", StringComparison.Ordinal);

        return string.IsNullOrWhiteSpace(locale)
            ? $"{TemplateRoot}.{normalizedTemplateName}.cshtml"
            : $"{TemplateRoot}.{locale}.{normalizedTemplateName}.cshtml";
    }

    private static LayoutEmailModel BuildLayoutModel<TModel>(TModel model, string bodyHtml, string locale)
        where TModel : BaseEmailModel
    {
        return new LayoutEmailModel
        {
            AppName = model.AppName,
            SupportUrl = model.SupportUrl,
            Year = model.Year,
            BodyHtml = bodyHtml,
            Locale = locale,
            FooterHelpPrefix = InvitationEmailLocalization.BuildFooterHelpPrefix(locale)
        };
    }

    private sealed record ResolvedTemplate(string ResourceName, string Content);
}
