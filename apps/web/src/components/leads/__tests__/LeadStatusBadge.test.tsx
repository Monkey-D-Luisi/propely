// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { LeadStatusBadge } from '../LeadStatusBadge';

describe('LeadStatusBadge', () => {
  it.each([
    ['New', 'bg-blue-100', 'text-blue-700'],
    ['Contacted', 'bg-amber-100', 'text-amber-700'],
    ['Qualified', 'bg-green-100', 'text-green-700'],
    ['Converted', 'bg-purple-100', 'text-purple-700'],
    ['Lost', 'bg-red-100', 'text-red-700'],
  ] as const)('renders %s status with correct colors', (status, bgClass, textClass) => {
    renderWithProviders(<LeadStatusBadge status={status} label={status} />);
    const badge = screen.getByTestId(`lead-status-badge-${status}`);
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveTextContent(status);
    expect(badge.className).toContain(bgClass);
    expect(badge.className).toContain(textClass);
  });

  it('renders as a span with rounded-full class', () => {
    renderWithProviders(<LeadStatusBadge status="New" label="New" />);
    const badge = screen.getByTestId('lead-status-badge-New');
    expect(badge.tagName).toBe('SPAN');
    expect(badge.className).toContain('rounded-full');
  });
});
