// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import { useActiveOrg } from '@/hooks/use-active-org';
import { useLeads, type LeadFilters, type LeadStatus } from '@/hooks/useLeads';
import { useChangeLeadStatus } from '@/hooks/useChangeLeadStatus';
import { useConvertLead } from '@/hooks/useConvertLead';
import { useRequireAuth } from '@/hooks/useRequireAuth';
import { LeadKanbanBoard } from '@/components/leads/LeadKanbanBoard';
import { LeadListView } from '@/components/leads/LeadListView';
import { LeadDetailPanel } from '@/components/leads/LeadDetailPanel';
import { LeadConversionModal } from '@/components/leads/LeadConversionModal';
import { LeadFilters as LeadFiltersBar } from '@/components/leads/LeadFilters';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';
import type { ContactRole } from '@/hooks/useContacts';

type ViewMode = 'kanban' | 'list';

export default function LeadsPage() {
  const t = useTranslations('leads');
  const tCommon = useTranslations('common');
  const { toast } = useToast();
  const { isLoading: userLoading } = useRequireAuth();
  const { hasOrgs, isLoading: orgsLoading } = useActiveOrg();

  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<LeadStatus | ''>('');
  const [viewMode, setViewMode] = useState<ViewMode>(() => {
    if (typeof window !== 'undefined') {
      return (localStorage.getItem('propely_leads_view') as ViewMode) || 'kanban';
    }
    return 'kanban';
  });
  const [selectedLeadId, setSelectedLeadId] = useState<string | null>(null);
  const [convertLeadId, setConvertLeadId] = useState<string | null>(null);

  const changeLeadStatus = useChangeLeadStatus();
  const convertLead = useConvertLead();

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
    }, 300);
    return () => clearTimeout(timer);
  }, [search]);

  const filters: LeadFilters = {
    search: debouncedSearch || undefined,
    status: statusFilter || undefined,
  };

  const { items, isLoading, error, refetch } = useLeads(1, 100, filters);

  const handleViewChange = useCallback((mode: ViewMode) => {
    setViewMode(mode);
    localStorage.setItem('propely_leads_view', mode);
  }, []);

  const handleChangeStatus = useCallback(async (id: string, status: LeadStatus) => {
    try {
      await changeLeadStatus(id, status);
      toast({ title: t('statusChange.success'), variant: 'success' });
      void refetch();
    } catch {
      toast({ title: t('statusChange.error'), variant: 'destructive' });
    }
  }, [changeLeadStatus, refetch, toast, t]);

  const handleConvert = useCallback(async (id: string, role: ContactRole, notes?: string) => {
    try {
      await convertLead(id, role, notes);
      toast({ title: t('conversion.success'), variant: 'success' });
      setConvertLeadId(null);
      setSelectedLeadId(null);
      void refetch();
    } catch {
      toast({ title: t('conversion.error'), variant: 'destructive' });
    }
  }, [convertLead, refetch, toast, t]);

  if (userLoading || orgsLoading) {
    return (
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6 p-6 md:p-10">
        <div className="animate-pulse space-y-4">
          <div className="h-8 w-48 rounded-lg bg-slate-200" />
          <div className="h-4 w-72 rounded-lg bg-slate-100" />
        </div>
      </div>
    );
  }

  if (!hasOrgs) {
    return (
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6 p-6 md:p-10">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-8 text-center shadow-sm">
          <span className="material-symbols-outlined text-4xl text-slate-300" aria-hidden="true">conversion_path</span>
          <p className="mt-3 text-sm text-slate-600">{t('noOrg')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto flex w-full max-w-7xl flex-col gap-6 p-6 md:p-10">
      {/* Header */}
      <div className="flex flex-col gap-4 border-b border-slate-100 pb-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => handleViewChange('kanban')}
            className={`inline-flex items-center gap-1.5 rounded-lg px-3 py-2 text-sm font-medium transition ${
              viewMode === 'kanban'
                ? 'bg-primary-600 text-white shadow-sm'
                : 'border border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
            }`}
            aria-pressed={viewMode === 'kanban'}
          >
            <span className="material-symbols-outlined text-sm" aria-hidden="true">view_kanban</span>
            {t('view.kanban')}
          </button>
          <button
            type="button"
            onClick={() => handleViewChange('list')}
            className={`inline-flex items-center gap-1.5 rounded-lg px-3 py-2 text-sm font-medium transition ${
              viewMode === 'list'
                ? 'bg-primary-600 text-white shadow-sm'
                : 'border border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
            }`}
            aria-pressed={viewMode === 'list'}
          >
            <span className="material-symbols-outlined text-sm" aria-hidden="true">view_list</span>
            {t('view.list')}
          </button>
        </div>
      </div>

      {/* Filters */}
      <LeadFiltersBar
        search={search}
        onSearchChange={(val) => setSearch(val)}
        status={statusFilter}
        onStatusChange={(val) => setStatusFilter(val)}
      />

      {/* Content */}
      {error ? (
        <ErrorMessage message={t('loadError')} />
      ) : items.length === 0 && !isLoading ? (
        <div className="rounded-xl border border-slate-200 bg-white p-12 text-center shadow-sm">
          <span className="material-symbols-outlined text-5xl text-slate-300" aria-hidden="true">conversion_path</span>
          <h2 className="mt-4 text-lg font-semibold text-slate-900">{t('empty.title')}</h2>
          <p className="mt-1 text-sm text-slate-500">{t('empty.subtitle')}</p>
        </div>
      ) : viewMode === 'kanban' ? (
        <LeadKanbanBoard
          items={items}
          isLoading={isLoading}
          onChangeStatus={(id, status) => void handleChangeStatus(id, status)}
          onViewDetails={setSelectedLeadId}
        />
      ) : (
        <LeadListView
          items={items}
          isLoading={isLoading}
          onViewDetails={setSelectedLeadId}
          onChangeStatus={(id, status) => void handleChangeStatus(id, status)}
          onConvert={setConvertLeadId}
        />
      )}

      {/* Detail Panel */}
      {selectedLeadId && (
        <LeadDetailPanel
          leadId={selectedLeadId}
          onClose={() => setSelectedLeadId(null)}
          onConvert={(id) => {
            setConvertLeadId(id);
          }}
        />
      )}

      {/* Conversion Modal */}
      {convertLeadId && (
        <LeadConversionModal
          leadId={convertLeadId}
          onConvert={handleConvert}
          onClose={() => setConvertLeadId(null)}
        />
      )}
    </div>
  );
}
