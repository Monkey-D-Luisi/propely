// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"use client";

import { useCallback, useEffect, useState } from "react";
import { apiFetch } from "@/lib/api";
import {
  type Agency,
  AgenciesResponseSchema,
  type AgencyDetail,
  AgencyDetailSchema,
  type CreateAgencyResponse,
  CreateAgencyResponseSchema,
} from "@/lib/schemas";

// --- List agencies for current user
export function useAgencies() {
  const [agencies, setAgencies] = useState<Agency[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetchData = useCallback(async (showLoading: boolean, signal?: AbortSignal) => {
    if (showLoading) setLoading(true);
    setError(null);
    try {
      const data = await apiFetch<Agency[]>(
        "/api/agencies",
        { method: "GET", signal },
        AgenciesResponseSchema,
      );
      setAgencies(data);
    } catch (e) {
      if (e instanceof DOMException && e.name === "AbortError") return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    void fetchData(true, controller.signal);
    return () => { controller.abort(); };
  }, [fetchData]);

  const refetch = useCallback(() => fetchData(false), [fetchData]);

  return { agencies, isLoading, error, refetch };
}

// --- Get single agency details
export function useAgency(agencyId: string) {
  const [agency, setAgency] = useState<AgencyDetail | null>(null);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const data = await apiFetch<AgencyDetail>(
        `/api/agencies/${agencyId}`,
        { method: "GET", signal },
        AgencyDetailSchema,
      );
      setAgency(data);
    } catch (e) {
      if (e instanceof DOMException && e.name === "AbortError") return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [agencyId]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { agency, isLoading, error, refetch: fetcher };
}

// --- Create agency
export function useCreateAgency() {
  return async (name: string, slug: string) => {
    return apiFetch<CreateAgencyResponse>(
      "/api/agencies",
      {
        method: "POST",
        body: JSON.stringify({ name, slug }),
      },
      CreateAgencyResponseSchema,
    );
  };
}

// --- Delete agency
export function useDeleteAgency(agencyId: string) {
  return async () => {
    await apiFetch(`/api/agencies/${agencyId}`, {
      method: "DELETE",
    });
  };
}

// --- Add branch to agency
export function useAddBranch(agencyId: string) {
  return async (organizationId: string) => {
    await apiFetch(`/api/agencies/${agencyId}/branches`, {
      method: "POST",
      body: JSON.stringify({ organizationId }),
    });
  };
}

// --- Remove branch from agency
export function useRemoveBranch(agencyId: string) {
  return async (branchId: string) => {
    await apiFetch(`/api/agencies/${agencyId}/branches/${branchId}`, {
      method: "DELETE",
    });
  };
}
