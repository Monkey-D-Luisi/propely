// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import MyOrgsPage from '../page';
import { ApiError } from '@/lib/api';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/hooks/useRequireAuth', () => ({
  useRequireAuth: vi.fn(),
}));
vi.mock('@/hooks/orgs', () => ({
  useMyOrgs: vi.fn(),
  useCreateOrg: vi.fn(),
}));
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { useMyOrgs, useCreateOrg } from '@/hooks/orgs';
import { useRequireAuth } from '@/hooks/useRequireAuth';

const mockUseMyOrgs = vi.mocked(useMyOrgs);
const mockUseCreateOrg = vi.mocked(useCreateOrg);
const mockUseRequireAuth = vi.mocked(useRequireAuth);

beforeEach(() => {
  vi.resetAllMocks();
  mockUseRequireAuth.mockReturnValue({ user: { id: '1', email: 'test@test.com' } as never, isLoading: false });
  mockUseMyOrgs.mockReturnValue({
    orgs: [],
    pagination: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    isLoading: false,
    error: null,
    refetch: vi.fn(),
  });
  mockUseCreateOrg.mockReturnValue(vi.fn().mockResolvedValue({ id: '1', name: 'Test' }));
});

describe('MyOrgsPage', () => {
  it('shows name-already-exists error on 409 conflict when creating org', async () => {
    const createFn = vi.fn().mockRejectedValue(new ApiError('Conflict', 409));
    mockUseCreateOrg.mockReturnValue(createFn);

    const { user } = renderWithProviders(<MyOrgsPage />);

    // Open the create form
    await user.click(screen.getByRole('button', { name: 'New organization' }));

    // Fill the name field and submit
    const nameInput = screen.getByLabelText('Organization name');
    await user.type(nameInput, 'Duplicate Org');
    await user.click(screen.getByRole('button', { name: 'Create' }));

    // Assert the 409-specific error message
    await waitFor(() => {
      expect(screen.getByText('An organization with this name already exists.')).toBeInTheDocument();
    });
  });

  it('shows generic error on non-409 failure when creating org', async () => {
    const createFn = vi.fn().mockRejectedValue(new Error('Network error'));
    mockUseCreateOrg.mockReturnValue(createFn);

    const { user } = renderWithProviders(<MyOrgsPage />);

    await user.click(screen.getByRole('button', { name: 'New organization' }));

    const nameInput = screen.getByLabelText('Organization name');
    await user.type(nameInput, 'Some Org');
    await user.click(screen.getByRole('button', { name: 'Create' }));

    await waitFor(() => {
      expect(screen.getByText('Failed to create organization.')).toBeInTheDocument();
    });
  });
});
