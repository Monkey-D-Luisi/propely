// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { aiApiFetch } from '@/lib/api';
import type { WorkItem, PagedResponse, ParseWorkItemResponse } from '@/lib/schemas';
import { WorkItemSchema, WorkItemsResponseSchema, ParseWorkItemResponseSchema } from '@/lib/schemas';
import type { PaginationState } from '@/hooks/orgs';
import { ensureCsrfToken } from '@/lib/csrf';

const emptyPagination: PaginationState = {
  pageNumber: 1,
  totalPages: 0,
  totalCount: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

export function useWorkItems(page = 1, pageSize = 10, status?: string, search?: string) {
  const [items, setItems] = useState<WorkItem[]>([]);
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
      if (status) params.set('status', status);
      if (search) params.set('search', search);

      const data = await aiApiFetch<PagedResponse<WorkItem>>(
        `/v1/work-items?${params}`,
        { method: 'GET', signal },
        WorkItemsResponseSchema,
      );
      const { items: fetchedItems, ...paginationData } = data;
      setItems(fetchedItems);
      setPagination(paginationData);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [page, pageSize, status, search]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { items, pagination, isLoading, error, refetch: fetcher };
}

export function useWorkItem(id: string) {
  const [item, setItem] = useState<WorkItem | null>(null);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const data = await aiApiFetch<WorkItem>(
        `/v1/work-items/${id}`,
        { method: 'GET', signal },
        WorkItemSchema,
      );
      setItem(data);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { item, isLoading, error, refetch: fetcher };
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
