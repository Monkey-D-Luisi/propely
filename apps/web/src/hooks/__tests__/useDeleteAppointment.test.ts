// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useDeleteAppointment } from '@/hooks/useDeleteAppointment';

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

describe('useDeleteAppointment', () => {
  it('calls DELETE /api/appointments/:id', async () => {
    mockAppointmentsApiFetch.mockResolvedValue(undefined);

    const { result } = renderHook(() => useDeleteAppointment());

    await act(async () => {
      await result.current(appointmentId);
    });

    expect(mockAppointmentsApiFetch).toHaveBeenCalledWith(
      `/api/appointments/${appointmentId}`,
      expect.objectContaining({ method: 'DELETE' }),
    );
  });

  it('propagates errors from the API fetch', async () => {
    mockAppointmentsApiFetch.mockRejectedValue(new Error('Not found'));

    const { result } = renderHook(() => useDeleteAppointment());

    await expect(
      act(async () => {
        await result.current(appointmentId);
      }),
    ).rejects.toThrow('Not found');
  });

  it('returns a stable callback reference', () => {
    const { result, rerender } = renderHook(() => useDeleteAppointment());
    const first = result.current;
    rerender();
    expect(result.current).toBe(first);
  });
});
