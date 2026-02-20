// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { apiFetch } from '@/lib/api';
import type { AuditLog, PagedResponse } from '@/lib/schemas';
import { AuditLogsResponseSchema } from '@/lib/schemas';
import type { PaginationState } from '@/hooks/orgs';

const emptyPagination: PaginationState = {
  pageNumber: 1,
  totalPages: 0,
  totalCount: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

export type AuditLogFilters = {
  dateFrom?: string;
  dateTo?: string;
  userId?: string;
  action?: string;
  entityType?: string;
  entityId?: string;
};

export function useAuditLogs(page = 1, pageSize = 50, filters: AuditLogFilters = {}) {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [pagination, setPagination] = useState<PaginationState>(emptyPagination);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
      });
      if (filters.dateFrom) params.set('dateFrom', filters.dateFrom);
      if (filters.dateTo) params.set('dateTo', filters.dateTo);
      if (filters.userId) params.set('userId', filters.userId);
      if (filters.action) params.set('action', filters.action);
      if (filters.entityType) params.set('entityType', filters.entityType);
      if (filters.entityId) params.set('entityId', filters.entityId);

      const data = await apiFetch<PagedResponse<AuditLog>>(
        `/admin/audit-logs?${params}`,
        { method: 'GET', signal },
        AuditLogsResponseSchema,
      );
      const { items, ...paginationData } = data;
      setLogs(items);
      setPagination(paginationData);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [page, pageSize, filters.dateFrom, filters.dateTo, filters.userId, filters.action, filters.entityType, filters.entityId]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { logs, pagination, isLoading, error, refetch: fetcher };
}

export function useExportAuditLogs() {
  const [isExporting, setExporting] = useState(false);

  const exportLogs = useCallback(async (format: 'csv' | 'json', filters: AuditLogFilters = {}) => {
    setExporting(true);
    try {
      const params = new URLSearchParams({ format });
      if (filters.dateFrom) params.set('dateFrom', filters.dateFrom);
      if (filters.dateTo) params.set('dateTo', filters.dateTo);
      if (filters.userId) params.set('userId', filters.userId);
      if (filters.action) params.set('action', filters.action);
      if (filters.entityType) params.set('entityType', filters.entityType);
      if (filters.entityId) params.set('entityId', filters.entityId);

      const API_BASE = process.env.NEXT_PUBLIC_ORGS_API_URL ?? 'http://localhost:5020';
      const res = await fetch(`${API_BASE}/admin/audit-logs/export?${params}`, {
        credentials: 'include',
      });

      if (!res.ok) throw new Error(`Export failed: ${res.status}`);

      const blob = await res.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `audit-logs.${format}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } finally {
      setExporting(false);
    }
  }, []);

  return { exportLogs, isExporting };
}
