// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { LeadStatus } from '@/hooks/useLeads';

interface LeadFiltersProps {
  search: string;
  onSearchChange: (value: string) => void;
  status: LeadStatus | '';
  onStatusChange: (value: LeadStatus | '') => void;
}

const ALL_STATUSES: LeadStatus[] = ['New', 'Contacted', 'Qualified', 'Converted', 'Lost'];

export function LeadFilters({ search, onSearchChange, status, onStatusChange }: LeadFiltersProps) {
  const t = useTranslations('leads.filters');
  const tStatus = useTranslations('leads.status');

  return (
    <div className="flex flex-col gap-3 sm:flex-row" data-testid="lead-filters">
      <div className="relative flex-1">
        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-lg text-slate-400" aria-hidden="true">search</span>
        <input
          type="text"
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder={t('searchPlaceholder')}
          className="w-full rounded-lg border border-slate-200 py-2 pl-10 pr-3 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
        />
      </div>
      <select
        value={status}
        onChange={(e) => onStatusChange(e.target.value as LeadStatus | '')}
        className="rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
      >
        <option value="">{t('allStatuses')}</option>
        {ALL_STATUSES.map((s) => (
          <option key={s} value={s}>{tStatus(s)}</option>
        ))}
      </select>
    </div>
  );
}
