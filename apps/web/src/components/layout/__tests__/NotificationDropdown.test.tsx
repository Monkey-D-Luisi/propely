// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { NotificationDropdown } from '@/components/layout/NotificationDropdown';
import { renderWithProviders, screen } from '@test/utils';
import type { Notification } from '@/lib/schemas';

beforeEach(() => {
  vi.resetAllMocks();
});

const createNotification = (overrides?: Partial<Notification>): Notification => ({
  id: '1',
  type: 'RoleChanged',
  title: 'Role changed',
  body: 'Your role was changed.',
  isRead: false,
  createdAtUtc: new Date().toISOString(),
  metadata: null,
  ...overrides,
});

describe('NotificationDropdown', () => {
  it('shows empty state when no notifications', () => {
    renderWithProviders(
      <NotificationDropdown
        notifications={[]}
        unreadCount={0}
        onMarkAsRead={vi.fn()}
        onMarkAllAsRead={vi.fn()}
      />,
    );

    expect(screen.getByText('No notifications yet')).toBeInTheDocument();
  });

  it('renders notifications', () => {
    const notifications = [
      createNotification({ id: '1', title: 'First notification', body: 'First body' }),
      createNotification({ id: '2', title: 'Second notification', body: 'Second body' }),
    ];

    renderWithProviders(
      <NotificationDropdown
        notifications={notifications}
        unreadCount={2}
        onMarkAsRead={vi.fn()}
        onMarkAllAsRead={vi.fn()}
      />,
    );

    expect(screen.getByText('First notification')).toBeInTheDocument();
    expect(screen.getByText('Second notification')).toBeInTheDocument();
    expect(screen.getByText('First body')).toBeInTheDocument();
    expect(screen.getByText('Second body')).toBeInTheDocument();
  });

  it('shows mark all read button when there are unread notifications', () => {
    renderWithProviders(
      <NotificationDropdown
        notifications={[createNotification()]}
        unreadCount={1}
        onMarkAsRead={vi.fn()}
        onMarkAllAsRead={vi.fn()}
      />,
    );

    expect(screen.getByText('Mark all as read')).toBeInTheDocument();
  });

  it('hides mark all read button when unread count is zero', () => {
    renderWithProviders(
      <NotificationDropdown
        notifications={[createNotification({ isRead: true })]}
        unreadCount={0}
        onMarkAsRead={vi.fn()}
        onMarkAllAsRead={vi.fn()}
      />,
    );

    expect(screen.queryByText('Mark all as read')).not.toBeInTheDocument();
  });

  it('calls onMarkAllAsRead when mark all button is clicked', async () => {
    const mockMarkAllAsRead = vi.fn().mockResolvedValue(undefined);

    const { user } = renderWithProviders(
      <NotificationDropdown
        notifications={[createNotification()]}
        unreadCount={1}
        onMarkAsRead={vi.fn()}
        onMarkAllAsRead={mockMarkAllAsRead}
      />,
    );

    await user.click(screen.getByText('Mark all as read'));

    expect(mockMarkAllAsRead).toHaveBeenCalledOnce();
  });

  it('calls onMarkAsRead when an unread notification is clicked', async () => {
    const mockMarkAsRead = vi.fn().mockResolvedValue(undefined);

    const { user } = renderWithProviders(
      <NotificationDropdown
        notifications={[createNotification({ id: 'notif-1' })]}
        unreadCount={1}
        onMarkAsRead={mockMarkAsRead}
        onMarkAllAsRead={vi.fn()}
      />,
    );

    await user.click(screen.getByText('Role changed'));

    expect(mockMarkAsRead).toHaveBeenCalledWith('notif-1');
  });

  it('does not call onMarkAsRead when a read notification is clicked', async () => {
    const mockMarkAsRead = vi.fn().mockResolvedValue(undefined);

    const { user } = renderWithProviders(
      <NotificationDropdown
        notifications={[createNotification({ id: 'notif-1', isRead: true })]}
        unreadCount={0}
        onMarkAsRead={mockMarkAsRead}
        onMarkAllAsRead={vi.fn()}
      />,
    );

    await user.click(screen.getByText('Role changed'));

    expect(mockMarkAsRead).not.toHaveBeenCalled();
  });

  it('displays the title header', () => {
    renderWithProviders(
      <NotificationDropdown
        notifications={[]}
        unreadCount={0}
        onMarkAsRead={vi.fn()}
        onMarkAllAsRead={vi.fn()}
      />,
    );

    expect(screen.getByText('Notifications')).toBeInTheDocument();
  });
});
