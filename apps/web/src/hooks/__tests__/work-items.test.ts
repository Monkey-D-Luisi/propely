// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import {
  useWorkItems,
  useWorkItem,
  useCreateWorkItem,
  useDeleteWorkItem,
  useParseWorkItem,
} from '@/hooks/work-items';

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return {
    ...actual,
    aiApiFetch: vi.fn(),
  };
});

vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn().mockResolvedValue('mock-csrf-token'),
}));

import { aiApiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';

const mockAiApiFetch = vi.mocked(aiApiFetch);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);

beforeEach(() => {
  vi.resetAllMocks();
  vi.stubGlobal('fetch', vi.fn());
  // Re-mock ensureCsrfToken after resetAllMocks
  mockEnsureCsrfToken.mockResolvedValue('mock-csrf-token');
});

const mockWorkItem = {
  id: '11111111-1111-1111-1111-111111111111',
  orgId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  title: 'Fix login bug',
  description: 'Users cannot log in',
  status: 'Active',
  priority: 'High',
  type: 'Bug',
  dueDateUtc: '2026-02-01T00:00:00Z',
  estimatedEffort: 'M',
  createdAtUtc: '2026-01-15T10:30:00Z',
  updatedAtUtc: '2026-01-16T14:00:00Z',
};

const mockPagedResponse = {
  items: [mockWorkItem],
  pageNumber: 1,
  totalPages: 1,
  totalCount: 1,
  hasPreviousPage: false,
  hasNextPage: false,
};

describe('useWorkItems', () => {
  it('fetches and returns items on mount', async () => {
    mockAiApiFetch.mockResolvedValue(mockPagedResponse);

    const { result } = renderHook(() => useWorkItems());

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.items).toEqual([mockWorkItem]);
    expect(result.current.pagination.totalCount).toBe(1);
    expect(mockAiApiFetch).toHaveBeenCalledWith(
      expect.stringContaining('/v1/work-items?'),
      expect.objectContaining({ method: 'GET' }),
      expect.anything(),
    );
  });

  it('starts in loading state', () => {
    mockAiApiFetch.mockReturnValue(new Promise(() => {}));

    const { result } = renderHook(() => useWorkItems());

    expect(result.current.isLoading).toBe(true);
  });

  it('passes page and pageSize params', async () => {
    mockAiApiFetch.mockResolvedValue(mockPagedResponse);

    const { result } = renderHook(() => useWorkItems(2, 25));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    const callUrl = mockAiApiFetch.mock.calls[0][0] as string;
    expect(callUrl).toContain('page=2');
    expect(callUrl).toContain('pageSize=25');
  });

  it('passes status filter param', async () => {
    mockAiApiFetch.mockResolvedValue(mockPagedResponse);

    const { result } = renderHook(() => useWorkItems(1, 10, 'Active'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    const callUrl = mockAiApiFetch.mock.calls[0][0] as string;
    expect(callUrl).toContain('status=Active');
  });

  it('passes search filter param', async () => {
    mockAiApiFetch.mockResolvedValue(mockPagedResponse);

    const { result } = renderHook(() => useWorkItems(1, 10, undefined, 'login'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    const callUrl = mockAiApiFetch.mock.calls[0][0] as string;
    expect(callUrl).toContain('search=login');
  });

  it('sets error when API call fails', async () => {
    mockAiApiFetch.mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useWorkItems());

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.error).toBeDefined();
    expect(result.current.items).toEqual([]);
  });
});

describe('useWorkItem', () => {
  it('fetches a single item by id', async () => {
    mockAiApiFetch.mockResolvedValue(mockWorkItem);

    const { result } = renderHook(() =>
      useWorkItem('11111111-1111-1111-1111-111111111111'),
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.item).toEqual(mockWorkItem);
    expect(mockAiApiFetch).toHaveBeenCalledWith(
      expect.stringContaining('/v1/work-items/11111111-1111-1111-1111-111111111111'),
      expect.objectContaining({ method: 'GET' }),
      expect.anything(),
    );
  });

  it('starts in loading state', () => {
    mockAiApiFetch.mockReturnValue(new Promise(() => {}));

    const { result } = renderHook(() =>
      useWorkItem('11111111-1111-1111-1111-111111111111'),
    );

    expect(result.current.isLoading).toBe(true);
    expect(result.current.item).toBeNull();
  });

  it('sets error when fetch fails', async () => {
    mockAiApiFetch.mockRejectedValue(new Error('Not found'));

    const { result } = renderHook(() =>
      useWorkItem('11111111-1111-1111-1111-111111111111'),
    );

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.error).toBeDefined();
    expect(result.current.item).toBeNull();
  });
});

describe('useCreateWorkItem', () => {
  it('calls POST to create a work item', async () => {
    mockAiApiFetch.mockResolvedValue(mockWorkItem);

    const { result } = renderHook(() => useCreateWorkItem());

    let created: unknown;
    await act(async () => {
      created = await result.current({ title: 'New item', description: 'Desc', priority: 'High', type: 'Bug', dueDate: '2026-03-01', estimatedEffort: 'L' });
    });

    expect(created).toEqual(mockWorkItem);
    expect(mockAiApiFetch).toHaveBeenCalledWith(
      '/v1/work-items',
      expect.objectContaining({
        method: 'POST',
        headers: { 'x-csrf-token': 'mock-csrf-token' },
        body: JSON.stringify({ title: 'New item', description: 'Desc', priority: 'High', type: 'Bug', dueDate: '2026-03-01', estimatedEffort: 'L' }),
      }),
      expect.anything(),
    );
  });
});

describe('useDeleteWorkItem', () => {
  it('calls DELETE to remove a work item', async () => {
    mockAiApiFetch.mockResolvedValue(undefined);

    const { result } = renderHook(() => useDeleteWorkItem());

    await act(async () => {
      await result.current('11111111-1111-1111-1111-111111111111');
    });

    expect(mockAiApiFetch).toHaveBeenCalledWith(
      '/v1/work-items/11111111-1111-1111-1111-111111111111',
      expect.objectContaining({
        method: 'DELETE',
        headers: { 'x-csrf-token': 'mock-csrf-token' },
      }),
    );
  });
});

describe('useParseWorkItem', () => {
  it('calls POST to parse endpoint and returns result', async () => {
    const mockParseResult = {
      title: 'Parsed title',
      description: 'Parsed description',
      status: 'Active',
      priority: 'High',
      type: 'Bug',
      dueDateUtc: '2026-03-01T00:00:00Z',
      estimatedEffort: 'L',
      confidence: 0.85,
    };
    mockAiApiFetch.mockResolvedValue(mockParseResult);

    const { result } = renderHook(() => useParseWorkItem());

    let parsed: unknown;
    await act(async () => {
      parsed = await result.current.parse('Fix the login bug that breaks email validation');
    });

    expect(parsed).toEqual(mockParseResult);
    expect(mockAiApiFetch).toHaveBeenCalledWith(
      '/v1/work-items/parse',
      expect.objectContaining({
        method: 'POST',
        headers: { 'x-csrf-token': 'mock-csrf-token' },
        body: JSON.stringify({ text: 'Fix the login bug that breaks email validation' }),
      }),
      expect.anything(),
    );
  });

  it('starts with isLoading false', () => {
    const { result } = renderHook(() => useParseWorkItem());

    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('returns null and sets error when parse fails', async () => {
    mockAiApiFetch.mockRejectedValue(new Error('Parse failed'));

    const { result } = renderHook(() => useParseWorkItem());

    let parsed: unknown;
    await act(async () => {
      parsed = await result.current.parse('some text');
    });

    expect(parsed).toBeNull();
    expect(result.current.error).toBeDefined();
    expect(result.current.isLoading).toBe(false);
  });
});
