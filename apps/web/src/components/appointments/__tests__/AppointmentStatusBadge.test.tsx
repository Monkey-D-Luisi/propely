// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { AppointmentStatusBadge } from '../AppointmentStatusBadge';

describe('AppointmentStatusBadge', () => {
  it.each([
    ['Scheduled', 'bg-blue-100', 'text-blue-700'],
    ['Confirmed', 'bg-green-100', 'text-green-700'],
    ['Completed', 'bg-slate-100', 'text-slate-700'],
    ['Cancelled', 'bg-red-100', 'text-red-700'],
    ['NoShow', 'bg-amber-100', 'text-amber-700'],
  ] as const)('renders %s status with correct colors', (status, bgClass, textClass) => {
    renderWithProviders(<AppointmentStatusBadge status={status} label={status} />);
    const badge = screen.getByTestId(`appointment-status-badge-${status}`);
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveTextContent(status);
    expect(badge.className).toContain(bgClass);
    expect(badge.className).toContain(textClass);
  });

  it('renders as a span with rounded-full class', () => {
    renderWithProviders(<AppointmentStatusBadge status="Scheduled" label="Scheduled" />);
    const badge = screen.getByTestId('appointment-status-badge-Scheduled');
    expect(badge.tagName).toBe('SPAN');
    expect(badge.className).toContain('rounded-full');
  });
});
