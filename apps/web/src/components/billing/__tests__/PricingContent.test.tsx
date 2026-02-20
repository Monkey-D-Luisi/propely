// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PricingContent } from '@/components/billing/PricingContent';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/hooks/billing', () => ({
  usePlans: vi.fn(),
  useCreateCheckout: vi.fn(() => vi.fn()),
}));

vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(() => ({ user: null, isLoading: false, error: null })),
  useMyOrgs: vi.fn(() => ({ orgs: [], isLoading: false, error: null })),
}));

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
  useRouter: vi.fn(() => ({ push: vi.fn(), replace: vi.fn() })),
}));

vi.mock('@/components/common/TrustedBySection', () => ({
  TrustedBySection: () => <div data-testid="trusted-by" />,
}));

import { usePlans } from '@/hooks/billing';

const mockUsePlans = vi.mocked(usePlans);

beforeEach(() => {
  vi.resetAllMocks();
});

describe('PricingContent', () => {
  it('renders loading skeleton when loading', () => {
    mockUsePlans.mockReturnValue({ plans: [], isLoading: true, error: null });

    renderWithProviders(<PricingContent />);

    expect(document.querySelector('.animate-pulse')).toBeInTheDocument();
  });

  it('renders pricing title when loaded', async () => {
    mockUsePlans.mockReturnValue({ plans: [], isLoading: false, error: null });

    renderWithProviders(<PricingContent />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { level: 1 })).toBeInTheDocument();
    });
  });

  it('renders fallback plan cards when API returns no plans', async () => {
    mockUsePlans.mockReturnValue({ plans: [], isLoading: false, error: null });

    renderWithProviders(<PricingContent />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: /starter/i })).toBeInTheDocument();
    });
  });
});
