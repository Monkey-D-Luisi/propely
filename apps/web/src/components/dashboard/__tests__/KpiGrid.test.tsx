// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { KpiGrid, KpiCard } from '@/components/dashboard/KpiGrid';

vi.mock('next-intl', async () => {
  const actual = await vi.importActual('next-intl');
  return {
    ...actual,
    useTranslations: () => (key: string) => {
      const translations: Record<string, string> = {
        activeListings: 'Active Listings',
        openLeads: 'Open Leads',
        upcomingAppointments: 'Upcoming Appointments',
        convertedThisMonth: 'Converted This Month',
      };
      return translations[key] ?? key;
    },
  };
});

describe('KpiCard', () => {
  it('renders value and label', () => {
    renderWithProviders(
      <KpiCard
        icon="home"
        value={24}
        label="Active Listings"
        accentColor="border-primary-600"
        iconBg="bg-primary-600/10 text-primary-600"
      />,
    );

    expect(screen.getByText('24')).toBeInTheDocument();
    expect(screen.getByText('Active Listings')).toBeInTheDocument();
    expect(screen.getByText('home')).toBeInTheDocument();
  });

  it('renders with testid', () => {
    renderWithProviders(
      <KpiCard
        icon="home"
        value={0}
        label="Test"
        accentColor="border-primary-600"
        iconBg="bg-primary-600/10"
      />,
    );

    expect(screen.getByTestId('kpi-card')).toBeInTheDocument();
  });
});

describe('KpiGrid', () => {
  it('renders four KPI cards', () => {
    renderWithProviders(
      <KpiGrid
        activeListings={24}
        openLeads={18}
        upcomingAppointments={7}
        convertedThisMonth={5}
      />,
    );

    expect(screen.getAllByTestId('kpi-card')).toHaveLength(4);
    expect(screen.getByText('24')).toBeInTheDocument();
    expect(screen.getByText('18')).toBeInTheDocument();
    expect(screen.getByText('7')).toBeInTheDocument();
    expect(screen.getByText('5')).toBeInTheDocument();
  });

  it('renders zero values', () => {
    renderWithProviders(
      <KpiGrid
        activeListings={0}
        openLeads={0}
        upcomingAppointments={0}
        convertedThisMonth={0}
      />,
    );

    const zeros = screen.getAllByText('0');
    expect(zeros).toHaveLength(4);
  });
});
