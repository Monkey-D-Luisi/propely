// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { AgencyDashboard } from '@/components/agencies/AgencyDashboard';
import { renderWithProviders, screen } from '@test/utils';

vi.mock('@/hooks/agencies', () => ({
  useAgency: vi.fn(),
}));
vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(),
}));
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { useAgency } from '@/hooks/agencies';
import { useCurrentUser } from '@/hooks/orgs';

const mockUseAgency = vi.mocked(useAgency);
const mockUseCurrentUser = vi.mocked(useCurrentUser);

const AGENCY_ID = '00000000-0000-0000-0000-000000000001';
const USER_ID = '00000000-0000-0000-0000-000000000099';

const mockAgency = {
  id: AGENCY_ID,
  name: 'Test Agency',
  slug: 'test-agency',
  createdByUserId: USER_ID,
  createdAtUtc: '2026-01-01T00:00:00Z',
  updatedAtUtc: null,
  branches: [
    {
      id: '00000000-0000-0000-0000-000000000010',
      name: 'Madrid Branch',
      memberCount: 5,
      createdAtUtc: '2026-01-15T00:00:00Z',
    },
    {
      id: '00000000-0000-0000-0000-000000000020',
      name: 'Barcelona Branch',
      memberCount: 3,
      createdAtUtc: '2026-02-01T00:00:00Z',
    },
  ],
};

beforeEach(() => {
  vi.resetAllMocks();
  mockUseAgency.mockReturnValue({
    agency: mockAgency,
    isLoading: false,
    error: null,
    refetch: vi.fn(),
  });
  mockUseCurrentUser.mockReturnValue({
    user: { id: USER_ID, email: 'test@test.com', name: 'Test User', emailVerified: true },
    isLoading: false,
    error: null,
  });
});

describe('AgencyDashboard', () => {
  it('renders loading state', () => {
    mockUseAgency.mockReturnValue({
      agency: null,
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<AgencyDashboard agencyId={AGENCY_ID} />);

    // Should show skeleton, not the agency name
    expect(screen.queryByText('Test Agency')).not.toBeInTheDocument();
  });

  it('renders error state', () => {
    mockUseAgency.mockReturnValue({
      agency: null,
      isLoading: false,
      error: new Error('Load failed'),
      refetch: vi.fn(),
    });

    renderWithProviders(<AgencyDashboard agencyId={AGENCY_ID} />);

    expect(screen.getByText("We couldn't load the agency. Please try again.")).toBeInTheDocument();
  });

  it('renders agency name and branches', () => {
    renderWithProviders(<AgencyDashboard agencyId={AGENCY_ID} />);

    expect(screen.getByRole('heading', { level: 1, name: 'Test Agency' })).toBeInTheDocument();
    expect(screen.getByText('Madrid Branch')).toBeInTheDocument();
    expect(screen.getByText('Barcelona Branch')).toBeInTheDocument();
  });

  it('shows settings link for agency owner', () => {
    renderWithProviders(<AgencyDashboard agencyId={AGENCY_ID} />);

    expect(screen.getByRole('link', { name: /Settings/i })).toBeInTheDocument();
  });

  it('hides settings link for non-owner', () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: 'other-user-id', email: 'other@test.com', name: 'Other User', emailVerified: true },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<AgencyDashboard agencyId={AGENCY_ID} />);

    expect(screen.queryByRole('link', { name: /Settings/i })).not.toBeInTheDocument();
  });

  it('shows empty state when no branches', () => {
    mockUseAgency.mockReturnValue({
      agency: { ...mockAgency, branches: [] },
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<AgencyDashboard agencyId={AGENCY_ID} />);

    expect(screen.getByText('No branches yet')).toBeInTheDocument();
  });

  it('shows branch member counts', () => {
    renderWithProviders(<AgencyDashboard agencyId={AGENCY_ID} />);

    expect(screen.getByText('5 members')).toBeInTheDocument();
    expect(screen.getByText('3 members')).toBeInTheDocument();
  });

  it('shows back to organizations link', () => {
    renderWithProviders(<AgencyDashboard agencyId={AGENCY_ID} />);

    expect(screen.getAllByText(/Back to my organizations/).length).toBeGreaterThan(0);
  });
});
