// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { Notification } from '@/lib/schemas';

type Props = {
  notifications: Notification[];
  unreadCount: number;
  onMarkAsRead: (id: string) => Promise<void>;
  onMarkAllAsRead: () => Promise<void>;
};

function useTimeAgo() {
  const t = useTranslations('notifications.timeAgo');

  return (dateStr: string): string => {
    const now = Date.now();
    const then = new Date(dateStr).getTime();
    const diffMs = now - then;
    const diffMin = Math.floor(diffMs / 60000);
    const diffHr = Math.floor(diffMin / 60);
    const diffDay = Math.floor(diffHr / 24);

    if (diffMin < 1) return t('justNow');
    if (diffMin < 60) return t('minutesAgo', { count: diffMin });
    if (diffHr < 24) return t('hoursAgo', { count: diffHr });
    return t('daysAgo', { count: diffDay });
  };
}

export function NotificationDropdown({
  notifications,
  unreadCount,
  onMarkAsRead,
  onMarkAllAsRead,
}: Props) {
  const t = useTranslations('notifications');
  const timeAgo = useTimeAgo();

  return (
    <div
      role="menu"
      aria-label={t('title')}
      className="absolute right-0 top-full z-50 mt-2 w-80 rounded-lg border border-slate-200 bg-white shadow-lg"
    >
      <div role="none" className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
        <h3 className="text-sm font-semibold text-slate-900">{t('title')}</h3>
        {unreadCount > 0 && (
          <button
            type="button"
            role="menuitem"
            onClick={() => void onMarkAllAsRead()}
            className="text-xs font-medium text-blue-600 transition hover:text-blue-800"
          >
            {t('markAllRead')}
          </button>
        )}
      </div>

      <div role="none" className="max-h-80 overflow-y-auto">
        {notifications.length === 0 ? (
          <div role="none" className="px-4 py-8 text-center text-sm text-slate-500">
            {t('empty')}
          </div>
        ) : (
          <ul role="none">
            {notifications.map((notification) => (
              <li key={notification.id} role="none">
                <button
                  type="button"
                  role="menuitem"
                  onClick={() => {
                    if (!notification.isRead) {
                      void onMarkAsRead(notification.id);
                    }
                  }}
                  className={`w-full px-4 py-3 text-left transition hover:bg-slate-50 ${
                    !notification.isRead ? 'bg-blue-50/50' : ''
                  }`}
                >
                  <div className="flex items-start gap-2">
                    {!notification.isRead && (
                      <span className="mt-1.5 h-2 w-2 flex-shrink-0 rounded-full bg-blue-500" />
                    )}
                    <div className={!notification.isRead ? '' : 'ml-4'}>
                      <p className="text-sm font-medium text-slate-900">
                        {notification.title}
                      </p>
                      <p className="mt-0.5 text-xs text-slate-600">
                        {notification.body}
                      </p>
                      <p className="mt-1 text-xs text-slate-400">
                        {timeAgo(notification.createdAtUtc)}
                      </p>
                    </div>
                  </div>
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
