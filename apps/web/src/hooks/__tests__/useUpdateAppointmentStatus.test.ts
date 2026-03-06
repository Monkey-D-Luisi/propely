// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useUpdateAppointmentStatus } from '@/hooks/useUpdateAppointmentStatus';

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
  title: 'Property Viewing',
  type: 'PropertyViewing',
  status: 'Confirmed',
  startTimeUtc: '2026-03-10T10:00:00Z',
  endTimeUtc: '2026-03-10T11:00:00Z',
  location: null,
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

const appointmentId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

describe('useUpdateAppointmentStatus', () => {
  it('calls confirm endpoint for Confirmed status', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockAppointment);

    const { result } = renderHook(() => useUpdateAppointmentStatus());

    await act(async () => {
      await result.current(appointmentId, 'Confirmed');
    });

    expect(mockAppointmentsApiFetch).toHaveBeenCalledWith(
      `/api/appointments/${appointmentId}/confirm`,
      expect.objectContaining({ method: 'PUT' }),
    );
  });

  it('calls complete endpoint for Completed status with empty body', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockAppointment);

    const { result } = renderHook(() => useUpdateAppointmentStatus());

    await act(async () => {
      await result.current(appointmentId, 'Completed');
    });

    expect(mockAppointmentsApiFetch).toHaveBeenCalledWith(
      `/api/appointments/${appointmentId}/complete`,
      expect.objectContaining({ method: 'PUT', body: JSON.stringify({}) }),
    );
  });

  it('calls cancel endpoint for Cancelled status with reason in body', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockAppointment);

    const { result } = renderHook(() => useUpdateAppointmentStatus());

    await act(async () => {
      await result.current(appointmentId, 'Cancelled', 'Client unavailable');
    });

    expect(mockAppointmentsApiFetch).toHaveBeenCalledWith(
      `/api/appointments/${appointmentId}/cancel`,
      expect.objectContaining({
        method: 'PUT',
        body: JSON.stringify({ reason: 'Client unavailable' }),
      }),
    );
  });

  it('calls cancel endpoint without body when no cancellation reason', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockAppointment);

    const { result } = renderHook(() => useUpdateAppointmentStatus());

    await act(async () => {
      await result.current(appointmentId, 'Cancelled');
    });

    expect(mockAppointmentsApiFetch).toHaveBeenCalledWith(
      `/api/appointments/${appointmentId}/cancel`,
      expect.objectContaining({ method: 'PUT' }),
    );
    // Should not have body when no reason
    const callArgs = mockAppointmentsApiFetch.mock.calls[0][1] as Record<string, unknown>;
    expect(callArgs.body).toBeUndefined();
  });

  it('calls no-show endpoint for NoShow status', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(mockAppointment);

    const { result } = renderHook(() => useUpdateAppointmentStatus());

    await act(async () => {
      await result.current(appointmentId, 'NoShow');
    });

    expect(mockAppointmentsApiFetch).toHaveBeenCalledWith(
      `/api/appointments/${appointmentId}/no-show`,
      expect.objectContaining({ method: 'PUT' }),
    );
  });

  it('throws for unsupported status', async () => {
    const { result } = renderHook(() => useUpdateAppointmentStatus());

    await expect(
      act(async () => {
        await result.current(appointmentId, 'Scheduled');
      }),
    ).rejects.toThrow("No status transition endpoint for status 'Scheduled'");
  });
});
