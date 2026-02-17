// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ResetPasswordForm } from '@/components/auth/ResetPasswordForm';
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
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { apiFetch, ApiError } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';

const mockApiFetch = vi.mocked(apiFetch);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);
const mockUseRouter = vi.mocked(useRouter);
const mockUseSearchParams = vi.mocked(useSearchParams);

describe('ResetPasswordForm', () => {
  const replace = vi.fn();
  const refresh = vi.fn();

  beforeEach(() => {
    vi.resetAllMocks();
    mockEnsureCsrfToken.mockResolvedValue('csrf-token-123');
    mockUseRouter.mockReturnValue({
      push: vi.fn(),
      replace,
      refresh,
      back: vi.fn(),
      forward: vi.fn(),
      prefetch: vi.fn(),
    });
    mockUseSearchParams.mockReturnValue(new URLSearchParams('token=reset-token-123'));
  });

  it('renders reset password form', async () => {
    renderWithProviders(<ResetPasswordForm />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Reset password' })).toBeInTheDocument();
    });

    expect(screen.getByLabelText('New password')).toBeInTheDocument();
    expect(screen.getByLabelText('Confirm password')).toBeInTheDocument();
  });

  it('shows missing token message when token is absent', async () => {
    mockUseSearchParams.mockReturnValue(new URLSearchParams());
    renderWithProviders(<ResetPasswordForm />);

    await waitFor(() => {
      expect(screen.getByText('Reset token is missing or invalid.')).toBeInTheDocument();
    });
  });

  it('submits reset-password request and redirects to login', async () => {
    mockApiFetch.mockResolvedValueOnce({});
    const { user } = renderWithProviders(<ResetPasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('New password')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('New password'), 'NewPassword123!');
    await user.type(screen.getByLabelText('Confirm password'), 'NewPassword123!');
    await user.click(screen.getByRole('button', { name: 'Reset password' }));

    await waitFor(() => {
      expect(mockApiFetch).toHaveBeenCalledWith(
        '/auth/reset-password',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({
            token: 'reset-token-123',
            newPassword: 'NewPassword123!',
          }),
        }),
      );
    });

    await waitFor(() => {
      expect(replace).toHaveBeenCalledWith('/en/login?reset=success');
    });
  });

  it('shows token used message when API returns RESET_TOKEN_ALREADY_USED', async () => {
    mockApiFetch.mockRejectedValueOnce(
      new ApiError('Bad Request', 400, { detail: 'RESET_TOKEN_ALREADY_USED' })
    );
    const { user } = renderWithProviders(<ResetPasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('New password')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('New password'), 'NewPassword123!');
    await user.type(screen.getByLabelText('Confirm password'), 'NewPassword123!');
    await user.click(screen.getByRole('button', { name: 'Reset password' }));

    await waitFor(() => {
      expect(screen.getByText('This reset link has already been used.')).toBeInTheDocument();
    });
  });

  it('shows expired token message when API returns 400 with another code', async () => {
    mockApiFetch.mockRejectedValueOnce(
      new ApiError('Bad Request', 400, { detail: 'INVALID_OR_EXPIRED_RESET_TOKEN' })
    );
    const { user } = renderWithProviders(<ResetPasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('New password')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('New password'), 'NewPassword123!');
    await user.type(screen.getByLabelText('Confirm password'), 'NewPassword123!');
    await user.click(screen.getByRole('button', { name: 'Reset password' }));

    await waitFor(() => {
      expect(screen.getByText('This reset link is invalid or expired.')).toBeInTheDocument();
    });
  });
});
