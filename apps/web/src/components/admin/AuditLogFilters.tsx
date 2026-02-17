// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { AuditLogFilters as FilterState } from '@/hooks/audit-logs';

interface AuditLogFiltersProps {
  filters: FilterState;
  onFiltersChange: (filters: FilterState) => void;
}

export function AuditLogFilters({ filters, onFiltersChange }: AuditLogFiltersProps) {
  const t = useTranslations('auditLogs');

  const handleChange = (key: keyof FilterState, value: string) => {
    onFiltersChange({ ...filters, [key]: value || undefined });
  };

  const handleClear = () => {
    onFiltersChange({});
  };

  const hasFilters = Object.values(filters).some(Boolean);

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white p-6 shadow-sm space-y-3 md:p-8">
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
        <div>
          <label htmlFor="filter-dateFrom" className="block text-xs font-medium text-slate-500">
            {t('filters.dateFrom')}
          </label>
          <input
            id="filter-dateFrom"
            type="datetime-local"
            value={filters.dateFrom ?? ''}
            onChange={(e) => handleChange('dateFrom', e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="filter-dateTo" className="block text-xs font-medium text-slate-500">
            {t('filters.dateTo')}
          </label>
          <input
            id="filter-dateTo"
            type="datetime-local"
            value={filters.dateTo ?? ''}
            onChange={(e) => handleChange('dateTo', e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="filter-action" className="block text-xs font-medium text-slate-500">
            {t('filters.action')}
          </label>
          <input
            id="filter-action"
            type="text"
            placeholder={t('filters.actionPlaceholder')}
            value={filters.action ?? ''}
            onChange={(e) => handleChange('action', e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="filter-entityType" className="block text-xs font-medium text-slate-500">
            {t('filters.entityType')}
          </label>
          <input
            id="filter-entityType"
            type="text"
            placeholder={t('filters.entityTypePlaceholder')}
            value={filters.entityType ?? ''}
            onChange={(e) => handleChange('entityType', e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="filter-entityId" className="block text-xs font-medium text-slate-500">
            {t('filters.entityId')}
          </label>
          <input
            id="filter-entityId"
            type="text"
            placeholder={t('filters.entityIdPlaceholder')}
            value={filters.entityId ?? ''}
            onChange={(e) => handleChange('entityId', e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="filter-userId" className="block text-xs font-medium text-slate-500">
            {t('filters.userId')}
          </label>
          <input
            id="filter-userId"
            type="text"
            placeholder={t('filters.userIdPlaceholder')}
            value={filters.userId ?? ''}
            onChange={(e) => handleChange('userId', e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
      </div>
      {hasFilters && (
        <button
          type="button"
          onClick={handleClear}
          className="text-sm font-medium text-primary-600 hover:text-primary-700"
        >
          {t('filters.clear')}
        </button>
      )}
    </div>
  );
}
