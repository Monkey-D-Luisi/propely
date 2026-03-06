// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { useAppointments } from '@/hooks/useAppointments';

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return {
    ...actual,
    appointmentsApiFetch: vi.fn(),
  };
});

import { appointmentsApiFetch } from '@/lib/api';

const mockAppointmentsApiFetch = vi.mocked(appointmentsApiFetch);

beforeEach(() => {
  vi.resetAllMocks();
  vi.stubGlobal('fetch', vi.fn());
});

const mockPagedResult = {
  items: [
    {
      id: '1',
      title: 'Viewing',
      type: 'PropertyViewing',
      status: 'Scheduled',
      startTimeUtc: '2026-03-10T10:00:00Z',
      endTimeUtc: '2026-03-10T11:00:00Z',
      location: null,
      isAllDay: false,
      propertyId: null,
      contactId: null,
      agentId: 'agent-1',
    },
  ],
  pageNumber: 1,
  totalPages: 1,
  totalCount: 1,
  hasPreviousPage: false,
  hasNextPage: false,
};

describe('useAppointments', () => {
  it('fetches appointments on mount', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockPagedResult);

    const { result } = renderHook(() => useAppointments());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.items).toEqual(mockPagedResult.items);
    expect(result.current.error).toBeNull();
    expect(mockAppointmentsApiFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/appointments?'),
      expect.objectContaining({ method: 'GET' }),
    );
  });

  it('passes filter parameters as query params', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockPagedResult);

    renderHook(() =>
      useAppointments({
        status: 'Scheduled',
        type: 'PropertyViewing',
        search: 'test',
        fromUtc: '2026-03-01T00:00:00Z',
        toUtc: '2026-03-31T00:00:00Z',
      }),
    );

    await waitFor(() => {
      expect(mockAppointmentsApiFetch).toHaveBeenCalled();
    });

    const url = mockAppointmentsApiFetch.mock.calls[0][0] as string;
    expect(url).toContain('status=Scheduled');
    expect(url).toContain('type=PropertyViewing');
    expect(url).toContain('search=test');
    expect(url).toContain('fromUtc=');
    expect(url).toContain('toUtc=');
  });

  it('sets error on fetch failure', async () => {
    mockAppointmentsApiFetch.mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useAppointments());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.error).toBeTruthy();
    expect(result.current.items).toEqual([]);
  });

  it('refetch triggers a new API call', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockPagedResult);

    const { result } = renderHook(() => useAppointments());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    const callCountAfterMount = mockAppointmentsApiFetch.mock.calls.length;

    await act(async () => {
      await result.current.refetch();
    });

    expect(mockAppointmentsApiFetch.mock.calls.length).toBeGreaterThan(callCountAfterMount);
  });
});
