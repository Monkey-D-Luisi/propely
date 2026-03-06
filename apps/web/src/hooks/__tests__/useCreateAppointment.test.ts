// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useCreateAppointment } from '@/hooks/useCreateAppointment';
import type { CreateAppointmentRequest } from '@/hooks/useCreateAppointment';

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

const mockAppointment = {
  id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  title: 'Property Viewing - Calle Mayor',
  type: 'PropertyViewing',
  status: 'Scheduled',
  startTimeUtc: '2026-03-10T10:00:00Z',
  endTimeUtc: '2026-03-10T11:00:00Z',
  location: 'Calle Mayor 10',
  isAllDay: false,
  propertyId: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
  contactId: null,
  agentId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
  description: 'Viewing for client',
  tenantId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
  notes: null,
  cancellationReason: null,
  createdAtUtc: '2026-03-01T10:00:00Z',
  updatedAtUtc: null,
};

describe('useCreateAppointment', () => {
  it('calls POST /api/appointments with the request body', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockAppointment);

    const { result } = renderHook(() => useCreateAppointment());

    const request: CreateAppointmentRequest = {
      title: 'Property Viewing - Calle Mayor',
      type: 'PropertyViewing',
      startTimeUtc: '2026-03-10T10:00:00Z',
      endTimeUtc: '2026-03-10T11:00:00Z',
      description: 'Viewing for client',
      location: 'Calle Mayor 10',
      isAllDay: false,
      propertyId: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
    };

    let created: unknown;
    await act(async () => {
      created = await result.current(request);
    });

    expect(mockAppointmentsApiFetch).toHaveBeenCalledWith(
      '/api/appointments',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify(request),
      }),
    );
    expect(created).toEqual(mockAppointment);
  });

  it('sends minimal request without optional fields', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockAppointment);

    const { result } = renderHook(() => useCreateAppointment());

    const request: CreateAppointmentRequest = {
      title: 'Team Meeting',
      type: 'Generic',
      startTimeUtc: '2026-03-10T10:00:00Z',
      endTimeUtc: '2026-03-10T11:00:00Z',
    };

    await act(async () => {
      await result.current(request);
    });

    expect(mockAppointmentsApiFetch).toHaveBeenCalledWith(
      '/api/appointments',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify(request),
      }),
    );
  });

  it('propagates errors from the API fetch', async () => {
    mockAppointmentsApiFetch.mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useCreateAppointment());

    const request: CreateAppointmentRequest = {
      title: 'Test',
      type: 'Generic',
      startTimeUtc: '2026-03-10T10:00:00Z',
      endTimeUtc: '2026-03-10T11:00:00Z',
    };

    await expect(
      act(async () => {
        await result.current(request);
      }),
    ).rejects.toThrow('Network error');
  });

  it('returns a stable callback reference', () => {
    const { result, rerender } = renderHook(() => useCreateAppointment());
    const first = result.current;
    rerender();
    expect(result.current).toBe(first);
  });
});
