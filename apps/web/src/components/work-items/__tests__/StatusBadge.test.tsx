// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect } from 'vitest';
import { StatusBadge } from '@/components/work-items/StatusBadge';
import { renderWithProviders, screen } from '@test/utils';
import type { WorkItemStatusType } from '@/lib/schemas';

describe('StatusBadge', () => {
  it('renders Pending status with correct text and styles', () => {
    renderWithProviders(<StatusBadge status="Pending" />);

    const badge = screen.getByText('Pending');
    expect(badge).toBeInTheDocument();
    expect(badge.className).toContain('bg-amber-50');
    expect(badge.className).toContain('text-amber-700');
    expect(badge.className).toContain('border-amber-200');
  });

  it('renders Active status with correct text and styles', () => {
    renderWithProviders(<StatusBadge status="Active" />);

    const badge = screen.getByText('Active');
    expect(badge).toBeInTheDocument();
    expect(badge.className).toContain('bg-emerald-50');
    expect(badge.className).toContain('text-emerald-700');
    expect(badge.className).toContain('border-emerald-200');
  });

  it('renders Deactivated status with correct text and styles', () => {
    renderWithProviders(<StatusBadge status="Deactivated" />);

    const badge = screen.getByText('Deactivated');
    expect(badge).toBeInTheDocument();
    expect(badge.className).toContain('bg-slate-50');
    expect(badge.className).toContain('text-slate-600');
    expect(badge.className).toContain('border-slate-200');
  });

  it('renders Expired status with correct text and styles', () => {
    renderWithProviders(<StatusBadge status="Expired" />);

    const badge = screen.getByText('Expired');
    expect(badge).toBeInTheDocument();
    expect(badge.className).toContain('bg-red-50');
    expect(badge.className).toContain('text-red-700');
    expect(badge.className).toContain('border-red-200');
  });

  it('falls back to Pending style for unknown status', () => {
    renderWithProviders(<StatusBadge status={'Unknown' as WorkItemStatusType} />);

    const badge = screen.getByText('Unknown');
    expect(badge).toBeInTheDocument();
    // Falls back to Pending styles
    expect(badge.className).toContain('bg-amber-50');
    expect(badge.className).toContain('text-amber-700');
    expect(badge.className).toContain('border-amber-200');
  });

  it('renders as a span with rounded-full class', () => {
    renderWithProviders(<StatusBadge status="Active" />);

    const badge = screen.getByText('Active');
    expect(badge.tagName).toBe('SPAN');
    expect(badge.className).toContain('rounded-full');
  });
});
