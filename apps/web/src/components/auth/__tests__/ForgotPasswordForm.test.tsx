// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ForgotPasswordForm } from '@/components/auth/ForgotPasswordForm';
import { renderWithProviders, screen, waitFor } from '@test/utils';

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

describe('ForgotPasswordForm', () => {
  beforeEach(() => {
    vi.resetAllMocks();
    mockEnsureCsrfToken.mockResolvedValue('csrf-token-123');
  });

  it('renders forgot password form', async () => {
    renderWithProviders(<ForgotPasswordForm />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Forgot your password?' })).toBeInTheDocument();
    });

    expect(screen.getByLabelText('Email')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Send reset link' })).toBeInTheDocument();
  });

  it('submits email to forgot-password endpoint', async () => {
    mockApiFetch.mockResolvedValueOnce({});
    const { user } = renderWithProviders(<ForgotPasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.click(screen.getByRole('button', { name: 'Send reset link' }));

    await waitFor(() => {
      expect(mockApiFetch).toHaveBeenCalledWith(
        '/auth/forgot-password',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({ email: 'test@example.com', locale: 'en' }),
        }),
      );
    });
  });

  it('shows success message after submit', async () => {
    mockApiFetch.mockResolvedValueOnce({});
    const { user } = renderWithProviders(<ForgotPasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.click(screen.getByRole('button', { name: 'Send reset link' }));

    await waitFor(() => {
      expect(screen.getByText('Reset link sent')).toBeInTheDocument();
    });
  });

  it('shows rate limit error on 429', async () => {
    mockApiFetch.mockRejectedValueOnce(new ApiError('Too many requests', 429, undefined, 15));
    const { user } = renderWithProviders(<ForgotPasswordForm />);

    await waitFor(() => {
      expect(screen.getByLabelText('Email')).toBeInTheDocument();
    });

    await user.type(screen.getByLabelText('Email'), 'test@example.com');
    await user.click(screen.getByRole('button', { name: 'Send reset link' }));

    await waitFor(() => {
      expect(screen.getByText('Too many attempts. Please try again in 15 seconds.')).toBeInTheDocument();
    });
  });
});
