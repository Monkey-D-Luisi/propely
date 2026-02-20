// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LoginForm } from '@/components/auth/LoginForm';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { useRouter, useSearchParams } from 'next/navigation';

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return { ...actual, apiFetch: vi.fn() };
});
vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn(),
  readCsrfTokenFromCookie: vi.fn(),
}));
vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(() => ({ user: null, isLoading: false, error: null })),
}));
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { apiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { useCurrentUser } from '@/hooks/orgs';
import { ApiError } from '@/lib/api';

const mockApiFetch = vi.mocked(apiFetch);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);
const mockUseCurrentUser = vi.mocked(useCurrentUser);
const mockUseRouter = vi.mocked(useRouter);
const mockUseSearchParams = vi.mocked(useSearchParams);

beforeEach(() => {
  vi.resetAllMocks();
  mockUseCurrentUser.mockReturnValue({ user: null, isLoading: false, error: null });
  mockEnsureCsrfToken.mockResolvedValue('csrf-token-123');
  mockUseRouter.mockReturnValue({
    push: vi.fn(),
    replace: vi.fn(),
    refresh: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
    prefetch: vi.fn(),
  });
  mockUseSearchParams.mockReturnValue(new URLSearchParams());
});

describe('LoginForm', () => {
  it('renders the form with email and password fields', async () => {
    renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });
    expect(screen.getByLabelText('Password')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Sign in' })).toBeInTheDocument();
  });

  it('renders OAuth provider buttons', async () => {
    renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Google' })).toBeInTheDocument();
    });
    expect(screen.getByRole('button', { name: 'GitHub' })).toBeInTheDocument();
  });

  it('renders the sign-in heading', async () => {
    renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Sign in' })).toBeInTheDocument();
    });
  });

  it('renders link to register page', async () => {
    renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByText('Create account')).toBeInTheDocument();
    });
  });

  it('renders forgot password link', async () => {
    renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByRole('link', { name: 'Forgot password?' })).toBeInTheDocument();
    });

    expect(screen.getByRole('link', { name: 'Forgot password?' })).toHaveAttribute('href', '/forgot-password');
  });

  it('shows validation errors for empty fields on submit', async () => {
    const { user } = renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    await waitFor(() => {
      expect(screen.getByText('Email is required.')).toBeInTheDocument();
    });
    expect(screen.getByText('Password is required.')).toBeInTheDocument();
  });

  it('shows validation error for invalid email', async () => {
    const { user } = renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'not-an-email');
    await user.type(screen.getByLabelText('Password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    await waitFor(() => {
      expect(screen.getByText('Enter a valid email address.')).toBeInTheDocument();
    });
  });

  it('submits the form with valid data', async () => {
    mockApiFetch.mockResolvedValueOnce({});
    const { user } = renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText('Password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    await waitFor(() => {
      expect(mockApiFetch).toHaveBeenCalledWith(
        '/auth/login',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({ email: 'test@example.com', password: 'password123' }),
        }),
      );
    });
  });

  it('shows error on invalid credentials (401)', async () => {
    mockApiFetch.mockRejectedValueOnce(new ApiError('Unauthorized', 401));
    const { user } = renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText('Password'), 'wrongpassword');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    await waitFor(() => {
      expect(screen.getByText(/could not find a user/i)).toBeInTheDocument();
    });
  });

  it('shows CSRF error on 403', async () => {
    mockApiFetch.mockRejectedValueOnce(new ApiError('Forbidden', 403));
    const { user } = renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText('Password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    await waitFor(() => {
      expect(screen.getByText(/couldn't verify your session/i)).toBeInTheDocument();
    });
  });

  it('shows rate limit error on 429 with retry-after seconds', async () => {
    mockApiFetch.mockRejectedValueOnce(new ApiError('Too many requests', 429, undefined, 25));
    const { user } = renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText('Password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    await waitFor(() => {
      expect(
        screen.getByText('Too many attempts. Please try again in 25 seconds.'),
      ).toBeInTheDocument();
    });
  });

  it('shows generic error on unknown API error', async () => {
    mockApiFetch.mockRejectedValueOnce(new ApiError('Server Error', 500));
    const { user } = renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText('Password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    await waitFor(() => {
      expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
    });
  });

  it('disables submit button when CSRF token is not available', async () => {
    mockEnsureCsrfToken.mockResolvedValueOnce(null);
    renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Sign in' })).toBeDisabled();
    });
  });

  it('redirects already-authenticated user', async () => {
    const replace = vi.fn();
    const refresh = vi.fn();
    mockUseRouter.mockReturnValue({
      push: vi.fn(),
      replace,
      refresh,
      back: vi.fn(),
      forward: vi.fn(),
      prefetch: vi.fn(),
    });
    mockUseCurrentUser.mockReturnValue({
      user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: true },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(replace).toHaveBeenCalledWith('/');
    });
  });

  it('shows OAuth error message when oauthError query param is present', async () => {
    mockUseSearchParams.mockReturnValue(new URLSearchParams('oauthError=email_not_verified'));
    renderWithProviders(<LoginForm />);

    await waitFor(() => {
      expect(screen.getByText('Social login failed. Please try again.')).toBeInTheDocument();
    });
  });
});
