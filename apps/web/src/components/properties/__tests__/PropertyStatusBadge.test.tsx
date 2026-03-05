// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { renderWithProviders, screen } from '@test/utils';
import { PropertyStatusBadge } from '../PropertyStatusBadge';

describe('PropertyStatusBadge', () => {
  it.each([
    ['Draft', 'bg-slate-100', 'text-slate-600'],
    ['Active', 'bg-green-100', 'text-green-700'],
    ['Reserved', 'bg-amber-100', 'text-amber-700'],
    ['Sold', 'bg-blue-100', 'text-blue-700'],
    ['Rented', 'bg-purple-100', 'text-purple-700'],
    ['Archived', 'bg-red-100', 'text-red-700'],
  ] as const)('renders %s status with correct colors', (status, bgClass, textClass) => {
    renderWithProviders(<PropertyStatusBadge status={status} label={status} />);
    const badge = screen.getByTestId(`status-badge-${status}`);
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveTextContent(status);
    expect(badge.className).toContain(bgClass);
    expect(badge.className).toContain(textClass);
  });
});
