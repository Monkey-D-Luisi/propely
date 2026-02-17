// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BillingManagement } from '@/components/billing/BillingManagement';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/hooks/billing', () => ({
  useSubscription: vi.fn(),
  useCreateCustomerPortal: vi.fn(() => vi.fn()),
  usePayments: vi.fn(() => ({ payments: [], isLoading: false, error: null })),
}));

vi.mock('@/hooks/orgs', () => ({
  useOrg: vi.fn(() => ({ org: { id: 'org-1', name: 'Test Org', role: 'owner' } })),
  useCurrentUser: vi.fn(() => ({ user: null, isLoading: false, error: null })),
}));

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { useSubscription } from '@/hooks/billing';

const mockUseSubscription = vi.mocked(useSubscription);

beforeEach(() => {
  vi.resetAllMocks();
});

describe('BillingManagement', () => {
  it('renders loading skeleton when loading', () => {
    mockUseSubscription.mockReturnValue({
      subscription: null,
      isLoading: true,
      error: null,
    });

    renderWithProviders(<BillingManagement orgId="org-1" />);

    expect(document.querySelector('.animate-pulse')).toBeInTheDocument();
  });

  it('renders no-subscription state when subscription is null', async () => {
    mockUseSubscription.mockReturnValue({
      subscription: null,
      isLoading: false,
      error: null,
    });

    renderWithProviders(<BillingManagement orgId="org-1" />);

    await waitFor(() => {
      expect(screen.getByText(/no active subscription/i)).toBeInTheDocument();
    });
  });

  it('renders subscription details when subscription exists', async () => {
    mockUseSubscription.mockReturnValue({
      subscription: {
        planName: 'Pro Plan',
        status: 'active',
        currentPeriodEnd: '2026-03-01T00:00:00Z',
        features: ['Unlimited projects', 'Priority support'],
      },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<BillingManagement orgId="org-1" />);

    await waitFor(() => {
      expect(screen.getByText('Pro Plan')).toBeInTheDocument();
    });
    expect(screen.getByText('Unlimited projects')).toBeInTheDocument();
    expect(screen.getByText('Priority support')).toBeInTheDocument();
  });
});
