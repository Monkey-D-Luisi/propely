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
      <main className="flex-1 w-full max-w-6xl mx-auto px-4 lg:px-8 py-8 flex flex-col gap-6">
        <div className="animate-pulse space-y-4">
          <div className="h-8 w-48 rounded-lg bg-slate-200" />
          <div className="h-4 w-72 rounded-lg bg-slate-100" />
        </div>
      </main>
    );
  }

  return (
    <main className="flex-1 w-full max-w-6xl mx-auto px-4 lg:px-8 py-8 flex flex-col gap-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between mb-2">
        <div className="flex items-center gap-3">
          <h1 className="text-slate-900 tracking-tight text-2xl lg:text-3xl font-bold leading-tight">{t('title')}</h1>
          <span className="flex items-center justify-center rounded-full h-6 px-2.5 bg-red-50 text-red-600 text-xs font-semibold border border-red-100">
            Admin Only
          </span>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => void handleExport('csv')}
            disabled={isExporting}
            className="flex items-center justify-center gap-2 rounded-lg h-10 px-4 bg-white text-slate-700 text-sm font-semibold border border-slate-200 shadow-sm hover:bg-slate-50 transition-colors disabled:opacity-50"
          >
            <span className="material-symbols-outlined text-[18px]" aria-hidden="true">download</span>
            {t('exportCsv')}
          </button>
          <button
            type="button"
            onClick={() => void handleExport('json')}
            disabled={isExporting}
            className="flex items-center justify-center gap-2 rounded-lg h-10 px-4 bg-white text-slate-700 text-sm font-semibold border border-slate-200 shadow-sm hover:bg-slate-50 transition-colors disabled:opacity-50"
          >
            <span className="material-symbols-outlined text-[18px]" aria-hidden="true">download</span>
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
    </main>
  );
}
