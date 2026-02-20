// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { VerificationBanner } from '@/components/auth/VerificationBanner';

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return { ...actual, apiFetch: vi.fn() };
});
vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn(),
}));

import { apiFetch, ApiError } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';

const mockApiFetch = vi.mocked(apiFetch);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);

describe('VerificationBanner', () => {
  beforeEach(() => {
    vi.resetAllMocks();
    mockEnsureCsrfToken.mockResolvedValue('csrf-token-123');
  });

  it('does not render when show is false', () => {
    renderWithProviders(<VerificationBanner show={false} />);
    expect(screen.queryByText('Please verify your email address to secure your account.')).not.toBeInTheDocument();
  });

  it('renders banner when show is true', () => {
    renderWithProviders(<VerificationBanner show />);
    expect(screen.getByText('Please verify your email address to secure your account.')).toBeInTheDocument();
  });

  it('sends resend verification request and shows success feedback', async () => {
    mockApiFetch.mockResolvedValueOnce({ ok: true, sent: true, alreadyVerified: false });
    const { user } = renderWithProviders(<VerificationBanner show />);

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

    await waitFor(() => {
      expect(screen.getByText('Verification email sent. Check your inbox.')).toBeInTheDocument();
    });
  });

  it('shows rate limited feedback when backend returns 429', async () => {
    mockApiFetch.mockRejectedValueOnce(new ApiError('Too Many Requests', 429));
    const { user } = renderWithProviders(<VerificationBanner show />);

    await user.click(screen.getByRole('button', { name: 'Resend verification email' }));

    await waitFor(() => {
      expect(screen.getByText('You can request another email in 5 minutes.')).toBeInTheDocument();
    });
  });
});
