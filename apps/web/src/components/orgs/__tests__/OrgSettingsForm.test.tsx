// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { OrgSettingsForm } from '@/components/orgs/OrgSettingsForm';
import { ApiError } from '@/lib/api';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/hooks/orgs', () => ({
  useOrg: vi.fn(),
  useUpdateOrg: vi.fn(),
  useDeleteOrg: vi.fn(() => vi.fn()),
}));
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { useOrg, useUpdateOrg } from '@/hooks/orgs';

const mockUseOrg = vi.mocked(useOrg);
const mockUseUpdateOrg = vi.mocked(useUpdateOrg);
const ORG_ID = '00000000-0000-0000-0000-000000000020';

const mockOrg = {
  id: ORG_ID,
  name: 'Test Organization',
  description: 'A test org',
  role: 'owner' as const,
};

beforeEach(() => {
  vi.resetAllMocks();
  mockUseOrg.mockReturnValue({
    org: mockOrg,
    isLoading: false,
    error: null,
    refetch: vi.fn(),
  });
  mockUseUpdateOrg.mockReturnValue(vi.fn().mockResolvedValue({ id: ORG_ID, name: 'Updated' }));
});

describe('OrgSettingsForm', () => {
  it('renders loading state', () => {
    mockUseOrg.mockReturnValue({
      org: null,
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<OrgSettingsForm orgId={ORG_ID} />);

    // Skeleton elements are present (not the form itself)
    expect(screen.queryByLabelText('Organization name')).not.toBeInTheDocument();
  });

  it('renders form with pre-filled org data', () => {
    renderWithProviders(<OrgSettingsForm orgId={ORG_ID} />);

    expect(screen.getByLabelText('Organization name')).toHaveValue('Test Organization');
    expect(screen.getByLabelText('Description (optional)')).toHaveValue('A test org');
    expect(screen.getByRole('button', { name: 'Save changes' })).toBeInTheDocument();
  });

  it('shows settings title and section header', () => {
    renderWithProviders(<OrgSettingsForm orgId={ORG_ID} />);

    expect(screen.getByRole('heading', { name: 'Organization Settings' })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'General Information' })).toBeInTheDocument();
  });

  it('shows validation error for empty name', async () => {
    const { user } = renderWithProviders(<OrgSettingsForm orgId={ORG_ID} />);

    const nameInput = screen.getByLabelText('Organization name');
    await user.clear(nameInput);
    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(screen.getByText('Organization name is required.')).toBeInTheDocument();
    });
  });

  it('submits successfully and shows success toast', async () => {
    const updateFn = vi.fn().mockResolvedValue({ id: ORG_ID, name: 'Updated Org' });
    mockUseUpdateOrg.mockReturnValue(updateFn);

    const { user } = renderWithProviders(<OrgSettingsForm orgId={ORG_ID} />);

    const nameInput = screen.getByLabelText('Organization name');
    await user.clear(nameInput);
    await user.type(nameInput, 'Updated Org');
    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(updateFn).toHaveBeenCalledWith({
        name: 'Updated Org',
        description: 'A test org',
      });
    });

    await waitFor(() => {
      expect(screen.getByText('Settings updated')).toBeInTheDocument();
    });
  });

  it('shows error toast on failure', async () => {
    const updateFn = vi.fn().mockRejectedValue(new Error('Update failed'));
    mockUseUpdateOrg.mockReturnValue(updateFn);

    const { user } = renderWithProviders(<OrgSettingsForm orgId={ORG_ID} />);

    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(screen.getByText("Couldn't update settings")).toBeInTheDocument();
    });
  });

  it('shows name-already-exists error on 409 conflict', async () => {
    const updateFn = vi.fn().mockRejectedValue(new ApiError('Conflict', 409));
    mockUseUpdateOrg.mockReturnValue(updateFn);

    const { user } = renderWithProviders(<OrgSettingsForm orgId={ORG_ID} />);

    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    // The error toast title is always shown for any error
    await waitFor(() => {
      expect(screen.getByText("Couldn't update settings")).toBeInTheDocument();
    });

    // The 409-specific message is shown in the form error alert
    expect(updateFn).toHaveBeenCalled();
    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent('An organization with this name already exists.');
    }, { timeout: 3000 });
  });

  it('disables form fields for non-admin roles', () => {
    mockUseOrg.mockReturnValue({
      org: { ...mockOrg, role: 'agent' as const },
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<OrgSettingsForm orgId={ORG_ID} />);

    expect(screen.getByLabelText('Organization name')).toBeDisabled();
    expect(screen.getByLabelText('Description (optional)')).toBeDisabled();
    expect(screen.queryByRole('button', { name: 'Save changes' })).not.toBeInTheDocument();
  });

  it('shows error state when loading fails', () => {
    mockUseOrg.mockReturnValue({
      org: null,
      isLoading: false,
      error: new Error('Failed to load'),
      refetch: vi.fn(),
    });

    renderWithProviders(<OrgSettingsForm orgId={ORG_ID} />);

    expect(screen.getByText("We couldn't load the organization settings.")).toBeInTheDocument();
  });

});
