# Walkthrough 0047 -- Prompt Engineering & Spanish RE Vocabulary

**Task:** [0047-prompt-engineering-spanish-vocabulary](../tasks/0047-prompt-engineering-spanish-vocabulary.md)
**Epic:** P3 -- AI Action Engine (Task 3.9)

## Summary

Implemented comprehensive Spanish real estate vocabulary mapping and enhanced all AI prompts with domain-specific context, few-shot examples, and Spanish term aliases.

## Decisions

- **Static vocabulary class** rather than DB-driven: the vocabulary is stable domain knowledge, not user-configurable data. A static dictionary with case-insensitive matching is the simplest correct approach.
- **Bidirectional by convention**: maps Spanish→English for normalization. English terms also map to themselves for pass-through.
- **Accent handling via OrdinalIgnoreCase + explicit entries**: both `atico` and `ático` are in the dictionary rather than doing Unicode normalization, because Spanish has very few accent variants in RE terminology and explicit entries are clearer.
- **Few-shot examples in system prompt**: 5 examples covering the most common agent workflows (create property, query, book viewing, create lead, filter).

## Files Created

| File | Description |
|------|-------------|
| `src/Infrastructure/AI/Vocabulary/SpanishRealEstateVocabulary.cs` | 90+ term mappings for property types, operations, features |
| `tests/UnitTests/.../Vocabulary/SpanishRealEstateVocabularyTests.cs` | 107 parameterized tests |

## Files Modified

| File | Change |
|------|--------|
| `src/Infrastructure/AI/OpenAiIntentClassifier.cs` | Enhanced system prompt with vocabulary reference, guidelines, 5 few-shot examples |
| `src/Infrastructure/AI/ToolDefinitions.cs` | Added Spanish aliases to property_type, operation_type descriptions |
| `src/Infrastructure/AI/Prompts/PropertyExtractionPrompt.cs` | Added Spanish market term reference section |
| `src/Infrastructure/AI/Prompts/CopyGenerationPrompt.cs` | Added European Spanish convention guidance |

## Commands Run

```bash
dotnet build services/ai-api/Propely.AiApi.sln           # 0 errors
dotnet test services/ai-api/tests/Propely.AiApi.UnitTests  # 461 passed
```

## Tests

- 107 new vocabulary tests covering all property types, accent variants, plurals, operations, features
- Completeness assertions: >=20 property types, >=10 operations, >=15 features
- Previous 354 tests still pass → Total: 461

## Checklist

- [x] Implementation complete
- [x] Tests passing
- [x] Walkthrough accurate
- [x] Committed
