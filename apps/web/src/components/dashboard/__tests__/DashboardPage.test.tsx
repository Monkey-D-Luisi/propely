// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import DashboardPage from '@/app/[locale]/(dashboard)/page';

vi.mock('@/hooks/use-dashboard', () => ({
  useDashboard: vi.fn(),
}));

vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(),
}));

import { useDashboard } from '@/hooks/use-dashboard';
import { useCurrentUser } from '@/hooks/orgs';

const mockUseDashboard = vi.mocked(useDashboard);
const mockUseCurrentUser = vi.mocked(useCurrentUser);

const defaultDashboardData = {
  properties: { byStatus: { Draft: 3, Active: 10, Reserved: 2, Sold: 5 } },
  leads: { byStatus: { New: 8, Contacted: 4, Qualified: 3, Converted: 2, Lost: 1 } },
  appointments: { byStatus: { Scheduled: 3, Confirmed: 2 }, upcoming: 7 },
  isLoading: false,
  error: null,
};

beforeEach(() => {
  vi.resetAllMocks();
  mockUseDashboard.mockReturnValue(defaultDashboardData);
  mockUseCurrentUser.mockReturnValue({
    user: { id: '1', name: 'María García', email: 'maria@test.com' },
    isLoading: false,
    error: null,
  } as ReturnType<typeof useCurrentUser>);
});

describe('DashboardPage', () => {
  it('renders page title', () => {
    renderWithProviders(<DashboardPage />);

    expect(screen.getByText('Dashboard')).toBeInTheDocument();
  });

  it('renders welcome message with first name', () => {
    renderWithProviders(<DashboardPage />);

    expect(screen.getByText(/María/)).toBeInTheDocument();
  });

  it('renders KPI cards with correct values', () => {
    renderWithProviders(<DashboardPage />);

    expect(screen.getAllByTestId('kpi-card')).toHaveLength(4);
    // Active Listings = Active count = 10
    expect(screen.getByText('10')).toBeInTheDocument();
    // Open Leads = New + Contacted + Qualified = 15
    expect(screen.getByText('15')).toBeInTheDocument();
    // Upcoming = 7
    expect(screen.getByText('7')).toBeInTheDocument();
    // Converted = 2
    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('renders charts', () => {
    renderWithProviders(<DashboardPage />);

    expect(screen.getByTestId('status-pie-chart')).toBeInTheDocument();
    expect(screen.getByTestId('status-bar-chart')).toBeInTheDocument();
  });

  it('renders quick actions', () => {
    renderWithProviders(<DashboardPage />);

    expect(screen.getByTestId('quick-actions')).toBeInTheDocument();
  });

  it('shows loading skeleton when data is loading', () => {
    mockUseDashboard.mockReturnValue({
      properties: null,
      leads: null,
      appointments: null,
      isLoading: true,
      error: null,
    });

    renderWithProviders(<DashboardPage />);

    // Should NOT render KPI cards during loading
    expect(screen.queryAllByTestId('kpi-card')).toHaveLength(0);
  });

  it('handles null data gracefully', () => {
    mockUseDashboard.mockReturnValue({
      properties: null,
      leads: null,
      appointments: null,
      isLoading: false,
      error: null,
    });

    renderWithProviders(<DashboardPage />);

    // Should render with zero values
    expect(screen.getAllByTestId('kpi-card')).toHaveLength(4);
  });
});
