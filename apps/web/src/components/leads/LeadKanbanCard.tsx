// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { LeadListItem, LeadStatus } from '@/hooks/useLeads';
import { LeadStatusBadge } from './LeadStatusBadge';

interface LeadKanbanCardProps {
  lead: LeadListItem;
  onChangeStatus: (id: string, status: LeadStatus) => void;
  onViewDetails: (id: string) => void;
}

const ALL_STATUSES: LeadStatus[] = ['New', 'Contacted', 'Qualified', 'Converted', 'Lost'];

function daysSinceCreation(createdAtUtc: string): number {
  const now = new Date();
  const created = new Date(createdAtUtc);
  return Math.floor((now.getTime() - created.getTime()) / (1000 * 60 * 60 * 24));
}

export function LeadKanbanCard({ lead, onChangeStatus, onViewDetails }: LeadKanbanCardProps) {
  const t = useTranslations('leads');
  const days = daysSinceCreation(lead.createdAtUtc);
  const daysText = days === 0 ? t('card.today') : t('card.daysAgo', { count: days });

  const availableStatuses = ALL_STATUSES.filter((s) => s !== lead.status);

  return (
    <div
      data-testid={`lead-card-${lead.id}`}
      className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition hover:shadow-md"
    >
      <div className="mb-2 flex items-start justify-between">
        <button
          type="button"
          onClick={() => onViewDetails(lead.id)}
          className="text-sm font-medium text-slate-900 hover:text-primary-600 text-left"
        >
          {lead.name}
        </button>
        {lead.assignedAgentId && (
          <span
            className="flex h-6 w-6 items-center justify-center rounded-full bg-primary-100 text-xs font-medium text-primary-700"
            title={lead.assignedAgentId}
          >
            {lead.assignedAgentId.slice(0, 2).toUpperCase()}
          </span>
        )}
      </div>
      <p className="mb-2 truncate text-xs text-slate-500">{lead.email}</p>
      {lead.source && (
        <span className="mb-2 inline-flex items-center rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-600">
          {lead.source}
        </span>
      )}
      <div className="mt-3 flex items-center justify-between border-t border-slate-100 pt-3">
        <span className="text-xs text-slate-400">{daysText}</span>
        <select
          value=""
          onChange={(e) => {
            if (e.target.value) onChangeStatus(lead.id, e.target.value as LeadStatus);
          }}
          className="rounded-lg border border-slate-200 px-2 py-1 text-xs text-slate-600 transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
          aria-label={t('card.changeStatus')}
        >
          <option value="">{t('card.changeStatus')}</option>
          {availableStatuses.map((s) => (
            <option key={s} value={s}>{t(`status.${s}`)}</option>
          ))}
        </select>
      </div>
    </div>
  );
}
