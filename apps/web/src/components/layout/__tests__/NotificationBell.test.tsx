// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { NotificationBell } from '@/components/layout/NotificationBell';
import { renderWithProviders, screen } from '@test/utils';

vi.mock('@/hooks/notifications', () => ({
  useNotifications: vi.fn(),
}));

import { useNotifications } from '@/hooks/notifications';

const mockUseNotifications = vi.mocked(useNotifications);

beforeEach(() => {
  vi.resetAllMocks();
});

describe('NotificationBell', () => {
  it('renders the bell button', () => {
    mockUseNotifications.mockReturnValue({
      notifications: [],
      unreadCount: 0,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
      markAsRead: vi.fn(),
      markAllAsRead: vi.fn(),
    });

    renderWithProviders(<NotificationBell />);

    expect(screen.getByRole('button', { name: 'Notifications' })).toBeInTheDocument();
  });

  it('shows unread badge when there are unread notifications', () => {
    mockUseNotifications.mockReturnValue({
      notifications: [],
      unreadCount: 3,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
      markAsRead: vi.fn(),
      markAllAsRead: vi.fn(),
    });

    renderWithProviders(<NotificationBell />);

    expect(screen.getByText('3')).toBeInTheDocument();
  });

  it('does not show badge when unread count is zero', () => {
    mockUseNotifications.mockReturnValue({
      notifications: [],
      unreadCount: 0,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
      markAsRead: vi.fn(),
      markAllAsRead: vi.fn(),
    });

    renderWithProviders(<NotificationBell />);

    expect(screen.queryByText('0')).not.toBeInTheDocument();
  });

  it('shows 99+ when unread count exceeds 99', () => {
    mockUseNotifications.mockReturnValue({
      notifications: [],
      unreadCount: 150,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
      markAsRead: vi.fn(),
      markAllAsRead: vi.fn(),
    });

    renderWithProviders(<NotificationBell />);

    expect(screen.getByText('99+')).toBeInTheDocument();
  });

  it('opens dropdown when bell is clicked', async () => {
    mockUseNotifications.mockReturnValue({
      notifications: [],
      unreadCount: 0,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
      markAsRead: vi.fn(),
      markAllAsRead: vi.fn(),
    });

    const { user } = renderWithProviders(<NotificationBell />);

    await user.click(screen.getByRole('button', { name: 'Notifications' }));

    expect(screen.getByText('No notifications yet')).toBeInTheDocument();
  });

  it('closes dropdown when bell is clicked again', async () => {
    mockUseNotifications.mockReturnValue({
      notifications: [],
      unreadCount: 0,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
      markAsRead: vi.fn(),
      markAllAsRead: vi.fn(),
    });

    const { user } = renderWithProviders(<NotificationBell />);
    const bell = screen.getByRole('button', { name: 'Notifications' });

    await user.click(bell);
    expect(screen.getByText('No notifications yet')).toBeInTheDocument();

    await user.click(bell);
    expect(screen.queryByText('No notifications yet')).not.toBeInTheDocument();
  });

  it('shows notifications in the dropdown', async () => {
    mockUseNotifications.mockReturnValue({
      notifications: [
        {
          id: '1',
          type: 'RoleChanged',
          title: 'Role changed',
          body: 'Your role was changed to admin.',
          isRead: false,
          createdAtUtc: new Date().toISOString(),
          metadata: null,
        },
      ],
      unreadCount: 1,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
      markAsRead: vi.fn(),
      markAllAsRead: vi.fn(),
    });

    const { user } = renderWithProviders(<NotificationBell />);

    await user.click(screen.getByRole('button', { name: 'Notifications' }));

    expect(screen.getByText('Role changed')).toBeInTheDocument();
    expect(screen.getByText('Your role was changed to admin.')).toBeInTheDocument();
  });
});
