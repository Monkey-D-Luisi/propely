// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"use client";

import { useCallback, useEffect, useState } from "react";
import { apiFetch } from "@/lib/api";
import { ensureCsrfToken } from "@/lib/csrf";
import {
  type Plan,
  PlansResponseSchema,
  type Subscription,
  SubscriptionSchema,
  type Payment,
  PaymentsResponseSchema,
} from "@/lib/schemas";

export function usePlans() {
  const [plans, setPlans] = useState<Plan[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchPlans() {
      setLoading(true);
      setError(null);
      try {
        const data = await apiFetch<Plan[]>(
          "/billing/plans",
          { method: "GET", signal: controller.signal },
          PlansResponseSchema,
        );
        setPlans(data);
      } catch (e) {
        if (e instanceof DOMException && e.name === "AbortError") return;
        setError(e);
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }

    fetchPlans();
    return () => {
      controller.abort();
    };
  }, []);

  return { plans, isLoading, error };
}

export function useSubscription(orgId: string) {
  const [subscription, setSubscription] = useState<Subscription | null>(null);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchSubscription() {
      setLoading(true);
      setError(null);
      try {
        const data = await apiFetch<Subscription>(
          `/billing/subscription?orgId=${orgId}`,
          { method: "GET", signal: controller.signal },
          SubscriptionSchema,
        );
        setSubscription(data);
      } catch (e) {
        if (e instanceof DOMException && e.name === "AbortError") return;
        setError(e);
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }

    fetchSubscription();
    return () => {
      controller.abort();
    };
  }, [orgId]);

  return { subscription, isLoading, error };
}

export function usePayments(orgId: string) {
  const [payments, setPayments] = useState<Payment[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchPayments() {
      setLoading(true);
      setError(null);
      try {
        const data = await apiFetch<Payment[]>(
          `/billing/payments?orgId=${orgId}`,
          { method: "GET", signal: controller.signal },
          PaymentsResponseSchema,
        );
        setPayments(data);
      } catch (e) {
        if (e instanceof DOMException && e.name === "AbortError") return;
        setError(e);
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }

    fetchPayments();
    return () => {
      controller.abort();
    };
  }, [orgId]);

  return { payments, isLoading, error };
}

const ALLOWED_REDIRECT_HOSTNAMES = new Set([
  "checkout.stripe.com",
  "billing.stripe.com",
]);

function isAllowedRedirect(url: string): boolean {
  try {
    const parsed = new URL(url);
    return ALLOWED_REDIRECT_HOSTNAMES.has(parsed.hostname);
  } catch {
    return false;
  }
}

export function useCreateCheckout() {
  return useCallback(
    async (orgId: string, planId: string, locale: string) => {
      const csrfToken = await ensureCsrfToken();
      const data = await apiFetch<{ checkoutUrl: string }>(
        "/billing/checkout",
        {
          method: "POST",
          headers: csrfToken ? { "x-csrf-token": csrfToken } : {},
          body: JSON.stringify({
            orgId,
            planId,
            successUrl: `/${locale}/orgs/${orgId}/billing`,
            cancelUrl: `/${locale}/pricing`,
          }),
        },
      );
      if (!isAllowedRedirect(data.checkoutUrl)) {
        throw new Error("Invalid checkout redirect URL");
      }
      window.location.href = data.checkoutUrl;
    },
    [],
  );
}

export function useCreateCustomerPortal() {
  return useCallback(async (orgId: string, locale: string) => {
    const csrfToken = await ensureCsrfToken();
    const data = await apiFetch<{ portalUrl: string }>(
      "/billing/customer-portal",
      {
        method: "POST",
        headers: csrfToken ? { "x-csrf-token": csrfToken } : {},
        body: JSON.stringify({
          orgId,
          returnUrl: `/${locale}/orgs/${orgId}/billing`,
        }),
      },
    );
    if (!isAllowedRedirect(data.portalUrl)) {
      throw new Error("Invalid portal redirect URL");
    }
    window.location.href = data.portalUrl;
  }, []);
}

export function useCreatePayment() {
  return useCallback(
    async (
      orgId: string,
      amount: number,
      currency: string,
      description: string,
      locale: string,
    ) => {
      const csrfToken = await ensureCsrfToken();
      const data = await apiFetch<{ checkoutUrl: string }>(
        "/billing/payment",
        {
          method: "POST",
          headers: csrfToken ? { "x-csrf-token": csrfToken } : {},
          body: JSON.stringify({
            orgId,
            amount,
            currency,
            description,
            successUrl: `/${locale}/orgs/${orgId}/billing`,
            cancelUrl: `/${locale}/orgs/${orgId}/billing`,
          }),
        },
      );
      if (!isAllowedRedirect(data.checkoutUrl)) {
        throw new Error("Invalid checkout redirect URL");
      }
      window.location.href = data.checkoutUrl;
    },
    [],
  );
}
