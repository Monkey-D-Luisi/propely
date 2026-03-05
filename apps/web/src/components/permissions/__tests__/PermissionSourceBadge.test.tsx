// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect } from 'vitest';
import { PermissionSourceBadge } from '@/components/permissions/PermissionSourceBadge';
import { renderWithProviders, screen } from '@test/utils';

describe('PermissionSourceBadge', () => {
  it('renders "Role Default" for non-override source', () => {
    renderWithProviders(<PermissionSourceBadge source="Role Default" />);

    expect(screen.getByText('Role Default')).toBeInTheDocument();
  });

  it('renders "Override" for override source', () => {
    renderWithProviders(<PermissionSourceBadge source="Override" />);

    expect(screen.getByText('Override')).toBeInTheDocument();
  });

  it('applies slate styling for role default', () => {
    renderWithProviders(<PermissionSourceBadge source="Role Default" />);

    const badge = screen.getByText('Role Default');
    expect(badge.className).toContain('bg-slate-100');
    expect(badge.className).toContain('text-slate-600');
  });

  it('applies amber styling for override', () => {
    renderWithProviders(<PermissionSourceBadge source="Override" />);

    const badge = screen.getByText('Override');
    expect(badge.className).toContain('bg-amber-50');
    expect(badge.className).toContain('text-amber-700');
  });
});
