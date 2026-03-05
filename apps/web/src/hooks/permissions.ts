// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"use client";

import { useCallback, useEffect, useState } from "react";
import { apiFetch } from "@/lib/api";
import {
  EffectivePermission,
  EffectivePermissionsResponseSchema,
} from "@/lib/schemas";

// --- User permissions for an organization
export function useUserPermissions(orgId: string, userId: string) {
  const [permissions, setPermissions] = useState<EffectivePermission[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(
    async (signal?: AbortSignal) => {
      setLoading(true);
      setError(null);
      try {
        const data = await apiFetch<EffectivePermission[]>(
          `/orgs/${orgId}/permissions/${userId}`,
          { method: "GET", signal },
          EffectivePermissionsResponseSchema,
        );
        setPermissions(data);
      } catch (e) {
        if (e instanceof DOMException && e.name === "AbortError") return;
        setError(e);
      } finally {
        if (!signal?.aborted) setLoading(false);
      }
    },
    [orgId, userId],
  );

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => {
      controller.abort();
    };
  }, [fetcher]);

  return { permissions, isLoading, error, refetch: fetcher };
}

// --- Set permission override
export function useSetPermissionOverride(orgId: string) {
  return async (userId: string, permission: string, granted: boolean) => {
    await apiFetch(`/orgs/${orgId}/permissions/${userId}/${permission}`, {
      method: "PUT",
      body: JSON.stringify({ granted }),
    });
  };
}

// --- Remove permission override
export function useRemovePermissionOverride(orgId: string) {
  return async (userId: string, permission: string) => {
    await apiFetch(`/orgs/${orgId}/permissions/${userId}/${permission}`, {
      method: "DELETE",
    });
  };
}
