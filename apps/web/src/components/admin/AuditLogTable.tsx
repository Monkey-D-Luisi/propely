// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { Fragment, useState } from 'react';
import { useTranslations } from 'next-intl';
import { useLocale } from 'next-intl';
import type { AuditLog } from '@/lib/schemas';
import { formatDateTime } from '@/lib/format';

interface AuditLogTableProps {
  logs: AuditLog[];
  isLoading: boolean;
}

function formatChanges(changes: string | null): string {
  if (!changes) return '';
  try {
    return JSON.stringify(JSON.parse(changes), null, 2);
  } catch {
    return changes;
  }
}

export function AuditLogTable({ logs, isLoading }: AuditLogTableProps) {
  const t = useTranslations('auditLogs');
  const locale = useLocale();
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());

  const toggleRow = (id: string) => {
    setExpandedRows((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  if (isLoading) {
    return (
      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="animate-pulse space-y-3 p-5">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-10 rounded-lg bg-slate-100" />
          ))}
        </div>
      </div>
    );
  }

  if (logs.length === 0) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-8 text-center shadow-sm">
        <p className="text-sm text-slate-500">{t('emptyState')}</p>
      </div>
    );
  }

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <table className="min-w-full text-left text-sm">
        <thead className="border-b border-slate-200 bg-slate-50/50">
          <tr>
            <th className="w-8 px-4 py-4" />
            <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
              {t('columns.timestamp')}
            </th>
            <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
              {t('columns.action')}
            </th>
            <th className="hidden px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 md:table-cell">
              {t('columns.entityType')}
            </th>
            <th className="hidden px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 lg:table-cell">
              {t('columns.entityId')}
            </th>
            <th className="hidden px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 xl:table-cell">
              {t('columns.userId')}
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {logs.map((log) => {
            const isExpanded = expandedRows.has(log.id);
            return (
              <Fragment key={log.id}>
                <tr className="group hover:bg-slate-50 transition-colors">
                  <td className="px-4 py-4">
                    {log.changes && (
                      <button
                        type="button"
                        onClick={() => toggleRow(log.id)}
                        className="rounded-lg p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-600 focus:outline-none focus:ring-2 focus:ring-primary-600"
                        aria-expanded={isExpanded}
                        aria-label={isExpanded ? t('collapseRow') : t('expandRow')}
                      >
                        <svg
                          className={`h-4 w-4 transition-transform ${isExpanded ? 'rotate-90' : ''}`}
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                        </svg>
                      </button>
                    )}
                  </td>
                  <td className="whitespace-nowrap px-6 py-5 text-slate-600">
                    {formatDateTime(log.createdAtUtc, locale)}
                  </td>
                  <td className="px-6 py-5">
                    <span className="inline-flex items-center rounded-full border border-slate-200 bg-slate-100 px-2.5 py-0.5 text-xs font-medium text-slate-800">
                      {log.action}
                    </span>
                  </td>
                  <td className="hidden px-6 py-5 font-mono text-xs text-slate-600 md:table-cell">
                    {log.entityType}
                  </td>
                  <td className="hidden px-6 py-5 font-mono text-xs text-slate-500 lg:table-cell">
                    {log.entityId}
                  </td>
                  <td className="hidden px-6 py-5 font-mono text-xs text-slate-500 xl:table-cell">
                    {log.userId ?? '—'}
                  </td>
                </tr>
                {isExpanded && log.changes && (
                  <tr key={`${log.id}-changes`}>
                    <td colSpan={6} className="border-t border-slate-100 bg-slate-50 px-6 py-4">
                      <p className="mb-1 text-xs font-medium text-slate-500">{t('columns.changes')}</p>
                      <pre className="max-h-64 overflow-auto rounded-lg bg-slate-900 p-3 text-xs text-slate-100">
                        {formatChanges(log.changes)}
                      </pre>
                    </td>
                  </tr>
                )}
              </Fragment>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
