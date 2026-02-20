// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { AcceptInvite } from '@/components/orgs/AcceptInvite';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { useRouter, usePathname, useSearchParams } from 'next/navigation';

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return { ...actual, apiFetch: vi.fn() };
});
vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn(),
  readCsrfTokenFromCookie: vi.fn(),
}));
vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(() => ({
    user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: true },
    isLoading: false,
    error: null,
  })),
}));
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { apiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { useCurrentUser } from '@/hooks/orgs';

const mockApiFetch = vi.mocked(apiFetch);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);
const mockUseCurrentUser = vi.mocked(useCurrentUser);
const mockUseRouter = vi.mocked(useRouter);
const mockUseSearchParams = vi.mocked(useSearchParams);
const mockUsePathname = vi.mocked(usePathname);

beforeEach(() => {
  vi.resetAllMocks();
  mockUseCurrentUser.mockReturnValue({
    user: { id: '1', email: 'test@example.com', name: 'Test', emailVerified: true },
    isLoading: false,
    error: null,
  });
  mockEnsureCsrfToken.mockResolvedValue('csrf-token-123');
  mockUseRouter.mockReturnValue({
    push: vi.fn(),
    replace: vi.fn(),
    refresh: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
    prefetch: vi.fn(),
  });
  mockUsePathname.mockReturnValue('/accept-invite');
});

describe('AcceptInvite', () => {
  it('shows error when no token is provided', async () => {
    mockUseSearchParams.mockReturnValue(new URLSearchParams());

    renderWithProviders(<AcceptInvite />);

    await waitFor(() => {
      expect(screen.getByText(/missing/i)).toBeInTheDocument();
    });
  });

  it('shows processing state when accepting invite', async () => {
    mockUseSearchParams.mockReturnValue(new URLSearchParams('token=abc123'));
    mockApiFetch.mockReturnValue(new Promise(() => {})); // never resolves

    renderWithProviders(<AcceptInvite />);

    await waitFor(() => {
      expect(screen.getByText(/processing|accepting/i)).toBeInTheDocument();
    });
  });

  it('shows success state on successful acceptance', async () => {
    mockUseSearchParams.mockReturnValue(new URLSearchParams('token=abc123'));
    mockApiFetch.mockResolvedValueOnce({});

    renderWithProviders(<AcceptInvite />);

    await waitFor(() => {
      expect(screen.getByText(/invitation accepted/i)).toBeInTheDocument();
    });
  });

  it('redirects unauthenticated user to login', async () => {
    const replace = vi.fn();
    mockUseRouter.mockReturnValue({
      push: vi.fn(),
      replace,
      refresh: vi.fn(),
      back: vi.fn(),
      forward: vi.fn(),
      prefetch: vi.fn(),
    });
    mockUseCurrentUser.mockReturnValue({ user: null, isLoading: false, error: null });
    mockUseSearchParams.mockReturnValue(new URLSearchParams('token=abc123'));

    renderWithProviders(<AcceptInvite />);

    await waitFor(() => {
      expect(replace).toHaveBeenCalledWith(expect.stringContaining('/login'));
    });
  });
});
