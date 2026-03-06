// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { ContactRoleBadge } from '../ContactRoleBadge';
import type { ContactRole } from '@/hooks/useContacts';

describe('ContactRoleBadge', () => {
  it.each([
    ['Buyer', 'bg-blue-100', 'text-blue-700'],
    ['Seller', 'bg-green-100', 'text-green-700'],
    ['Tenant', 'bg-purple-100', 'text-purple-700'],
    ['Landlord', 'bg-amber-100', 'text-amber-700'],
    ['Professional', 'bg-slate-100', 'text-slate-600'],
  ] as const)('renders %s role with correct colors', (role, bgClass, textClass) => {
    renderWithProviders(<ContactRoleBadge role={role} label={role} />);
    const badge = screen.getByTestId(`role-badge-${role}`);
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveTextContent(role);
    expect(badge.className).toContain(bgClass);
    expect(badge.className).toContain(textClass);
  });

  it('renders as a span with rounded-full class', () => {
    renderWithProviders(<ContactRoleBadge role="Buyer" label="Buyer" />);
    const badge = screen.getByTestId('role-badge-Buyer');
    expect(badge.tagName).toBe('SPAN');
    expect(badge.className).toContain('rounded-full');
  });
});
