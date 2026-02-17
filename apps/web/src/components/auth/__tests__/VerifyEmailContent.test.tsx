// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { act } from 'react';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { VerifyEmailContent } from '@/components/auth/VerifyEmailContent';
import { useSearchParams } from 'next/navigation';

const mockRouterReplace = vi.hoisted(() => vi.fn());
const mockRouterRefresh = vi.hoisted(() => vi.fn());
const mockRouter = vi.hoisted(() => ({
  push: vi.fn(),
  replace: mockRouterReplace,
  refresh: mockRouterRefresh,
  back: vi.fn(),
  forward: vi.fn(),
  prefetch: vi.fn(),
}));

vi.mock('next/navigation', async () => {
  const actual = await vi.importActual<typeof import('next/navigation')>('next/navigation');
  return {
    ...actual,
    useSearchParams: vi.fn(),
  };
});

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
  useRouter: vi.fn(() => mockRouter),
}));

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return { ...actual, apiFetch: vi.fn() };
});

vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn(),
}));

import { apiFetch, ApiError } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';

const mockUseSearchParams = vi.mocked(useSearchParams);
const mockApiFetch = vi.mocked(apiFetch);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);

describe('VerifyEmailContent', () => {
  beforeEach(() => {
    vi.useRealTimers();
    vi.resetAllMocks();
    mockRouter.push.mockReset();
    mockRouterReplace.mockReset();
    mockRouterRefresh.mockReset();
    mockRouter.back.mockReset();
    mockRouter.forward.mockReset();
    mockRouter.prefetch.mockReset();
    mockEnsureCsrfToken.mockResolvedValue('csrf-token-123');
    mockUseSearchParams.mockReturnValue(new URLSearchParams('token=valid-token'));
  });

  it('shows missing-token error when token query param is absent', async () => {
    mockUseSearchParams.mockReturnValue(new URLSearchParams());
    renderWithProviders(<VerifyEmailContent />);

    await waitFor(() => {
      expect(screen.getByText('Email verification failed')).toBeInTheDocument();
    });
    expect(screen.getByText('The verification link is missing a token.')).toBeInTheDocument();
  });

  it('submits token and shows success state when verification succeeds', async () => {
    mockApiFetch.mockResolvedValueOnce({ ok: true });
    renderWithProviders(<VerifyEmailContent />);

    await waitFor(() => {
      expect(mockApiFetch).toHaveBeenCalledWith(
        '/auth/verify-email',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({ token: 'valid-token' }),
        }),
      );
    });

    await waitFor(() => {
      expect(screen.getByText('Email verified')).toBeInTheDocument();
    });
  });

  it('redirects to home after success when user session is active', async () => {
    vi.useFakeTimers();
    mockApiFetch.mockImplementation(async (path: string) => {
      if (path === '/auth/verify-email') {
        return { ok: true };
      }

      if (path === '/auth/me') {
        return { user: { id: '1', email: 'test@example.com' } };
      }

      throw new Error(`Unexpected path: ${path}`);
    });

    renderWithProviders(<VerifyEmailContent />);

    await act(async () => {
      await Promise.resolve();
      await Promise.resolve();
    });

    expect(screen.getByText('Email verified')).toBeInTheDocument();

    await act(async () => {
      vi.advanceTimersByTime(1300);
    });

    expect(mockRouterReplace).toHaveBeenLastCalledWith('/');
    vi.useRealTimers();
  });

  it('redirects to login after success when user session is not active', async () => {
    vi.useFakeTimers();
    mockApiFetch.mockImplementation(async (path: string) => {
      if (path === '/auth/verify-email') {
        return { ok: true };
      }

      if (path === '/auth/me') {
        throw new ApiError('Unauthorized', 401);
      }

      throw new Error(`Unexpected path: ${path}`);
    });

    renderWithProviders(<VerifyEmailContent />);

    await act(async () => {
      await Promise.resolve();
      await Promise.resolve();
    });

    expect(screen.getByText('Email verified')).toBeInTheDocument();

    await act(async () => {
      vi.advanceTimersByTime(1300);
    });

    expect(mockRouterReplace).toHaveBeenLastCalledWith('/login');
    vi.useRealTimers();
  });

  it('shows expired/invalid message when backend returns token error', async () => {
    mockApiFetch.mockRejectedValue(
      new ApiError('Bad Request', 400, { detail: 'INVALID_OR_EXPIRED_VERIFICATION_TOKEN' }),
    );
    renderWithProviders(<VerifyEmailContent />);

    await waitFor(() => {
      expect(screen.getByText('Email verification failed')).toBeInTheDocument();
    });
    expect(
      screen.getByText('The verification link is invalid or expired. Request a new email.'),
    ).toBeInTheDocument();
  });

  it('shows already verified feedback when resend returns alreadyVerified', async () => {
    mockApiFetch.mockImplementation(async (path: string) => {
      if (path === '/auth/verify-email') {
        throw new ApiError('Bad Request', 400, { detail: 'INVALID_OR_EXPIRED_VERIFICATION_TOKEN' });
      }

      if (path === '/auth/resend-verification') {
        return { ok: true, sent: false, alreadyVerified: true };
      }

      throw new Error(`Unexpected path: ${path}`);
    });

    const { user } = renderWithProviders(<VerifyEmailContent />);

    await waitFor(() => {
      expect(screen.getByText('Email verification failed')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Resend verification email' }));

    await waitFor(() => {
      expect(mockApiFetch).toHaveBeenCalledWith(
        '/auth/resend-verification',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({ locale: 'en' }),
        }),
      );
    });

    expect(screen.getByText('Your email is already verified.')).toBeInTheDocument();
  });

  it('redirects to login when resend returns unauthorized', async () => {
    mockApiFetch.mockImplementation(async (path: string) => {
      if (path === '/auth/verify-email') {
        throw new ApiError('Bad Request', 400, { detail: 'INVALID_OR_EXPIRED_VERIFICATION_TOKEN' });
      }

      if (path === '/auth/resend-verification') {
        throw new ApiError('Unauthorized', 401);
      }

      throw new Error(`Unexpected path: ${path}`);
    });

    const { user } = renderWithProviders(<VerifyEmailContent />);

    await waitFor(() => {
      expect(screen.getByText('Email verification failed')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: 'Resend verification email' }));

    await waitFor(() => {
      expect(mockRouterReplace).toHaveBeenCalledWith('/login');
    });
  });
});
