'use client';

import { JsonForms } from '@jsonforms/react';
import { vanillaRenderers, vanillaCells } from '@jsonforms/vanilla-renderers';
import type { JsonSchema, UISchemaElement } from '@jsonforms/core';
import { propertyRenderers } from './renderers';
import propertySchema from '@/schemas/property-schema.json';
import propertyUiSchema from '@/schemas/property-ui-schema.json';

interface PropertyJsonFormProps {
  data: Record<string, unknown>;
  onChange: (data: Record<string, unknown>) => void;
  schema?: JsonSchema;
  uiSchema?: UISchemaElement;
  readonly?: boolean;
}

export function PropertyJsonForm({
  data,
  onChange,
  schema,
  uiSchema,
  readonly = false,
}: PropertyJsonFormProps) {
  const allRenderers = [...vanillaRenderers, ...propertyRenderers];

  return (
    <JsonForms
      schema={(schema ?? propertySchema) as JsonSchema}
      uischema={(uiSchema ?? propertyUiSchema) as UISchemaElement}
      data={data}
      renderers={allRenderers}
      cells={vanillaCells}
      onChange={({ data: newData }) => onChange(newData as Record<string, unknown>)}
      readonly={readonly}
    />
  );
}
