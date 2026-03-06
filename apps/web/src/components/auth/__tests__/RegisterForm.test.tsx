// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { RegisterForm } from '@/components/auth/RegisterForm';
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

import { apiFetch, ApiError } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { useCurrentUser } from '@/hooks/orgs';

const mockApiFetch = vi.mocked(apiFetch);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);
const mockUseCurrentUser = vi.mocked(useCurrentUser);
const mockUseRouter = vi.mocked(useRouter);
const mockUseSearchParams = vi.mocked(useSearchParams);

beforeEach(() => {
  vi.resetAllMocks();
  mockUseCurrentUser.mockReturnValue({ user: null, isLoading: false, error: null });
  mockEnsureCsrfToken.mockResolvedValue('csrf-token-123');
  mockUseSearchParams.mockReturnValue(new URLSearchParams());
  mockUseRouter.mockReturnValue({
    push: vi.fn(),
    replace: vi.fn(),
    refresh: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
    prefetch: vi.fn(),
  });
});

describe('RegisterForm', () => {
  it('renders the form with name, email, password, and confirm password fields', async () => {
    renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByLabelText(/name/i)).toBeInTheDocument();
    });
    expect(screen.getByLabelText('Email')).toBeInTheDocument();
    expect(screen.getByLabelText(/^Password$/)).toBeInTheDocument();
    expect(screen.getByLabelText('Confirm password')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Create account' })).toBeInTheDocument();
  });

  it('renders OAuth provider buttons', async () => {
    renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Google' })).toBeInTheDocument();
    });
    expect(screen.getByRole('button', { name: 'GitHub' })).toBeInTheDocument();
  });

  it('renders the brand heading and link to login', async () => {
    renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Propely' })).toBeInTheDocument();
    });
    expect(screen.getByText('Sign in')).toBeInTheDocument();
  });

  it('shows validation errors for empty required fields', async () => {
    const { user } = renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Create account' }));

    await waitFor(() => {
      expect(screen.getByText('Email is required.')).toBeInTheDocument();
    });
    expect(screen.getByText('Password is required.')).toBeInTheDocument();
    expect(screen.getByText('Confirm password is required.')).toBeInTheDocument();
  });

  it('shows validation error for short password', async () => {
    const { user } = renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText(/^Password$/), 'short');
    await user.type(screen.getByLabelText('Confirm password'), 'short');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    await waitFor(() => {
      expect(screen.getByText(/at least 8 characters/i)).toBeInTheDocument();
    });
  });

  it('shows validation error when passwords do not match', async () => {
    const { user } = renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText(/^Password$/), 'password123');
    await user.type(screen.getByLabelText('Confirm password'), 'different123');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    await waitFor(() => {
      expect(screen.getByText('Passwords do not match.')).toBeInTheDocument();
    });
  });

  it('submits the form with valid data', async () => {
    mockApiFetch.mockResolvedValueOnce({});
    const locationAssignSpy = vi.spyOn(window.location, 'assign').mockImplementation(() => {});
    const { user } = renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText(/name/i), 'Test User');
    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText(/^Password$/), 'password123');
    await user.type(screen.getByLabelText('Confirm password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    await waitFor(() => {
      expect(mockApiFetch).toHaveBeenCalledWith(
        '/auth/register',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({
            email: 'test@example.com',
            password: 'password123',
            name: 'Test User',
            locale: 'en',
          }),
        }),
      );
    });

    await waitFor(() => {
      expect(locationAssignSpy).toHaveBeenCalledWith('/');
    });
  });

  it('redirects to next path after successful register when next query param exists', async () => {
    mockUseSearchParams.mockReturnValue(new URLSearchParams('next=%2Fen%2Forgs%2Fmine'));
    mockApiFetch.mockResolvedValueOnce({});
    const locationAssignSpy = vi.spyOn(window.location, 'assign').mockImplementation(() => {});
    const { user } = renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText(/^Password$/), 'password123');
    await user.type(screen.getByLabelText('Confirm password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    await waitFor(() => {
      expect(locationAssignSpy).toHaveBeenCalledWith('/en/orgs/mine');
    });
  });

  it('shows error when email already exists (409)', async () => {
    mockApiFetch.mockRejectedValueOnce(new ApiError('Conflict', 409));
    const { user } = renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'existing@example.com');
    await user.type(screen.getByLabelText(/^Password$/), 'password123');
    await user.type(screen.getByLabelText('Confirm password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    await waitFor(() => {
      expect(screen.getByText(/already exists/i)).toBeInTheDocument();
    });
  });

  it('shows CSRF error on 403', async () => {
    mockApiFetch.mockRejectedValueOnce(new ApiError('Forbidden', 403));
    const { user } = renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText(/^Password$/), 'password123');
    await user.type(screen.getByLabelText('Confirm password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    await waitFor(() => {
      expect(screen.getByText(/couldn't verify your session/i)).toBeInTheDocument();
    });
  });

  it('shows rate limit error on 429 with retry-after seconds', async () => {
    mockApiFetch.mockRejectedValueOnce(new ApiError('Too many requests', 429, undefined, 35));
    const { user } = renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText(/^Password$/), 'password123');
    await user.type(screen.getByLabelText('Confirm password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    await waitFor(() => {
      expect(screen.getByText('Too many attempts. Please try again in 35 seconds.')).toBeInTheDocument();
    });
  });

  it('shows generic error on unknown failure', async () => {
    mockApiFetch.mockRejectedValueOnce(new ApiError('Server Error', 500));
    const { user } = renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.type(screen.getByLabelText(/^Password$/), 'password123');
    await user.type(screen.getByLabelText('Confirm password'), 'password123');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    await waitFor(() => {
      expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
    });
  });

  it('disables submit when CSRF token is not available', async () => {
    mockEnsureCsrfToken.mockResolvedValueOnce(null);
    renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Create account' })).toBeDisabled();
    });
  });

  it('redirects already-authenticated user', async () => {
    const replace = vi.fn();
    mockUseRouter.mockReturnValue({
      push: vi.fn(),
      replace,
      refresh: vi.fn(),
      back: vi.fn(),
      forward: vi.fn(),
      prefetch: vi.fn(),
    });
    mockUseCurrentUser.mockReturnValue({
      user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: true },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(replace).toHaveBeenCalledWith('/');
    });
  });

  it('redirects already-authenticated user to next path when provided', async () => {
    const replace = vi.fn();
    mockUseSearchParams.mockReturnValue(new URLSearchParams('next=%2Fen%2Forgs%2Fmine'));
    mockUseRouter.mockReturnValue({
      push: vi.fn(),
      replace,
      refresh: vi.fn(),
      back: vi.fn(),
      forward: vi.fn(),
      prefetch: vi.fn(),
    });
    mockUseCurrentUser.mockReturnValue({
      user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: true },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(replace).toHaveBeenCalledWith('/en/orgs/mine');
    });
  });

  it('shows OAuth error message when oauthError query param is present', async () => {
    mockUseSearchParams.mockReturnValue(new URLSearchParams('oauthError=email_not_verified'));
    renderWithProviders(<RegisterForm />);

    await waitFor(() => {
      expect(screen.getByText('Social login failed. Please try again.')).toBeInTheDocument();
    });
  });
});
