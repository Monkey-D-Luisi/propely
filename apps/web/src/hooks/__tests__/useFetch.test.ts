// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { useFetch, usePaginatedFetch, type FetcherFn } from '@/hooks/use-fetch';

// ---------------------------------------------------------------------------
// Mock apiFetch (default fetcher)
// ---------------------------------------------------------------------------

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return {
    ...actual,
    apiFetch: vi.fn(),
  };
});

import { apiFetch } from '@/lib/api';

const mockApiFetch = vi.mocked(apiFetch);

beforeEach(() => {
  vi.resetAllMocks();
});

// ---------------------------------------------------------------------------
// useFetch
// ---------------------------------------------------------------------------

describe('useFetch', () => {
  it('starts in loading state when url is provided', () => {
    mockApiFetch.mockReturnValue(new Promise(() => {})); // never resolves
    const { result } = renderHook(() => useFetch<{ id: string }>('/test'));

    expect(result.current.isLoading).toBe(true);
    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it('starts with isLoading=false when url is null (conditional fetching)', () => {
    const { result } = renderHook(() => useFetch<unknown>(null));

    expect(result.current.isLoading).toBe(false);
    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it('returns data on success', async () => {
    const payload = { id: '1', name: 'Test' };
    mockApiFetch.mockResolvedValueOnce(payload);

    const { result } = renderHook(() => useFetch<typeof payload>('/items/1'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.data).toEqual(payload);
    expect(result.current.error).toBeNull();
    expect(mockApiFetch).toHaveBeenCalledWith(
      '/items/1',
      expect.objectContaining({ method: 'GET' }),
      undefined,
    );
  });

  it('sets error on failure', async () => {
    const error = new Error('Network error');
    mockApiFetch.mockRejectedValueOnce(error);

    const { result } = renderHook(() => useFetch<unknown>('/fail'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.data).toBeNull();
    expect(result.current.error).toBe(error);
  });

  it('supports refetch', async () => {
    const first = { id: '1' };
    const second = { id: '2' };
    mockApiFetch.mockResolvedValueOnce(first).mockResolvedValueOnce(second);

    const { result } = renderHook(() => useFetch<typeof first>('/items'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.data).toEqual(first);

    await act(async () => {
      await result.current.refetch();
    });

    expect(result.current.data).toEqual(second);
    expect(mockApiFetch).toHaveBeenCalledTimes(2);
  });

  it('aborts fetch on unmount', async () => {
    let capturedSignal: AbortSignal | undefined;
    mockApiFetch.mockImplementation(async (_url, init) => {
      capturedSignal = init?.signal as AbortSignal | undefined;
      return new Promise(() => {}); // never resolves
    });

    const { unmount } = renderHook(() => useFetch<unknown>('/slow'));

    // Wait for the fetch to be initiated
    await waitFor(() => expect(mockApiFetch).toHaveBeenCalled());

    unmount();

    expect(capturedSignal?.aborted).toBe(true);
  });

  it('does not fetch when url is null', async () => {
    const { result } = renderHook(() => useFetch<unknown>(null));

    // Give time for any potential async operations
    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(mockApiFetch).not.toHaveBeenCalled();
    expect(result.current.data).toBeNull();
  });

  it('passes schema to the fetcher', async () => {
    const schema = { parse: vi.fn((d: unknown) => d) };
    mockApiFetch.mockResolvedValueOnce({ value: 42 });

    const { result } = renderHook(() =>
      useFetch<{ value: number }>('/validated', { schema }),
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(mockApiFetch).toHaveBeenCalledWith(
      '/validated',
      expect.objectContaining({ method: 'GET' }),
      schema,
    );
  });

  it('applies transform to raw response', async () => {
    mockApiFetch.mockResolvedValueOnce({ user: { id: '1', name: 'Alice' } });

    const { result } = renderHook(() =>
      useFetch<{ id: string; name: string }>('/auth/me', {
        transform: (raw) => (raw as { user: { id: string; name: string } }).user,
      }),
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.data).toEqual({ id: '1', name: 'Alice' });
  });

  it('uses custom fetcher when provided', async () => {
    const customFetcher = vi.fn().mockResolvedValueOnce({ custom: true });

    const { result } = renderHook(() =>
      useFetch<{ custom: boolean }>('/custom', { fetcher: customFetcher as FetcherFn }),
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(customFetcher).toHaveBeenCalledWith(
      '/custom',
      expect.objectContaining({ method: 'GET' }),
      undefined,
    );
    expect(mockApiFetch).not.toHaveBeenCalled();
    expect(result.current.data).toEqual({ custom: true });
  });

  it('re-fetches when url changes', async () => {
    mockApiFetch.mockResolvedValueOnce({ id: '1' }).mockResolvedValueOnce({ id: '2' });

    const { result, rerender } = renderHook(
      ({ url }: { url: string }) => useFetch<{ id: string }>(url),
      { initialProps: { url: '/items/1' } },
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.data).toEqual({ id: '1' });

    rerender({ url: '/items/2' });

    await waitFor(() => expect(result.current.data).toEqual({ id: '2' }));
    expect(mockApiFetch).toHaveBeenCalledTimes(2);
  });

  it('clears data and stops loading when url changes to null', async () => {
    mockApiFetch.mockResolvedValueOnce({ id: '1' });

    const { result, rerender } = renderHook(
      ({ url }: { url: string | null }) => useFetch<{ id: string }>(url),
      { initialProps: { url: '/items/1' as string | null } },
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.data).toEqual({ id: '1' });

    rerender({ url: null });

    await waitFor(() => {
      expect(result.current.data).toBeNull();
      expect(result.current.isLoading).toBe(false);
    });
  });

  it('ignores AbortError from cancelled requests', async () => {
    const abortError = new DOMException('Aborted', 'AbortError');
    mockApiFetch.mockRejectedValueOnce(abortError);

    const { result } = renderHook(() => useFetch<unknown>('/aborted'));

    // Wait a tick for the effect to run
    await new Promise((r) => setTimeout(r, 50));

    // AbortError should NOT be set as the error
    expect(result.current.error).toBeNull();
  });
});

// ---------------------------------------------------------------------------
// usePaginatedFetch
// ---------------------------------------------------------------------------

describe('usePaginatedFetch', () => {
  const mockPagedResponse = {
    items: [{ id: '1' }, { id: '2' }],
    pageNumber: 1,
    totalPages: 3,
    totalCount: 50,
    hasPreviousPage: false,
    hasNextPage: true,
  };

  it('starts in loading state', () => {
    mockApiFetch.mockReturnValue(new Promise(() => {}));
    const { result } = renderHook(() => usePaginatedFetch<{ id: string }>('/list'));

    expect(result.current.isLoading).toBe(true);
    expect(result.current.items).toEqual([]);
    expect(result.current.pagination.totalCount).toBe(0);
  });

  it('returns items and pagination on success', async () => {
    mockApiFetch.mockResolvedValueOnce(mockPagedResponse);

    const { result } = renderHook(() => usePaginatedFetch<{ id: string }>('/list'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.items).toEqual([{ id: '1' }, { id: '2' }]);
    expect(result.current.pagination).toEqual({
      pageNumber: 1,
      totalPages: 3,
      totalCount: 50,
      hasPreviousPage: false,
      hasNextPage: true,
    });
    expect(result.current.error).toBeNull();
  });

  it('sets error on failure', async () => {
    const error = new Error('Server error');
    mockApiFetch.mockRejectedValueOnce(error);

    const { result } = renderHook(() => usePaginatedFetch<unknown>('/list'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.error).toBe(error);
    expect(result.current.items).toEqual([]);
  });

  it('supports refetch', async () => {
    const first = { ...mockPagedResponse, items: [{ id: '1' }] };
    const second = { ...mockPagedResponse, items: [{ id: '2' }, { id: '3' }] };
    mockApiFetch.mockResolvedValueOnce(first).mockResolvedValueOnce(second);

    const { result } = renderHook(() => usePaginatedFetch<{ id: string }>('/list'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.items).toEqual([{ id: '1' }]);

    await act(async () => {
      await result.current.refetch();
    });

    expect(result.current.items).toEqual([{ id: '2' }, { id: '3' }]);
    expect(mockApiFetch).toHaveBeenCalledTimes(2);
  });

  it('does not fetch when url is null', async () => {
    const { result } = renderHook(() => usePaginatedFetch<unknown>(null));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(mockApiFetch).not.toHaveBeenCalled();
    expect(result.current.items).toEqual([]);
  });

  it('uses custom fetcher when provided', async () => {
    const customFetcher = vi.fn().mockResolvedValueOnce(mockPagedResponse);

    const { result } = renderHook(() =>
      usePaginatedFetch<{ id: string }>('/list', { fetcher: customFetcher as FetcherFn }),
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(customFetcher).toHaveBeenCalled();
    expect(mockApiFetch).not.toHaveBeenCalled();
    expect(result.current.items).toEqual([{ id: '1' }, { id: '2' }]);
  });

  it('passes schema to the fetcher', async () => {
    const schema = { parse: vi.fn((d: unknown) => d) };
    mockApiFetch.mockResolvedValueOnce(mockPagedResponse);

    const { result } = renderHook(() =>
      usePaginatedFetch<{ id: string }>('/list', { schema }),
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(mockApiFetch).toHaveBeenCalledWith(
      '/list',
      expect.objectContaining({ method: 'GET' }),
      schema,
    );
  });

  it('re-fetches when url changes', async () => {
    const page1 = { ...mockPagedResponse, items: [{ id: '1' }], pageNumber: 1 };
    const page2 = { ...mockPagedResponse, items: [{ id: '10' }], pageNumber: 2 };
    mockApiFetch.mockResolvedValueOnce(page1).mockResolvedValueOnce(page2);

    const { result, rerender } = renderHook(
      ({ url }: { url: string }) => usePaginatedFetch<{ id: string }>(url),
      { initialProps: { url: '/list?page=1' } },
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.items).toEqual([{ id: '1' }]);

    rerender({ url: '/list?page=2' });

    await waitFor(() => expect(result.current.items).toEqual([{ id: '10' }]));
    expect(mockApiFetch).toHaveBeenCalledTimes(2);
  });

  it('aborts fetch on unmount', async () => {
    let capturedSignal: AbortSignal | undefined;
    mockApiFetch.mockImplementation(async (_url, init) => {
      capturedSignal = init?.signal as AbortSignal | undefined;
      return new Promise(() => {}); // never resolves
    });

    const { unmount } = renderHook(() => usePaginatedFetch<unknown>('/slow'));

    await waitFor(() => expect(mockApiFetch).toHaveBeenCalled());

    unmount();

    expect(capturedSignal?.aborted).toBe(true);
  });

  it('resets items and pagination when url changes to null', async () => {
    mockApiFetch.mockResolvedValueOnce(mockPagedResponse);

    const { result, rerender } = renderHook(
      ({ url }: { url: string | null }) => usePaginatedFetch<{ id: string }>(url),
      { initialProps: { url: '/list' as string | null } },
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.items.length).toBe(2);

    rerender({ url: null });

    await waitFor(() => {
      expect(result.current.items).toEqual([]);
      expect(result.current.pagination.totalCount).toBe(0);
      expect(result.current.isLoading).toBe(false);
    });
  });
});
