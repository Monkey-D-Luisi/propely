// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useActiveOrg } from '@/hooks/use-active-org';
import { useWorkItems, useDeleteWorkItem } from '@/hooks/work-items';
import { useRequireAuth } from '@/hooks/useRequireAuth';
import { WorkItemsTable } from '@/components/work-items/WorkItemsTable';
import { Pagination } from '@/components/ui/pagination';
import { DialogOverlay } from '@/components/ui/dialog-overlay';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';
import type { WorkItemStatusType } from '@/lib/schemas';

const statusOptions: (WorkItemStatusType | '')[] = ['', 'Pending', 'Active', 'Deleted', 'Deactivated', 'Expired'];

export default function WorkItemsPage() {
  const t = useTranslations('workItems');
  const tCommon = useTranslations('common');
  const { isLoading: userLoading } = useRequireAuth();
  const { orgs, activeOrgId, hasOrgs, isLoading: orgsLoading, selectOrg } = useActiveOrg();
  const { toast } = useToast();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');

  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [isDeleting, setDeleting] = useState(false);
  const dialogRef = useRef<HTMLDivElement>(null);

  const deleteWorkItem = useDeleteWorkItem();

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [search]);

  // Reset page on status change
  useEffect(() => {
    setPage(1);
  }, [statusFilter]);

  const { items, pagination, isLoading, error, refetch } = useWorkItems(
    page,
    10,
    statusFilter || undefined,
    debouncedSearch || undefined,
  );

  const handleDelete = async () => {
    if (!deleteId) return;
    setDeleting(true);
    try {
      await deleteWorkItem(deleteId);
      toast({ title: t('delete.confirm'), variant: 'success' });
      setDeleteId(null);
      void refetch();
    } catch {
      toast({ title: t('loadError'), variant: 'destructive' });
    } finally {
      setDeleting(false);
    }
  };

  if (userLoading || orgsLoading) {
    return (
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
        <div className="animate-pulse space-y-4">
          <div className="h-8 w-48 rounded-lg bg-slate-200" />
          <div className="h-4 w-72 rounded-lg bg-slate-100" />
        </div>
      </div>
    );
  }

  if (!hasOrgs) {
    return (
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-8 text-center shadow-sm">
          <span className="material-symbols-outlined text-4xl text-slate-300" aria-hidden="true">
            domain_add
          </span>
          <p className="mt-3 text-sm text-slate-600">{t('noOrg')}</p>
          <Link
            href="/orgs/mine"
            className="mt-4 inline-flex items-center gap-1.5 rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98]"
          >
            {t('createOrg')}
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
      {/* Header */}
      <div className="flex flex-col gap-4 border-b border-slate-100 pb-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <Link
          href="/work-items/new"
          className="inline-flex items-center gap-1.5 rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
        >
          <span className="material-symbols-outlined text-sm" aria-hidden="true">add</span>
          {t('newWorkItem')}
        </Link>
      </div>

      {/* Org selector (only when multiple orgs) */}
      {orgs.length > 1 && (
        <div className="flex items-center gap-2">
          <label htmlFor="org-selector" className="text-sm font-medium text-slate-700">
            {t('orgSelector')}
          </label>
          <select
            id="org-selector"
            value={activeOrgId ?? ''}
            onChange={(e) => {
              selectOrg(e.target.value);
              setPage(1);
              void refetch();
            }}
            className="rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-sm text-slate-700 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          >
            {orgs.map((org) => (
              <option key={org.id} value={org.id}>
                {org.name}
              </option>
            ))}
          </select>
        </div>
      )}

      {/* Filters */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <div className="relative flex-1">
          <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm text-slate-400" aria-hidden="true">
            search
          </span>
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={t('searchPlaceholder')}
            className="w-full rounded-lg border border-slate-200 py-2 pl-9 pr-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
        >
          {statusOptions.map((s) => (
            <option key={s} value={s}>
              {s ? t(`status.${s}`) : t('allStatuses')}
            </option>
          ))}
        </select>
      </div>

      {/* Content */}
      {error ? (
        <ErrorMessage message={t('loadError')} />
      ) : (
        <>
          <WorkItemsTable items={items} isLoading={isLoading} onDelete={setDeleteId} />
          <Pagination pagination={pagination} onPageChange={setPage} />
        </>
      )}

      {/* Delete confirmation dialog */}
      {deleteId && (
        <DialogOverlay
          dialogRef={dialogRef}
          isProcessing={isDeleting}
          onClose={() => setDeleteId(null)}
        >
          <div
            ref={dialogRef}
            role="alertdialog"
            aria-modal="true"
            aria-labelledby="delete-dialog-title"
            aria-describedby="delete-dialog-desc"
            className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-6 shadow-lg"
          >
            <h2 id="delete-dialog-title" className="text-lg font-semibold text-slate-900">
              {t('delete.dialogTitle')}
            </h2>
            <p id="delete-dialog-desc" className="mt-2 text-sm text-slate-500">
              {t('delete.dialogMessage')}
            </p>
            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setDeleteId(null)}
                disabled={isDeleting}
                className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50"
              >
                {t('delete.cancel')}
              </button>
              <button
                type="button"
                onClick={() => void handleDelete()}
                disabled={isDeleting}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-red-700 active:scale-[0.98] disabled:opacity-50"
              >
                {isDeleting ? tCommon('loading') : t('delete.confirm')}
              </button>
            </div>
          </div>
        </DialogOverlay>
      )}
    </div>
  );
}
