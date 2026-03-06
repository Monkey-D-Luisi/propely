// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import { useRequireAuth } from '@/hooks/useRequireAuth';
import { useAuditLogs, useExportAuditLogs } from '@/hooks/audit-logs';
import type { AuditLogFilters as FilterState } from '@/hooks/audit-logs';
import { AuditLogTable } from '@/components/admin/AuditLogTable';
import { AuditLogFilters } from '@/components/admin/AuditLogFilters';
import { Pagination } from '@/components/ui/pagination';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';

export default function AuditLogsPage() {
  const t = useTranslations('auditLogs');
  const { isLoading: userLoading } = useRequireAuth();
  const { toast } = useToast();

  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState<FilterState>({});
  const [debouncedFilters, setDebouncedFilters] = useState<FilterState>({});

  // Debounce filter changes
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedFilters(filters);
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [filters]);

  const { logs, pagination, isLoading, error } = useAuditLogs(page, 50, debouncedFilters);
  const { exportLogs, isExporting } = useExportAuditLogs();

  const handleExport = useCallback(
    async (format: 'csv' | 'json') => {
      try {
        await exportLogs(format, debouncedFilters);
        toast({ title: t('exportSuccess'), variant: 'success' });
      } catch {
        toast({ title: t('exportError'), variant: 'destructive' });
      }
    },
    [exportLogs, debouncedFilters, toast, t],
  );

  if (userLoading) {
    return (
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
        <div className="animate-pulse space-y-4">
          <div className="h-8 w-48 rounded-lg bg-slate-200" />
          <div className="h-4 w-72 rounded-lg bg-slate-100" />
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
      <div className="flex flex-col gap-4 border-b border-slate-100 pb-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => void handleExport('csv')}
            disabled={isExporting}
            className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {t('exportCsv')}
          </button>
          <button
            type="button"
            onClick={() => void handleExport('json')}
            disabled={isExporting}
            className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {t('exportJson')}
          </button>
        </div>
      </div>

      <AuditLogFilters filters={filters} onFiltersChange={setFilters} />

      {error ? (
        <ErrorMessage message={t('loadError')} />
      ) : (
        <>
          <AuditLogTable logs={logs} isLoading={isLoading} />
          <Pagination pagination={pagination} onPageChange={setPage} />
        </>
      )}
    </div>
  );
}
