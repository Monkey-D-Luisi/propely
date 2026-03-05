// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { JsonForms } from '@jsonforms/react';
import { vanillaRenderers, vanillaCells } from '@jsonforms/vanilla-renderers';
import { EnergyRatingRenderer, energyRatingTester } from '../EnergyRatingRenderer';

const schema = {
  type: 'object',
  properties: {
    energyRating: {
      type: 'string',
      enum: ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'Exempt', 'InProgress'],
    },
  },
};

const uiSchema = {
  type: 'Control',
  scope: '#/properties/energyRating',
};

const renderers = [
  ...vanillaRenderers,
  { tester: energyRatingTester, renderer: EnergyRatingRenderer },
];

describe('EnergyRatingRenderer', () => {
  it('renders all energy rating options', () => {
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

    expect(screen.getByRole('radio', { name: /^A$/i })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /^G$/i })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /Exempt/i })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /In Progress/i })).toBeInTheDocument();
  });

  it('highlights the selected rating', () => {
    render(
      <JsonForms
        schema={schema}
        uischema={uiSchema}
        data={{ energyRating: 'B' }}
        renderers={renderers}
        cells={vanillaCells}
        onChange={vi.fn()}
      />
    );

    const bButton = screen.getByRole('radio', { name: /^B$/i });
    expect(bButton).toHaveAttribute('aria-checked', 'true');
  });

  it('calls onChange when a rating is clicked', async () => {
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

    // JsonForms fires onChange on initial render, so reset the mock
    onChange.mockClear();

    fireEvent.click(screen.getByRole('radio', { name: /^C$/i }));

    await vi.waitFor(() => {
      expect(onChange).toHaveBeenCalled();
    });
  });
});
