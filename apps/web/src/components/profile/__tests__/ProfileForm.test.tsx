// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ProfileForm } from '@/components/profile/ProfileForm';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(),
  useUpdateProfile: vi.fn(),
}));
vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn(),
}));
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { useCurrentUser, useUpdateProfile } from '@/hooks/orgs';
import { ensureCsrfToken } from '@/lib/csrf';

const mockUseCurrentUser = vi.mocked(useCurrentUser);
const mockUseUpdateProfile = vi.mocked(useUpdateProfile);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);

const mockUser = {
  id: '00000000-0000-0000-0000-000000000001',
  email: 'test@example.com',
  name: 'Test User',
  emailVerified: false,
};

beforeEach(() => {
  vi.resetAllMocks();
  mockEnsureCsrfToken.mockResolvedValue('test-csrf-token');
  mockUseCurrentUser.mockReturnValue({
    user: mockUser,
    isLoading: false,
    error: null,
  });
  mockUseUpdateProfile.mockReturnValue(vi.fn().mockResolvedValue({ id: mockUser.id, email: mockUser.email, name: 'Updated' }));
});

describe('ProfileForm', () => {
  it('renders loading state', () => {
    mockUseCurrentUser.mockReturnValue({
      user: null,
      isLoading: true,
      error: null,
    });

    renderWithProviders(<ProfileForm />);

    expect(screen.queryByLabelText('Name')).not.toBeInTheDocument();
  });

  it('renders form with pre-filled user data', () => {
    renderWithProviders(<ProfileForm />);

    expect(screen.getByLabelText('Name')).toHaveValue('Test User');
    expect(screen.getByRole('button', { name: 'Save changes' })).toBeInTheDocument();
  });

  it('shows email as disabled field', () => {
    renderWithProviders(<ProfileForm />);

    const emailInput = screen.getByDisplayValue('test@example.com');
    expect(emailInput).toBeDisabled();
  });

  it('shows profile title', () => {
    renderWithProviders(<ProfileForm />);

    expect(screen.getByRole('heading', { name: 'Profile' })).toBeInTheDocument();
  });

  it('submits successfully and shows success toast', async () => {
    const updateFn = vi.fn().mockResolvedValue({ id: mockUser.id, email: mockUser.email, name: 'New Name' });
    mockUseUpdateProfile.mockReturnValue(updateFn);

    const { user } = renderWithProviders(<ProfileForm />);

    const nameInput = screen.getByLabelText('Name');
    await user.clear(nameInput);
    await user.type(nameInput, 'New Name');
    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(updateFn).toHaveBeenCalledWith({ name: 'New Name' }, 'test-csrf-token');
    });

    await waitFor(() => {
      expect(screen.getByText('Profile updated')).toBeInTheDocument();
    });
  });

  it('shows error toast on failure', async () => {
    const updateFn = vi.fn().mockRejectedValue(new Error('Update failed'));
    mockUseUpdateProfile.mockReturnValue(updateFn);

    const { user } = renderWithProviders(<ProfileForm />);

    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(screen.getByText("Couldn't update profile")).toBeInTheDocument();
    });
  });

  it('shows error state when loading fails', () => {
    mockUseCurrentUser.mockReturnValue({
      user: null,
      isLoading: false,
      error: new Error('Failed to load'),
    });

    renderWithProviders(<ProfileForm />);

    expect(screen.getByText("We couldn't load your profile.")).toBeInTheDocument();
  });
});
