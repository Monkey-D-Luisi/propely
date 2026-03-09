// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License.

'use client';

import { useTranslations, useLocale } from 'next-intl';
import type { LeadListItem, LeadStatus } from '@/hooks/useLeads';
import { LeadStatusBadge } from './LeadStatusBadge';
import { SkeletonTable } from '@/components/ui/skeleton';

interface LeadListViewProps {
  items: LeadListItem[];
  isLoading: boolean;
  onViewDetails: (id: string) => void;
  onChangeStatus: (id: string, status: LeadStatus) => void;
  onConvert: (id: string) => void;
}

function formatDate(dateStr: string | null | undefined, locale: string): string {
  if (!dateStr) return '\u2014';
  return new Intl.DateTimeFormat(locale, { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(dateStr));
}

const VALID_TRANSITIONS: Record<LeadStatus, LeadStatus[]> = {
  New: ['Contacted', 'Lost'],
  Contacted: ['Qualified', 'Lost'],
  Qualified: ['Converted', 'Lost'],
  Converted: [],
  Lost: [],
};

export function LeadListView({ items, isLoading, onViewDetails, onChangeStatus, onConvert }: LeadListViewProps) {
  const t = useTranslations('leads');
  const locale = useLocale();

  if (isLoading) {
    return <SkeletonTable rows={5} columns={7} />;
  }

  return (
    <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm" data-testid="lead-list-view">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-slate-100 bg-slate-50/50">
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.name')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.email')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.status')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.source')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.created')}</th>
            <th className="px-4 py-3 font-medium text-slate-500"><span className="sr-only">{t('table.actions')}</span></th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {items.map((item) => {
            const availableStatuses = VALID_TRANSITIONS[item.status] ?? [];
            return (
              <tr key={item.id} className="transition hover:bg-slate-50/50">
                <td className="px-4 py-3">
                  <button
                    type="button"
                    onClick={() => onViewDetails(item.id)}
                    className="font-medium text-slate-900 hover:text-primary-600"
                  >
                    {item.name}
                  </button>
                </td>
                <td className="px-4 py-3 text-slate-600">{item.email}</td>
                <td className="px-4 py-3">
                  <LeadStatusBadge status={item.status} label={t(`status.${item.status}`)} />
                </td>
                <td className="px-4 py-3 text-slate-500">{item.source ?? '\u2014'}</td>
                <td className="px-4 py-3 text-slate-500">{formatDate(item.createdAtUtc, locale)}</td>
                <td className="px-4 py-3">
                  <div className="flex items-center gap-1">
                    {availableStatuses.length > 0 && (
                      <select
                        value=""
                        onChange={(e) => {
                          if (e.target.value) onChangeStatus(item.id, e.target.value as LeadStatus);
                        }}
                        className="rounded-lg border border-slate-200 px-2 py-1 text-xs text-slate-600 transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
                        aria-label={t('card.changeStatus')}
                      >
                        <option value="">{t('card.changeStatus')}</option>
                        {availableStatuses.map((s) => (
                          <option key={s} value={s}>{t(`status.${s}`)}</option>
                        ))}
                      </select>
                    )}
                    {/* Only Qualified leads can be converted — domain invariant */}
                    {item.status === 'Qualified' && (
                      <button
                        type="button"
                        onClick={() => onConvert(item.id)}
                        className="rounded-lg px-2 py-1 text-xs font-medium text-primary-600 transition hover:bg-primary-50"
                      >
                        {t('actions.convert')}
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
