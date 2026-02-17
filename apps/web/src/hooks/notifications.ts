// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { apiFetch } from "@/lib/api";
import {
  type Notification,
  type NotificationsResponse,
  NotificationsResponseSchema,
} from "@/lib/schemas";

const POLL_INTERVAL_MS = 30_000;

export function useNotifications(page = 1, pageSize = 10) {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const fetchingRef = useRef(false);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    if (fetchingRef.current) return;
    fetchingRef.current = true;
    try {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
      });
      const data = await apiFetch<NotificationsResponse>(
        `/notifications?${params}`,
        { method: "GET", signal },
        NotificationsResponseSchema,
      );
      setNotifications(data.items);
      setUnreadCount(data.unreadCount);
      setError(null);
    } catch (e) {
      if (e instanceof DOMException && e.name === "AbortError") return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
      fetchingRef.current = false;
    }
  }, [page, pageSize]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);

    intervalRef.current = setInterval(() => {
      void fetcher(controller.signal);
    }, POLL_INTERVAL_MS);

    return () => {
      controller.abort();
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [fetcher]);

  const markAsRead = useCallback(
    async (notificationId: string) => {
      try {
        await apiFetch(`/notifications/${notificationId}/read`, {
          method: "PATCH",
        });
        setNotifications((prev) =>
          prev.map((n) =>
            n.id === notificationId ? { ...n, isRead: true } : n,
          ),
        );
        setUnreadCount((prev) => Math.max(0, prev - 1));
      } catch {
        setError(new Error("markReadError"));
      }
    },
    [],
  );

  const markAllAsRead = useCallback(async () => {
    try {
      await apiFetch("/notifications/read-all", {
        method: "PATCH",
      });
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch {
      setError(new Error("markAllReadError"));
    }
  }, []);

  return {
    notifications,
    unreadCount,
    isLoading,
    error,
    refetch: fetcher,
    markAsRead,
    markAllAsRead,
  };
}
