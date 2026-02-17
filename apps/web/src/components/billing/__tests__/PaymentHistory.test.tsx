// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PaymentHistory } from '@/components/billing/PaymentHistory';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/hooks/billing', () => ({
  usePayments: vi.fn(),
}));

import { usePayments } from '@/hooks/billing';

const mockUsePayments = vi.mocked(usePayments);

beforeEach(() => {
  vi.resetAllMocks();
});

describe('PaymentHistory', () => {
  it('renders loading skeleton when loading', () => {
    mockUsePayments.mockReturnValue({ payments: [], isLoading: true, error: null });

    renderWithProviders(<PaymentHistory orgId="org-1" />);

    expect(document.querySelector('.animate-pulse')).toBeInTheDocument();
  });

  it('renders empty state when no payments', async () => {
    mockUsePayments.mockReturnValue({ payments: [], isLoading: false, error: null });

    renderWithProviders(<PaymentHistory orgId="org-1" />);

    await waitFor(() => {
      expect(screen.getByText(/no payments/i)).toBeInTheDocument();
    });
  });

  it('renders payment rows when payments exist', async () => {
    mockUsePayments.mockReturnValue({
      payments: [
        {
          id: 'pay-1',
          amount: 2900,
          currency: 'usd',
          status: 'succeeded',
          description: 'Pro Plan - Monthly',
          createdAtUtc: '2026-01-15T10:00:00Z',
        },
      ],
      isLoading: false,
      error: null,
    });

    renderWithProviders(<PaymentHistory orgId="org-1" />);

    await waitFor(() => {
      expect(screen.getByText('Pro Plan - Monthly')).toBeInTheDocument();
    });
  });
});
