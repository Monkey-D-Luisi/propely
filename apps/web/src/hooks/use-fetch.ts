// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback, useRef } from 'react';
import { apiFetch } from '@/lib/api';
import type { PaginationState } from '@/lib/pagination';
import { emptyPagination } from '@/lib/pagination';

/**
 * Generic function signature matching all service-specific fetch functions
 * (apiFetch, contactsApiFetch, propertiesApiFetch, aiApiFetch, etc.).
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type FetcherFn = (path: string, init: RequestInit, schema?: { parse: (data: unknown) => any }) => Promise<any>;

// ---------------------------------------------------------------------------
// useFetch — generic data fetching hook
// ---------------------------------------------------------------------------

export type UseFetchResult<T> = {
  data: T | null;
  isLoading: boolean;
  error: unknown;
  refetch: (signal?: AbortSignal) => Promise<void>;
};

export type UseFetchOptions<T> = {
  /** Zod schema (or any object with a `parse` method) for response validation. */
  schema?: { parse: (data: unknown) => unknown };
  /** Transform the raw (validated) response before storing it. */
  transform?: (raw: unknown) => T;
  /** Service-specific fetch function. Defaults to `apiFetch` (orgs-api). */
  fetcher?: FetcherFn;
};

/**
 * Generic data-fetching hook that handles loading/error/data state,
 * AbortController cleanup, optional Zod validation, and refetch.
 *
 * Pass `null` as `url` to skip fetching (conditional fetching).
 */
export function useFetch<T>(
  url: string | null,
  options?: UseFetchOptions<T>,
): UseFetchResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setLoading] = useState(url !== null);
  const [error, setError] = useState<unknown>(null);

  // Store options in a ref so that unstable inline objects / arrow functions
  // passed by callers don't cause unnecessary re-fetches.
  const optionsRef = useRef(options);
  optionsRef.current = options;

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    if (url === null) return;
    const opts = optionsRef.current;
    const fetchFn: FetcherFn = opts?.fetcher ?? apiFetch;
    setLoading(true);
    setError(null);
    try {
      const raw = await fetchFn(url, { method: 'GET', signal }, opts?.schema);
      const result = opts?.transform ? opts.transform(raw) : (raw as T);
      setData(result);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [url]);

  useEffect(() => {
    if (url === null) {
      setData(null);
      setLoading(false);
      return;
    }
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher, url]);

  return { data, isLoading, error, refetch: fetcher };
}

// ---------------------------------------------------------------------------
// usePaginatedFetch — paginated variant
// ---------------------------------------------------------------------------

export type UsePaginatedFetchResult<T> = {
  items: T[];
  pagination: PaginationState;
  isLoading: boolean;
  error: unknown;
  refetch: (signal?: AbortSignal) => Promise<void>;
};

export type UsePaginatedFetchOptions = {
  /** Zod schema for the full PagedResponse envelope. */
  schema?: { parse: (data: unknown) => unknown };
  /** Service-specific fetch function. Defaults to `apiFetch`. */
  fetcher?: FetcherFn;
};

/**
 * Paginated data-fetching hook. Expects the API to return a standard
 * `{ items, pageNumber, totalPages, totalCount, hasPreviousPage, hasNextPage }`
 * envelope and splits it into `items` + `pagination`.
 *
 * Pass `null` as `url` to skip fetching.
 */
export function usePaginatedFetch<T>(
  url: string | null,
  options?: UsePaginatedFetchOptions,
): UsePaginatedFetchResult<T> {
  const [items, setItems] = useState<T[]>([]);
  const [pagination, setPagination] = useState<PaginationState>(emptyPagination);
  const [isLoading, setLoading] = useState(url !== null);
  const [error, setError] = useState<unknown>(null);

  const optionsRef = useRef(options);
  optionsRef.current = options;

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    if (url === null) return;
    const opts = optionsRef.current;
    const fetchFn: FetcherFn = opts?.fetcher ?? apiFetch;
    setLoading(true);
    setError(null);
    try {
      const data = await fetchFn(url, { method: 'GET', signal }, opts?.schema);
      const { items: fetchedItems, ...paginationData } = data as {
        items: T[];
        pageNumber: number;
        totalPages: number;
        totalCount: number;
        hasPreviousPage: boolean;
        hasNextPage: boolean;
      };
      setItems(fetchedItems);
      setPagination(paginationData);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [url]);

  useEffect(() => {
    if (url === null) {
      setItems([]);
      setPagination(emptyPagination);
      setLoading(false);
      return;
    }
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher, url]);

  return { items, pagination, isLoading, error, refetch: fetcher };
}
