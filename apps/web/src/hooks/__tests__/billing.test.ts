// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import {
  usePlans,
  useSubscription,
  usePayments,
  useCreateCheckout,
  useCreateCustomerPortal,
  useCreatePayment,
} from '@/hooks/billing';
import type { Plan, Subscription, Payment } from '@/lib/schemas';

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return {
    ...actual,
    apiFetch: vi.fn(),
  };
});

vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn().mockResolvedValue('mock-csrf-token'),
}));

import { apiFetch } from '@/lib/api';

const mockApiFetch = vi.mocked(apiFetch);

// ── Test data ──────────────────────────────────────────────

const mockPlan: Plan = {
  id: 'plan-1',
  name: 'Basic',
  description: 'Basic plan',
  price: 999,
  currency: 'USD',
  interval: 'month',
  features: ['Feature 1'],
  stripePriceId: 'price_123',
};

const mockSubscription: Subscription = {
  planId: 'plan-1',
  status: 'active',
  currentPeriodStart: '2024-01-01T00:00:00Z',
  currentPeriodEnd: '2024-02-01T00:00:00Z',
  cancelAtPeriodEnd: false,
};

const mockPayment: Payment = {
  id: 'payment-1',
  amount: 999,
  currency: 'USD',
  status: 'succeeded',
  description: 'Subscription payment',
  createdAt: '2024-01-01T00:00:00Z',
};

const ORG_ID = 'org-123';
const LOCALE = 'en';

beforeEach(() => {
  vi.resetAllMocks();
});

// ── usePlans ───────────────────────────────────────────────

describe('usePlans', () => {
  it('starts in loading state', () => {
    mockApiFetch.mockReturnValue(new Promise(() => {}));
    const { result } = renderHook(() => usePlans());

    expect(result.current.isLoading).toBe(true);
    expect(result.current.plans).toEqual([]);
    expect(result.current.error).toBeNull();
  });

  it('returns plans on success', async () => {
    mockApiFetch.mockResolvedValueOnce([mockPlan]);
    const { result } = renderHook(() => usePlans());

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.plans).toEqual([mockPlan]);
    expect(result.current.error).toBeNull();
    expect(mockApiFetch).toHaveBeenCalledWith(
      '/billing/plans',
      expect.objectContaining({ method: 'GET' }),
      expect.anything(),
    );
  });

  it('sets error on failure', async () => {
    const error = new Error('Network error');
    mockApiFetch.mockRejectedValueOnce(error);
    const { result } = renderHook(() => usePlans());

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.plans).toEqual([]);
    expect(result.current.error).toBe(error);
  });
});

// ── useSubscription ────────────────────────────────────────

describe('useSubscription', () => {
  it('starts in loading state', () => {
    mockApiFetch.mockReturnValue(new Promise(() => {}));
    const { result } = renderHook(() => useSubscription(ORG_ID));

    expect(result.current.isLoading).toBe(true);
    expect(result.current.subscription).toBeNull();
  });

  it('returns subscription on success', async () => {
    mockApiFetch.mockResolvedValueOnce(mockSubscription);
    const { result } = renderHook(() => useSubscription(ORG_ID));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.subscription).toEqual(mockSubscription);
    expect(mockApiFetch).toHaveBeenCalledWith(
      `/billing/subscription?orgId=${ORG_ID}`,
      expect.objectContaining({ method: 'GET' }),
      expect.anything(),
    );
  });

  it('sets error on failure', async () => {
    const error = new Error('Failed to fetch');
    mockApiFetch.mockRejectedValueOnce(error);
    const { result } = renderHook(() => useSubscription(ORG_ID));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.subscription).toBeNull();
    expect(result.current.error).toBe(error);
  });

  it('refetches when orgId changes', async () => {
    mockApiFetch.mockResolvedValue(mockSubscription);
    const { result, rerender } = renderHook(
      ({ orgId }) => useSubscription(orgId),
      { initialProps: { orgId: ORG_ID } },
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(mockApiFetch).toHaveBeenCalledTimes(1);

    rerender({ orgId: 'org-456' });
    await waitFor(() => expect(mockApiFetch).toHaveBeenCalledTimes(2));
  });
});

// ── usePayments ────────────────────────────────────────────

describe('usePayments', () => {
  it('starts in loading state', () => {
    mockApiFetch.mockReturnValue(new Promise(() => {}));
    const { result } = renderHook(() => usePayments(ORG_ID));

    expect(result.current.isLoading).toBe(true);
    expect(result.current.payments).toEqual([]);
  });

  it('returns payments on success', async () => {
    mockApiFetch.mockResolvedValueOnce([mockPayment]);
    const { result } = renderHook(() => usePayments(ORG_ID));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.payments).toEqual([mockPayment]);
    expect(mockApiFetch).toHaveBeenCalledWith(
      `/billing/payments?orgId=${ORG_ID}`,
      expect.objectContaining({ method: 'GET' }),
      expect.anything(),
    );
  });

  it('sets error on failure', async () => {
    const error = new Error('Failed');
    mockApiFetch.mockRejectedValueOnce(error);
    const { result } = renderHook(() => usePayments(ORG_ID));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.payments).toEqual([]);
    expect(result.current.error).toBe(error);
  });
});

// ── useCreateCheckout ──────────────────────────────────────

describe('useCreateCheckout', () => {
  it('creates checkout session with valid redirect URL', async () => {
    const checkoutUrl = 'https://checkout.stripe.com/pay/cs_test_123';
    mockApiFetch.mockResolvedValueOnce({ checkoutUrl });

    const { result } = renderHook(() => useCreateCheckout());

    Object.defineProperty(window, 'location', {
      writable: true,
      value: { href: '' },
    });

    await result.current(ORG_ID, 'plan-1', LOCALE);

    expect(mockApiFetch).toHaveBeenCalledWith(
      '/billing/checkout',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({
          orgId: ORG_ID,
          planId: 'plan-1',
          successUrl: `/${LOCALE}/orgs/${ORG_ID}/billing`,
          cancelUrl: `/${LOCALE}/pricing`,
        }),
      }),
    );
    expect(window.location.href).toBe(checkoutUrl);
  });

  it('throws error for invalid redirect URL', async () => {
    mockApiFetch.mockResolvedValueOnce({
      checkoutUrl: 'https://evil.com/phishing',
    });

    const { result } = renderHook(() => useCreateCheckout());

    await expect(result.current(ORG_ID, 'plan-1', LOCALE)).rejects.toThrow(
      'Invalid checkout redirect URL',
    );
  });
});

// ── useCreateCustomerPortal ────────────────────────────────

describe('useCreateCustomerPortal', () => {
  it('creates customer portal session with valid redirect URL', async () => {
    const portalUrl = 'https://billing.stripe.com/session/cs_test_123';
    mockApiFetch.mockResolvedValueOnce({ portalUrl });

    const { result } = renderHook(() => useCreateCustomerPortal());

    Object.defineProperty(window, 'location', {
      writable: true,
      value: { href: '' },
    });

    await result.current(ORG_ID, LOCALE);

    expect(mockApiFetch).toHaveBeenCalledWith(
      '/billing/customer-portal',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({
          orgId: ORG_ID,
          returnUrl: `/${LOCALE}/orgs/${ORG_ID}/billing`,
        }),
      }),
    );
    expect(window.location.href).toBe(portalUrl);
  });

  it('throws error for invalid redirect URL', async () => {
    mockApiFetch.mockResolvedValueOnce({
      portalUrl: 'https://malicious.com/portal',
    });

    const { result } = renderHook(() => useCreateCustomerPortal());

    await expect(result.current(ORG_ID, LOCALE)).rejects.toThrow(
      'Invalid portal redirect URL',
    );
  });
});

// ── useCreatePayment ───────────────────────────────────────

describe('useCreatePayment', () => {
  it('creates payment session with valid redirect URL', async () => {
    const checkoutUrl = 'https://checkout.stripe.com/pay/cs_test_payment';
    mockApiFetch.mockResolvedValueOnce({ checkoutUrl });

    const { result } = renderHook(() => useCreatePayment());

    Object.defineProperty(window, 'location', {
      writable: true,
      value: { href: '' },
    });

    await result.current(ORG_ID, 5000, 'USD', 'One-time payment', LOCALE);

    expect(mockApiFetch).toHaveBeenCalledWith(
      '/billing/payment',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({
          orgId: ORG_ID,
          amount: 5000,
          currency: 'USD',
          description: 'One-time payment',
          successUrl: `/${LOCALE}/orgs/${ORG_ID}/billing`,
          cancelUrl: `/${LOCALE}/orgs/${ORG_ID}/billing`,
        }),
      }),
    );
    expect(window.location.href).toBe(checkoutUrl);
  });

  it('throws error for invalid redirect URL', async () => {
    mockApiFetch.mockResolvedValueOnce({
      checkoutUrl: 'https://phishing.com/pay',
    });

    const { result } = renderHook(() => useCreatePayment());

    await expect(
      result.current(ORG_ID, 5000, 'USD', 'Payment', LOCALE),
    ).rejects.toThrow('Invalid checkout redirect URL');
  });
});
