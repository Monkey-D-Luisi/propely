// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { QuickActions } from '@/components/dashboard/QuickActions';

vi.mock('next-intl', async () => {
  const actual = await vi.importActual('next-intl');
  return {
    ...actual,
    useTranslations: () => (key: string) => {
      const translations: Record<string, string> = {
        quickActions: 'Quick Actions',
        newProperty: 'New Property',
        newLead: 'New Lead',
        scheduleAppointment: 'Schedule Appointment',
      };
      return translations[key] ?? key;
    },
  };
});

describe('QuickActions', () => {
  it('renders three action buttons', () => {
    renderWithProviders(<QuickActions />);

    expect(screen.getByText('New Property')).toBeInTheDocument();
    expect(screen.getByText('New Lead')).toBeInTheDocument();
    expect(screen.getByText('Schedule Appointment')).toBeInTheDocument();
  });

  it('renders section title', () => {
    renderWithProviders(<QuickActions />);

    expect(screen.getByText('Quick Actions')).toBeInTheDocument();
  });

  it('renders with testid', () => {
    renderWithProviders(<QuickActions />);

    expect(screen.getByTestId('quick-actions')).toBeInTheDocument();
  });
});
