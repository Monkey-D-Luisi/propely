// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"use client";

import { useCallback, useEffect, useState } from "react";
import { apiFetch } from "@/lib/api";
import { ensureCsrfToken } from "@/lib/csrf";
import { AUTH_USER_UPDATED_EVENT } from "@/lib/auth-events";
import { emptyPagination, type PaginationState } from "@/lib/pagination";
export type { PaginationState } from "@/lib/pagination";
import {
  InviteRequest,
  InviteResponseSchema,
  Me,
  MeResponseSchema,
  Member,
  MembersResponseSchema,
  MyOrgsResponseSchema,
  Org,
  type OrgDetail,
  OrgDetailSchema,
  type PagedResponse,
  Role,
} from "@/lib/schemas";
import { useFetch } from "@/hooks/use-fetch";

// --- Current user (assumes /auth/me already exists in API)
export function useCurrentUser() {
  const { data: user, isLoading, error, refetch } = useFetch<Me>(
    "/auth/me",
    {
      schema: MeResponseSchema,
      transform: (raw) => (raw as { user: Me }).user,
    },
  );

  useEffect(() => {
    const onUserUpdated = () => {
      void refetch();
    };

    window.addEventListener(AUTH_USER_UPDATED_EVENT, onUserUpdated);
    return () => {
      window.removeEventListener(AUTH_USER_UPDATED_EVENT, onUserUpdated);
    };
  }, [refetch]);

  return { user, isLoading, error };
}

// --- My organizations (paginated)
export function useMyOrgs(page = 1, pageSize = 20) {
  const [orgs, setOrgs] = useState<Org[]>([]);
  const [pagination, setPagination] = useState<PaginationState>(emptyPagination);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetchData = useCallback(async (showLoading: boolean, signal?: AbortSignal) => {
    if (showLoading) setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
      });
      const data = await apiFetch<PagedResponse<Org>>(
        `/orgs/mine?${params}`,
        { method: "GET", signal },
        MyOrgsResponseSchema,
      );
      const { items, ...paginationData } = data;
      setOrgs(items);
      setPagination(paginationData);
    } catch (e) {
      if (e instanceof DOMException && e.name === "AbortError") return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    const controller = new AbortController();
    void fetchData(true, controller.signal);
    return () => { controller.abort(); };
  }, [fetchData]);

  const refetch = useCallback(() => fetchData(false), [fetchData]);

  return { orgs, pagination, isLoading, error, refetch };
}

// --- Members of an organization (paginated with search)
export function useMembers(orgId: string, page = 1, pageSize = 20, search?: string) {
  const [members, setMembers] = useState<Member[]>([]);
  const [pagination, setPagination] = useState<PaginationState>(emptyPagination);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
      });
      if (search) params.set("search", search);
      const data = await apiFetch<PagedResponse<Member>>(
        `/orgs/${orgId}/members?${params}`,
        { method: "GET", signal },
        MembersResponseSchema,
      );
      const { items, ...paginationData } = data;
      setMembers(items);
      setPagination(paginationData);
    } catch (e) {
      if (e instanceof DOMException && e.name === "AbortError") return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [orgId, page, pageSize, search]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { members, setMembers, pagination, isLoading, error, refetch: fetcher };
}

// --- Create organization
export function useCreateOrg() {
  return async (name: string) => {
    const csrfToken = await ensureCsrfToken();
    return apiFetch<{ id: string; name: string }>(`/orgs`, {
      method: "POST",
      headers: csrfToken ? { "x-csrf-token": csrfToken } : {},
      body: JSON.stringify({ name }),
    });
  };
}

// --- Invite member
export function useInviteMember(orgId: string) {
  return async (payload: InviteRequest) => {
    const csrfToken = await ensureCsrfToken();
    return apiFetch(
      `/orgs/${orgId}/invitations`,
      {
        method: "POST",
        headers: csrfToken ? { "x-csrf-token": csrfToken } : {},
        body: JSON.stringify(payload),
      },
      InviteResponseSchema,
    );
  };
}

// --- Update role
export function useUpdateRole(orgId: string) {
  return async (userId: string, role: Role) => {
    const csrfToken = await ensureCsrfToken();
    await apiFetch(`/orgs/${orgId}/members/${userId}`, {
      method: "PUT",
      headers: csrfToken ? { "x-csrf-token": csrfToken } : {},
      body: JSON.stringify({ role }),
    });
  };
}

// --- Leave organization
export function useLeaveOrg(orgId: string) {
  return async ({
    members,
    currentMember,
  }: {
    members: Member[];
    currentMember: Member | null;
  }): Promise<{ ok: boolean; reason?: "LAST_OWNER" }> => {
    if (!currentMember) return { ok: true };

    // Client-side check to avoid unnecessary API call
    if (currentMember.role === "owner") {
      const owners = members.filter((m) => m.role === "owner");
      const isLast = owners.length <= 1;
      if (isLast) return { ok: false, reason: "LAST_OWNER" };
    }

    const csrfToken = await ensureCsrfToken();
    await apiFetch(`/orgs/${orgId}/members/me`, {
      method: "DELETE",
      headers: csrfToken ? { "x-csrf-token": csrfToken } : {},
    });
    return { ok: true };
  };
}

// --- Update user profile
export function useUpdateProfile() {
  return async (payload: { name?: string }, csrfToken: string) => {
    return apiFetch<{ id: string; email: string; name: string | null }>(
      "/auth/me",
      {
        method: "PATCH",
        body: JSON.stringify(payload),
        headers: { "x-csrf-token": csrfToken },
      },
    );
  };
}

// --- Change password
export function useChangePassword() {
  return async (payload: { currentPassword: string; newPassword: string }, csrfToken: string) => {
    return apiFetch<{ ok: boolean }>(
      "/auth/password",
      {
        method: "PUT",
        body: JSON.stringify(payload),
        headers: { "x-csrf-token": csrfToken },
      },
    );
  };
}

// --- Delete account
export function useDeleteAccount() {
  return async (payload: { password: string }, csrfToken: string) => {
    return apiFetch<{ ok: boolean }>(
      "/auth/me",
      {
        method: "DELETE",
        body: JSON.stringify(payload),
        headers: { "x-csrf-token": csrfToken },
      },
    );
  };
}

// --- Get single organization details
export function useOrg(orgId: string) {
  const [org, setOrg] = useState<OrgDetail | null>(null);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const data = await apiFetch<OrgDetail>(
        `/orgs/${orgId}`,
        { method: "GET", signal },
        OrgDetailSchema,
      );
      setOrg(data);
    } catch (e) {
      if (e instanceof DOMException && e.name === "AbortError") return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [orgId]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { org, isLoading, error, refetch: fetcher };
}

// --- Update organization
export function useUpdateOrg(orgId: string) {
  return async (payload: { name: string; description?: string }) => {
    const csrfToken = await ensureCsrfToken();
    return apiFetch<{ id: string; name: string; description: string | null }>(
      `/orgs/${orgId}`,
      {
        method: "PATCH",
        headers: csrfToken ? { "x-csrf-token": csrfToken } : {},
        body: JSON.stringify(payload),
      },
    );
  };
}

// --- Delete organization
export function useDeleteOrg(orgId: string) {
  return async () => {
    const csrfToken = await ensureCsrfToken();
    await apiFetch(`/orgs/${orgId}`, {
      method: "DELETE",
      headers: csrfToken ? { "x-csrf-token": csrfToken } : {},
    });
  };
}

// --- Remove member from organization
export function useRemoveMember(orgId: string) {
  return async (userId: string) => {
    const csrfToken = await ensureCsrfToken();
    await apiFetch(`/orgs/${orgId}/members/${userId}`, {
      method: "DELETE",
      headers: csrfToken ? { "x-csrf-token": csrfToken } : {},
    });
  };
}
