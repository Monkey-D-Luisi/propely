// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { AppHeader } from '@/components/layout/AppHeader';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return { ...actual, apiFetch: vi.fn() };
});
vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn(),
  readCsrfTokenFromCookie: vi.fn(),
}));
vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(),
}));
vi.mock('@/hooks/notifications', () => ({
  useNotifications: vi.fn(() => ({
    notifications: [],
    unreadCount: 0,
    isLoading: false,
    error: null,
    refetch: vi.fn(),
    markAsRead: vi.fn(),
    markAllAsRead: vi.fn(),
  })),
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
  usePathname: vi.fn(() => '/'),
}));

import { useCurrentUser } from '@/hooks/orgs';
import { ensureCsrfToken } from '@/lib/csrf';
import { apiFetch } from '@/lib/api';

const mockUseCurrentUser = vi.mocked(useCurrentUser);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);
const mockApiFetch = vi.mocked(apiFetch);

beforeEach(() => {
  vi.resetAllMocks();
  mockEnsureCsrfToken.mockResolvedValue('csrf-token');
});

describe('AppHeader', () => {
  it('renders the app name', () => {
    mockUseCurrentUser.mockReturnValue({ user: null, isLoading: false, error: null });

    renderWithProviders(<AppHeader />);

    expect(screen.getByText('SaaS Starter Kit')).toBeInTheDocument();
  });

  it('shows sign-in link when not authenticated', () => {
    mockUseCurrentUser.mockReturnValue({ user: null, isLoading: false, error: null });

    renderWithProviders(<AppHeader />);

    expect(screen.getByText('Sign in')).toBeInTheDocument();
    expect(screen.queryByText('Sign out')).not.toBeInTheDocument();
  });

  it('shows user name and sign-out button when authenticated', () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: true },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<AppHeader />);

    expect(screen.getByText('Test')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Sign out' })).toBeInTheDocument();
    expect(screen.queryByText('Sign in')).not.toBeInTheDocument();
    expect(screen.queryByText('Please verify your email address to secure your account.')).not.toBeInTheDocument();
  });

  it('shows verification banner when authenticated user is unverified', () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: false },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<AppHeader />);

    expect(screen.getByText('Please verify your email address to secure your account.')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Resend verification email' })).toBeInTheDocument();
  });

  it('does not show user info or sign-in while loading', () => {
    mockUseCurrentUser.mockReturnValue({ user: null, isLoading: true, error: null });

    renderWithProviders(<AppHeader />);

    // Neither sign in nor sign out should show during loading
    expect(screen.queryByText('Sign in')).not.toBeInTheDocument();
    expect(screen.queryByText('Sign out')).not.toBeInTheDocument();
  });

  it('calls logout API when sign-out button is clicked', async () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: true },
      isLoading: false,
      error: null,
    });
    mockApiFetch.mockResolvedValueOnce({});

    const { user } = renderWithProviders(<AppHeader />);

    await user.click(screen.getByRole('button', { name: 'Sign out' }));

    await waitFor(() => {
      expect(mockApiFetch).toHaveBeenCalledWith(
        '/auth/logout',
        expect.objectContaining({ method: 'POST' }),
      );
    });
  });

  it('shows error toast when logout fails', async () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: true },
      isLoading: false,
      error: null,
    });
    mockApiFetch.mockRejectedValueOnce(new Error('Network error'));

    const { user } = renderWithProviders(<AppHeader />);

    await user.click(screen.getByRole('button', { name: 'Sign out' }));

    await waitFor(() => {
      expect(screen.getByText("Couldn't sign out")).toBeInTheDocument();
    });
  });

  it('shows error toast when CSRF token is unavailable for logout', async () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: true },
      isLoading: false,
      error: null,
    });
    mockEnsureCsrfToken.mockResolvedValueOnce(null);

    const { user } = renderWithProviders(<AppHeader />);

    await user.click(screen.getByRole('button', { name: 'Sign out' }));

    await waitFor(() => {
      expect(screen.getByText("Couldn't sign out")).toBeInTheDocument();
    });
  });

  it('shows admin links when user is system admin', () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: '1', email: 'admin@example.com', name: 'Admin', emailVerified: true, isSystemAdmin: true },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<AppHeader />);

    expect(screen.getByText('Feature Flags')).toBeInTheDocument();
    expect(screen.getByText('Audit Logs')).toBeInTheDocument();
    expect(screen.getByText('Version')).toBeInTheDocument();
  });

  it('hides admin links when user is not system admin', () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: true, isSystemAdmin: false },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<AppHeader />);

    expect(screen.queryByText('Feature Flags')).not.toBeInTheDocument();
    expect(screen.queryByText('Audit Logs')).not.toBeInTheDocument();
    expect(screen.queryByText('Version')).not.toBeInTheDocument();
  });
});
