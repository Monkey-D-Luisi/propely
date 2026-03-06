// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { useAppointment } from '@/hooks/useAppointment';

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

const appointmentId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

const mockAppointment = {
  id: appointmentId,
  title: 'Property Viewing',
  type: 'PropertyViewing',
  status: 'Scheduled',
  startTimeUtc: '2026-03-10T10:00:00Z',
  endTimeUtc: '2026-03-10T11:00:00Z',
  location: 'Calle Mayor 10',
  isAllDay: false,
  propertyId: null,
  contactId: null,
  agentId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
  description: null,
  tenantId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
  notes: null,
  cancellationReason: null,
  createdAtUtc: '2026-03-01T10:00:00Z',
  updatedAtUtc: null,
};

describe('useAppointment', () => {
  it('fetches appointment by id on mount', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockAppointment);

    const { result } = renderHook(() => useAppointment(appointmentId));

    // Initially loading
    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.appointment).toEqual(mockAppointment);
    expect(result.current.error).toBeNull();
    expect(mockAppointmentsApiFetch).toHaveBeenCalledWith(
      `/api/appointments/${appointmentId}`,
      expect.objectContaining({ method: 'GET' }),
    );
  });

  it('returns null appointment when id is null', async () => {
    const { result } = renderHook(() => useAppointment(null));

    expect(result.current.appointment).toBeNull();
    expect(result.current.isLoading).toBe(false);
    expect(mockAppointmentsApiFetch).not.toHaveBeenCalled();
  });

  it('sets error on fetch failure', async () => {
    const error = new Error('Server error');
    mockAppointmentsApiFetch.mockRejectedValue(error);

    const { result } = renderHook(() => useAppointment(appointmentId));

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.error).toBe(error);
    expect(result.current.appointment).toBeNull();
  });

  it('refetch triggers a new API call', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockAppointment);

    const { result } = renderHook(() => useAppointment(appointmentId));

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(mockAppointmentsApiFetch).toHaveBeenCalledTimes(1);

    await act(async () => {
      await result.current.refetch();
    });

    expect(mockAppointmentsApiFetch).toHaveBeenCalledTimes(2);
  });
});
