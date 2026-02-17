// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { useNotifications } from '@/hooks/notifications';

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

const mockNotificationsResponse = {
  items: [
    { id: 'n1', title: 'Test notification', message: 'Hello', isRead: false, createdAtUtc: '2026-01-01T00:00:00Z' },
    { id: 'n2', title: 'Another notification', message: 'World', isRead: true, createdAtUtc: '2026-01-02T00:00:00Z' },
  ],
  unreadCount: 1,
};

describe('useNotifications', () => {
  it('starts in loading state', () => {
    mockApiFetch.mockReturnValue(new Promise(() => {}));
    const { result } = renderHook(() => useNotifications());

    expect(result.current.isLoading).toBe(true);
    expect(result.current.notifications).toEqual([]);
  });

  it('returns notifications on success', async () => {
    mockApiFetch.mockResolvedValueOnce(mockNotificationsResponse);

    const { result } = renderHook(() => useNotifications());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });
    expect(result.current.notifications).toHaveLength(2);
    expect(result.current.unreadCount).toBe(1);
  });

  it('sets error on failure', async () => {
    mockApiFetch.mockRejectedValueOnce(new Error('Network'));

    const { result } = renderHook(() => useNotifications());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });
    expect(result.current.error).toBeTruthy();
  });

  it('markAsRead updates local state optimistically', async () => {
    mockApiFetch.mockResolvedValueOnce(mockNotificationsResponse);

    const { result } = renderHook(() => useNotifications());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    mockApiFetch.mockResolvedValueOnce({});

    await act(async () => {
      await result.current.markAsRead('n1');
    });

    expect(result.current.notifications.find(n => n.id === 'n1')?.isRead).toBe(true);
    expect(result.current.unreadCount).toBe(0);
  });

  it('markAllAsRead marks all as read', async () => {
    mockApiFetch.mockResolvedValueOnce({
      items: [
        { id: 'n1', title: 'A', message: 'A', isRead: false, createdAtUtc: '2026-01-01' },
        { id: 'n2', title: 'B', message: 'B', isRead: false, createdAtUtc: '2026-01-02' },
      ],
      unreadCount: 2,
    });

    const { result } = renderHook(() => useNotifications());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    mockApiFetch.mockResolvedValueOnce({});

    await act(async () => {
      await result.current.markAllAsRead();
    });

    expect(result.current.unreadCount).toBe(0);
    expect(result.current.notifications.every(n => n.isRead)).toBe(true);
  });
});
