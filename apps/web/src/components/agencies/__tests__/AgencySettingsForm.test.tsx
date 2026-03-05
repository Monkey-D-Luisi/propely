// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { AgencySettingsForm } from '@/components/agencies/AgencySettingsForm';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/hooks/agencies', () => ({
  useAgency: vi.fn(),
  useDeleteAgency: vi.fn(() => vi.fn()),
}));
vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(),
}));
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
  useRouter: vi.fn(() => ({
    push: vi.fn(),
    replace: vi.fn(),
    refresh: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
    prefetch: vi.fn(),
  })),
}));

import { useAgency, useDeleteAgency } from '@/hooks/agencies';
import { useCurrentUser } from '@/hooks/orgs';

const mockUseAgency = vi.mocked(useAgency);
const mockUseCurrentUser = vi.mocked(useCurrentUser);
const mockUseDeleteAgency = vi.mocked(useDeleteAgency);

const AGENCY_ID = '00000000-0000-0000-0000-000000000001';
const USER_ID = '00000000-0000-0000-0000-000000000099';

const mockAgency = {
  id: AGENCY_ID,
  name: 'Test Agency',
  slug: 'test-agency',
  createdByUserId: USER_ID,
  createdAtUtc: '2026-01-01T00:00:00Z',
  updatedAtUtc: null,
  branches: [],
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
  mockUseDeleteAgency.mockReturnValue(vi.fn().mockResolvedValue(undefined));
});

describe('AgencySettingsForm', () => {
  it('renders loading state', () => {
    mockUseAgency.mockReturnValue({
      agency: null,
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    expect(screen.queryByLabelText('Agency name')).not.toBeInTheDocument();
  });

  it('renders error state', () => {
    mockUseAgency.mockReturnValue({
      agency: null,
      isLoading: false,
      error: new Error('Load failed'),
      refetch: vi.fn(),
    });

    renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    expect(screen.getByText("We couldn't load the agency settings.")).toBeInTheDocument();
  });

  it('renders settings form with pre-filled name', () => {
    renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    expect(screen.getByRole('heading', { name: 'Agency settings' })).toBeInTheDocument();
    expect(screen.getByLabelText('Agency name')).toHaveValue('Test Agency');
    expect(screen.getByLabelText('Agency slug')).toHaveValue('test-agency');
  });

  it('shows slug as read-only', () => {
    renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    expect(screen.getByLabelText('Agency slug')).toBeDisabled();
    expect(screen.getByText('The slug cannot be changed after creation.')).toBeInTheDocument();
  });

  it('shows danger zone for agency owner', () => {
    renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    expect(screen.getByText('Danger zone')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Delete agency' })).toBeInTheDocument();
  });

  it('hides danger zone for non-owner', () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: 'other-user-id', email: 'other@test.com', name: 'Other User', emailVerified: true },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    expect(screen.queryByText('Danger zone')).not.toBeInTheDocument();
  });

  it('disables delete button until confirmation matches', () => {
    renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    const deleteButton = screen.getByRole('button', { name: 'Delete agency' });
    expect(deleteButton).toBeDisabled();
  });

  it('enables delete button when confirmation matches agency name', async () => {
    const { user } = renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    const confirmInput = screen.getByPlaceholderText('Test Agency');
    await user.type(confirmInput, 'Test Agency');

    await waitFor(() => {
      const deleteButton = screen.getByRole('button', { name: 'Delete agency' });
      expect(deleteButton).toBeEnabled();
    });
  });

  it('calls deleteAgency when confirmed and clicked', async () => {
    const deleteFn = vi.fn().mockResolvedValue(undefined);
    mockUseDeleteAgency.mockReturnValue(deleteFn);

    const { user } = renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    const confirmInput = screen.getByPlaceholderText('Test Agency');
    await user.type(confirmInput, 'Test Agency');
    await user.click(screen.getByRole('button', { name: 'Delete agency' }));

    await waitFor(() => {
      expect(deleteFn).toHaveBeenCalled();
    });
  });

  it('disables name field for non-owner', () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: 'other-user-id', email: 'other@test.com', name: 'Other User', emailVerified: true },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    expect(screen.getByLabelText('Agency name')).toBeDisabled();
  });

  it('shows back to agency link', () => {
    renderWithProviders(<AgencySettingsForm agencyId={AGENCY_ID} />);

    expect(screen.getByText(/Back to agency/)).toBeInTheDocument();
  });
});
