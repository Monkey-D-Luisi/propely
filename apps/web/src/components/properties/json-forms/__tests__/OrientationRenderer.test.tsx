// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { JsonForms } from '@jsonforms/react';
import { vanillaRenderers, vanillaCells } from '@jsonforms/vanilla-renderers';
import { OrientationRenderer, orientationTester } from '../OrientationRenderer';

const schema = {
  type: 'object',
  properties: {
    orientation: {
      type: 'string',
      enum: ['N', 'NE', 'E', 'SE', 'S', 'SW', 'W', 'NW'],
    },
  },
};

const uiSchema = {
  type: 'Control',
  scope: '#/properties/orientation',
};

const renderers = [
  ...vanillaRenderers,
  { tester: orientationTester, renderer: OrientationRenderer },
];

describe('OrientationRenderer', () => {
  it('renders all compass directions', () => {
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

    expect(screen.getByRole('radio', { name: /^N$/i })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /^S$/i })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /^E$/i })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /^W$/i })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /^NE$/i })).toBeInTheDocument();
  });

  it('highlights selected direction', () => {
    render(
      <JsonForms
        schema={schema}
        uischema={uiSchema}
        data={{ orientation: 'SE' }}
        renderers={renderers}
        cells={vanillaCells}
        onChange={vi.fn()}
      />
    );

    const seButton = screen.getByRole('radio', { name: /^SE$/i });
    expect(seButton).toHaveAttribute('aria-checked', 'true');
  });

  it('calls onChange when direction is clicked', async () => {
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

    fireEvent.click(screen.getByRole('radio', { name: /^NW$/i }));

    await vi.waitFor(() => {
      expect(onChange).toHaveBeenCalled();
    });
  });
});
