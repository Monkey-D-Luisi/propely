// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useCallback } from 'react';
import type { AuditLog } from '@/lib/schemas';
import { AuditLogsResponseSchema } from '@/lib/schemas';
import { usePaginatedFetch } from '@/hooks/use-fetch';

export type AuditLogFilters = {
  dateFrom?: string;
  dateTo?: string;
  userId?: string;
  action?: string;
  entityType?: string;
  entityId?: string;
};

function buildAuditLogsUrl(page: number, pageSize: number, filters: AuditLogFilters): string {
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
  return `/admin/audit-logs?${params}`;
}

export function useAuditLogs(page = 1, pageSize = 50, filters: AuditLogFilters = {}) {
  const url = buildAuditLogsUrl(page, pageSize, filters);
  const { items: logs, pagination, isLoading, error, refetch } = usePaginatedFetch<AuditLog>(url, {
    schema: AuditLogsResponseSchema,
  });
  return { logs, pagination, isLoading, error, refetch };
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
