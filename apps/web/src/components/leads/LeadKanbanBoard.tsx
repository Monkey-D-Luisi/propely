// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { LeadListItem, LeadStatus } from '@/hooks/useLeads';
import { LeadKanbanCard } from './LeadKanbanCard';
import { SkeletonTable } from '@/components/ui/skeleton';

interface LeadKanbanBoardProps {
  items: LeadListItem[];
  isLoading: boolean;
  onChangeStatus: (id: string, status: LeadStatus) => void;
  onViewDetails: (id: string) => void;
}

const COLUMNS: LeadStatus[] = ['New', 'Contacted', 'Qualified', 'Converted', 'Lost'];

const columnColors: Record<LeadStatus, string> = {
  New: 'bg-blue-500',
  Contacted: 'bg-amber-500',
  Qualified: 'bg-green-500',
  Converted: 'bg-purple-500',
  Lost: 'bg-red-500',
};

export function LeadKanbanBoard({ items, isLoading, onChangeStatus, onViewDetails }: LeadKanbanBoardProps) {
  const t = useTranslations('leads');

  if (isLoading) {
    return <SkeletonTable rows={5} columns={5} />;
  }

  const groupedLeads: Record<LeadStatus, LeadListItem[]> = {
    New: [],
    Contacted: [],
    Qualified: [],
    Converted: [],
    Lost: [],
  };

  for (const lead of items) {
    if (groupedLeads[lead.status]) {
      groupedLeads[lead.status].push(lead);
    }
  }

  return (
    <div className="flex gap-4 overflow-x-auto pb-4" data-testid="lead-kanban-board">
      {COLUMNS.map((status) => (
        <div
          key={status}
          className="flex w-72 min-w-[18rem] flex-shrink-0 flex-col rounded-xl border border-slate-200 bg-slate-50"
          data-testid={`kanban-column-${status}`}
        >
          <div className="flex items-center gap-2 border-b border-slate-200 px-4 py-3">
            <span className={`h-2 w-2 rounded-full ${columnColors[status]}`} />
            <h3 className="text-sm font-semibold text-slate-900">{t(`status.${status}`)}</h3>
            <span className="ml-auto rounded-full bg-white px-2 py-0.5 text-xs font-medium text-slate-500 shadow-sm">
              {groupedLeads[status].length}
            </span>
          </div>
          <div className="flex flex-1 flex-col gap-3 p-3">
            {groupedLeads[status].length === 0 ? (
              <p className="py-4 text-center text-xs text-slate-400">{t('empty.title')}</p>
            ) : (
              groupedLeads[status].map((lead) => (
                <LeadKanbanCard
                  key={lead.id}
                  lead={lead}
                  onChangeStatus={onChangeStatus}
                  onViewDetails={onViewDetails}
                />
              ))
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
