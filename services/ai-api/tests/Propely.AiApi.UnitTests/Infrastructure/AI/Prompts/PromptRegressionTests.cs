// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AiApi.Application.Actions.Tools;
using Propely.AiApi.Infrastructure.AI;
using Propely.AiApi.Infrastructure.AI.Prompts;

namespace Propely.AiApi.UnitTests.Infrastructure.AI.Prompts;

/// <summary>
/// Regression tests verifying that system prompts and tool definitions
/// contain the required Spanish real estate vocabulary for accurate
/// intent classification and parameter extraction.
/// </summary>
public sealed class PromptRegressionTests
{
    private readonly ToolSchemaRegistry _registry = new();
    // --- System Prompt: Property Type Vocabulary ---

    [Theory]
    [InlineData("piso")]
    [InlineData("apartamento")]
    [InlineData("ático")]
    [InlineData("atico")]
    [InlineData("bajo")]
    [InlineData("dúplex")]
    [InlineData("duplex")]
    [InlineData("estudio")]
    [InlineData("loft")]
    [InlineData("adosado")]
    [InlineData("pareado")]
    [InlineData("chalet")]
    [InlineData("casa")]
    [InlineData("villa")]
    [InlineData("finca")]
    [InlineData("cortijo")]
    [InlineData("masía")]
    [InlineData("local comercial")]
    [InlineData("oficina")]
    [InlineData("nave industrial")]
    [InlineData("solar")]
    [InlineData("terreno")]
    [InlineData("parcela")]
    [InlineData("garaje")]
    [InlineData("trastero")]
    public void SystemPrompt_ShouldContainPropertyTypeTerm(string term)
    {
        var prompt = GetSystemPrompt();
        prompt.Should().Contain(term,
            $"the system prompt must include the Spanish property type term '{term}' for accurate classification");
    }

    // --- System Prompt: Operation Type Vocabulary ---

    [Theory]
    [InlineData("venta")]
    [InlineData("vender")]
    [InlineData("compra")]
    [InlineData("alquiler")]
    [InlineData("alquilar")]
    [InlineData("traspaso")]
    public void SystemPrompt_ShouldContainOperationTypeTerm(string term)
    {
        var prompt = GetSystemPrompt();
        prompt.Should().Contain(term,
            $"the system prompt must include the Spanish operation type term '{term}'");
    }

    // --- System Prompt: Feature Vocabulary ---

    [Theory]
    [InlineData("piscina")]
    [InlineData("jardín")]
    [InlineData("terraza")]
    [InlineData("balcón")]
    [InlineData("ascensor")]
    [InlineData("calefacción")]
    [InlineData("aire acondicionado")]
    [InlineData("amueblado")]
    [InlineData("reformado")]
    [InlineData("a estrenar")]
    [InlineData("luminoso")]
    public void SystemPrompt_ShouldContainFeatureTerm(string term)
    {
        var prompt = GetSystemPrompt();
        prompt.Should().Contain(term,
            $"the system prompt must include the Spanish feature term '{term}'");
    }

    // --- System Prompt: Financial & Area Terms ---

    [Theory]
    [InlineData("comunidad")]
    [InlineData("IBI")]
    [InlineData("catastro")]
    [InlineData("m² construidos")]
    [InlineData("m² útiles")]
    [InlineData("certificado energético")]
    public void SystemPrompt_ShouldContainFinancialAndAreaTerms(string term)
    {
        var prompt = GetSystemPrompt();
        prompt.Should().Contain(term,
            $"the system prompt must include the term '{term}'");
    }

    // --- System Prompt: Few-Shot Examples ---

    [Fact]
    public void SystemPrompt_ShouldContainAtLeast4FewShotExamples()
    {
        var prompt = GetSystemPrompt();

        // Count occurrences of "User:" pattern indicating few-shot examples
        var exampleCount = prompt.Split("User:").Length - 1;
        exampleCount.Should().BeGreaterThanOrEqualTo(4,
            "system prompt must contain at least 4 few-shot examples covering different listing styles");
    }

    [Fact]
    public void SystemPrompt_ShouldContainIdealistaStyleExample()
    {
        // Idealista-style: informal property listing with price and features
        var prompt = GetSystemPrompt();
        prompt.Should().Contain("piso",
            "system prompt should contain an Idealista-style listing example with 'piso'");
        prompt.Should().Contain("habitaciones",
            "system prompt should reference bedroom count using Spanish term 'habitaciones'");
    }

    [Fact]
    public void SystemPrompt_ShouldContainMixedLanguageExample()
    {
        // Mixed Spanish-English example
        var prompt = GetSystemPrompt();
        prompt.Should().Contain("lead",
            "system prompt should contain a mixed Spanish-English example using 'lead'");
    }

    // --- Tool Definitions: Spanish Aliases ---

    [Fact]
    public void CreatePropertyTool_ShouldContainSpanishPropertyTypeAliases()
    {
        var schema = _registry.GetByName("create_property")!;
        var json = schema.ParametersJsonSchema;

        json.Should().Contain("piso", "create_property tool should mention 'piso' in property_type description");
        json.Should().Contain("chalet", "create_property tool should mention 'chalet' in property_type description");
        json.Should().Contain("ático", "create_property tool should mention 'ático' in property_type description");
    }

    [Fact]
    public void CreatePropertyTool_ShouldContainSpanishOperationTypeAliases()
    {
        var schema = _registry.GetByName("create_property")!;
        var json = schema.ParametersJsonSchema;

        json.Should().Contain("venta", "create_property tool should mention 'venta' in operation_type description");
        json.Should().Contain("alquiler", "create_property tool should mention 'alquiler' in operation_type description");
    }

    [Fact]
    public void CreatePropertyTool_ShouldContainSpanishBedroomAlias()
    {
        var schema = _registry.GetByName("create_property")!;
        var json = schema.ParametersJsonSchema;

        json.Should().Contain("habitaciones",
            "create_property tool should mention 'habitaciones' in bedrooms description");
    }

    // --- PropertyExtractionPrompt: Spanish Context ---

    [Theory]
    [InlineData("piso")]
    [InlineData("apartamento")]
    [InlineData("ático")]
    [InlineData("chalet")]
    [InlineData("villa")]
    [InlineData("finca")]
    [InlineData("local comercial")]
    [InlineData("solar")]
    [InlineData("terreno")]
    public void PropertyExtractionPrompt_ShouldContainSpanishTerms(string term)
    {
        var prompt = PropertyExtractionPrompt.SystemPrompt;
        prompt.Should().Contain(term,
            $"the property extraction prompt must include Spanish term '{term}'");
    }

    [Theory]
    [InlineData("m² construidos")]
    [InlineData("m² útiles")]
    [InlineData("comunidad")]
    [InlineData("IBI")]
    [InlineData("catastro")]
    [InlineData("certificado energético")]
    public void PropertyExtractionPrompt_ShouldContainFinancialAndAreaTerms(string term)
    {
        var prompt = PropertyExtractionPrompt.SystemPrompt;
        prompt.Should().Contain(term,
            $"the property extraction prompt must include '{term}'");
    }

    // --- CopyGenerationPrompt: Spanish RE Conventions ---

    [Fact]
    public void CopyGenerationPrompt_ShouldReferenceSpanishMarket()
    {
        var prompt = CopyGenerationPrompt.BuildSystemPrompt("professional");
        prompt.Should().Contain("Spanish",
            "the copy generation prompt should reference the Spanish property market");
    }

    [Fact]
    public void CopyGenerationPrompt_ShouldMentionEuropeanSpanishConventions()
    {
        var prompt = CopyGenerationPrompt.BuildSystemPrompt("professional");
        prompt.Should().Contain("piso",
            "copy generation prompt should enforce European Spanish terminology (piso, not departamento)");
    }

    // --- Tool Definition Completeness ---

    [Fact]
    public void AllToolDefinitions_ShouldHave20Tools()
    {
        _registry.All.Should().HaveCount(20,
            "there should be 20 tool definitions covering all action types");
    }

    [Fact]
    public void AllToolDefinitions_ShouldHaveUniqueNames()
    {
        var names = _registry.All.Select(t => t.Name).ToList();
        names.Should().OnlyHaveUniqueItems("tool function names must be unique");
    }

    [Fact]
    public void AllToolDefinitions_ShouldNotHaveEmptyDescriptions()
    {
        foreach (var tool in _registry.All)
        {
            tool.Description.Should().NotBeNullOrWhiteSpace(
                $"tool '{tool.Name}' must have a description");
        }
    }

    // --- System Prompt: Contact Role Vocabulary ---

    [Theory]
    [InlineData("comprador")]
    [InlineData("vendedor")]
    [InlineData("inquilino")]
    [InlineData("propietario")]
    public void SystemPrompt_ShouldContainContactRoleTerms(string term)
    {
        var prompt = GetSystemPrompt();
        prompt.Should().Contain(term,
            $"the system prompt must include the Spanish contact role term '{term}'");
    }

    // --- System Prompt: Price Conversion Guidance ---

    [Theory]
    [InlineData("250k")]
    [InlineData("1M")]
    public void SystemPrompt_ShouldContainPriceConversionExamples(string term)
    {
        var prompt = GetSystemPrompt();
        prompt.Should().Contain(term,
            $"the system prompt must include price conversion example '{term}'");
    }

    // --- Helper ---

    /// <summary>
    /// Extracts the system prompt from the OpenAiIntentClassifier via reflection.
    /// This ensures the test verifies the actual prompt used in production.
    /// </summary>
    private static string GetSystemPrompt()
    {
        var field = typeof(OpenAiIntentClassifier).GetField("SystemPrompt",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        field.Should().NotBeNull("SystemPrompt field should exist in OpenAiIntentClassifier");
        var value = field!.GetValue(null) as string;
        value.Should().NotBeNullOrEmpty("SystemPrompt should not be empty");
        return value!;
    }
}
