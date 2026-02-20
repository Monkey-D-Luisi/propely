// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useAuditLogs, useExportAuditLogs } from '@/hooks/audit-logs';
import { act } from '@testing-library/react';

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
  vi.stubGlobal('fetch', vi.fn());
});

const mockLog = {
  id: '11111111-1111-1111-1111-111111111111',
  userId: '22222222-2222-2222-2222-222222222222',
  organizationId: null,
  action: 'Added',
  entityType: 'Organization',
  entityId: '33333333-3333-3333-3333-333333333333',
  changes: null,
  correlationId: null,
  createdAtUtc: '2026-01-15T10:30:00Z',
};

const mockPagedResponse = {
  items: [mockLog],
  pageNumber: 1,
  totalPages: 1,
  totalCount: 1,
  hasPreviousPage: false,
  hasNextPage: false,
};

describe('useAuditLogs', () => {
  it('fetches audit logs on mount', async () => {
    mockApiFetch.mockResolvedValue(mockPagedResponse);

    const { result } = renderHook(() => useAuditLogs());

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.logs).toEqual([mockLog]);
    expect(result.current.pagination.totalCount).toBe(1);
    expect(mockApiFetch).toHaveBeenCalledWith(
      expect.stringContaining('/admin/audit-logs?'),
      expect.objectContaining({ method: 'GET' }),
      expect.anything(),
    );
  });

  it('passes filter params to API', async () => {
    mockApiFetch.mockResolvedValue(mockPagedResponse);

    const filters = { action: 'Added', entityType: 'Organization' };
    const { result } = renderHook(() => useAuditLogs(1, 50, filters));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    const callUrl = mockApiFetch.mock.calls[0][0] as string;
    expect(callUrl).toContain('action=Added');
    expect(callUrl).toContain('entityType=Organization');
  });

  it('sets error when API call fails', async () => {
    mockApiFetch.mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useAuditLogs());

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.error).toBeDefined();
    expect(result.current.logs).toEqual([]);
  });

  it('starts in loading state', () => {
    mockApiFetch.mockReturnValue(new Promise(() => {}));

    const { result } = renderHook(() => useAuditLogs());

    expect(result.current.isLoading).toBe(true);
  });
});

describe('useExportAuditLogs', () => {
  function setupExportMocks() {
    const mockBlob = new Blob(['test'], { type: 'text/csv' });
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      blob: () => Promise.resolve(mockBlob),
    });
    vi.stubGlobal('fetch', mockFetch);

    // Create a real <a> element so happy-dom's appendChild works
    const anchor = document.createElement('a');
    const clickSpy = vi.spyOn(anchor, 'click').mockImplementation(() => {});
    vi.spyOn(document, 'createElement').mockImplementation((tag: string) => {
      if (tag === 'a') return anchor;
      return document.createElement.call(document, tag);
    });

    // Stub URL methods
    const origURL = globalThis.URL;
    vi.stubGlobal('URL', {
      ...origURL,
      createObjectURL: vi.fn().mockReturnValue('blob:test'),
      revokeObjectURL: vi.fn(),
    });

    return { mockFetch, clickSpy, anchor };
  }

  it('fetches export and triggers download', async () => {
    const { result } = renderHook(() => useExportAuditLogs());

    const { mockFetch, clickSpy } = setupExportMocks();

    await act(async () => {
      await result.current.exportLogs('csv');
    });

    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/admin/audit-logs/export?format=csv'),
      { credentials: 'include' },
    );
    expect(clickSpy).toHaveBeenCalled();
  });

  it('throws on non-ok response', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
    });
    vi.stubGlobal('fetch', mockFetch);

    const { result } = renderHook(() => useExportAuditLogs());

    let error: Error | undefined;
    try {
      await act(async () => {
        await result.current.exportLogs('json');
      });
    } catch (e) {
      error = e as Error;
    }

    expect(error).toBeDefined();
    expect(error?.message).toContain('Export failed: 500');
  });

  it('resets isExporting after completion', async () => {
    const { result } = renderHook(() => useExportAuditLogs());

    setupExportMocks();

    expect(result.current.isExporting).toBe(false);

    await act(async () => {
      await result.current.exportLogs('csv');
    });

    expect(result.current.isExporting).toBe(false);
  });
});
