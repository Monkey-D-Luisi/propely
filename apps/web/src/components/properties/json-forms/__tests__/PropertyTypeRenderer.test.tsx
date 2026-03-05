// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { JsonForms } from '@jsonforms/react';
import { vanillaRenderers, vanillaCells } from '@jsonforms/vanilla-renderers';
import { PropertyTypeRenderer, propertyTypeTester } from '../PropertyTypeRenderer';

const schema = {
  type: 'object',
  properties: {
    propertyType: {
      type: 'string',
      enum: ['Apartment', 'House', 'Villa', 'Penthouse', 'Studio', 'Commercial', 'Land', 'Garage', 'StorageRoom', 'Building', 'Office'],
    },
  },
};

const uiSchema = {
  type: 'Control',
  scope: '#/properties/propertyType',
};

const renderers = [
  ...vanillaRenderers,
  { tester: propertyTypeTester, renderer: PropertyTypeRenderer },
];

describe('PropertyTypeRenderer', () => {
  it('renders all property type options', () => {
    render(
      <JsonForms
        schema={schema}
        uischema={uiSchema}
        data={{}}
        renderers={renderers}
        cells={vanillaCells}
        onChange={vi.fn()}
      />
    );

    expect(screen.getByRole('radio', { name: /Apartment/i })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /Villa/i })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /Land/i })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /Office/i })).toBeInTheDocument();
  });

  it('highlights selected type', () => {
    render(
      <JsonForms
        schema={schema}
        uischema={uiSchema}
        data={{ propertyType: 'Villa' }}
        renderers={renderers}
        cells={vanillaCells}
        onChange={vi.fn()}
      />
    );

    const villaButton = screen.getByRole('radio', { name: /Villa/i });
    expect(villaButton).toHaveAttribute('aria-checked', 'true');
  });

  it('calls onChange when a type is clicked', async () => {
    const onChange = vi.fn();

    render(
      <JsonForms
        schema={schema}
        uischema={uiSchema}
        data={{}}
        renderers={renderers}
        cells={vanillaCells}
        onChange={onChange}
      />
    );

    onChange.mockClear();

    fireEvent.click(screen.getByRole('radio', { name: /Apartment/i }));

    await vi.waitFor(() => {
      expect(onChange).toHaveBeenCalled();
    });
  });
});
