// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useCallback } from 'react';
import { aiApiFetch } from '@/lib/api';
import { useFetch, usePaginatedFetch, type FetcherFn } from '@/hooks/use-fetch';
import type { WorkItem, ParseWorkItemResponse } from '@/lib/schemas';
import { WorkItemSchema, WorkItemsResponseSchema, ParseWorkItemResponseSchema } from '@/lib/schemas';
import { ensureCsrfToken } from '@/lib/csrf';

function buildWorkItemsUrl(page: number, pageSize: number, status?: string, search?: string): string {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });
  if (status) params.set('status', status);
  if (search) params.set('search', search);
  return `/v1/work-items?${params}`;
}

export function useWorkItems(page = 1, pageSize = 10, status?: string, search?: string) {
  const url = buildWorkItemsUrl(page, pageSize, status, search);
  return usePaginatedFetch<WorkItem>(url, {
    fetcher: aiApiFetch as FetcherFn,
    schema: WorkItemsResponseSchema,
  });
}

export function useWorkItem(id: string) {
  const { data: item, ...rest } = useFetch<WorkItem>(`/v1/work-items/${id}`, {
    fetcher: aiApiFetch as FetcherFn,
    schema: WorkItemSchema,
  });
  return { item, ...rest };
}

export function useCreateWorkItem() {
  return useCallback(async (data: { title: string; description?: string; priority?: string; type?: string; dueDate?: string; estimatedEffort?: string }) => {
    const csrfToken = await ensureCsrfToken();
    return aiApiFetch<WorkItem>('/v1/work-items', {
      method: 'POST',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify(data),
    }, WorkItemSchema);
  }, []);
}

export function useUpdateWorkItem() {
  return useCallback(async (id: string, data: { title: string; description?: string; status: string; priority?: string; type?: string; dueDate?: string; estimatedEffort?: string }) => {
    const csrfToken = await ensureCsrfToken();
    await aiApiFetch<void>(`/v1/work-items/${id}`, {
      method: 'PUT',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify(data),
    });
  }, []);
}

export function useDeleteWorkItem() {
  return useCallback(async (id: string) => {
    const csrfToken = await ensureCsrfToken();
    await aiApiFetch<void>(`/v1/work-items/${id}`, {
      method: 'DELETE',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
    });
  }, []);
}

export function useParseWorkItem() {
  const [isLoading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);

  const parse = useCallback(async (text: string): Promise<ParseWorkItemResponse | null> => {
    setLoading(true);
    setError(null);
    try {
      const csrfToken = await ensureCsrfToken();
      const result = await aiApiFetch<ParseWorkItemResponse>('/v1/work-items/parse', {
        method: 'POST',
        headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
        body: JSON.stringify({ text }),
      }, ParseWorkItemResponseSchema);
      return result;
    } catch (e) {
      setError(e);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  return { parse, isLoading, error };
}
