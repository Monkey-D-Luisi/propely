// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { Link } from '@/i18n/navigation';
import { useWorkItem, useDeleteWorkItem } from '@/hooks/work-items';
import { useActiveOrg } from '@/hooks/use-active-org';
import { formatDateLong } from '@/lib/format';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { StatusBadge } from './StatusBadge';
import { PriorityBadge, TypeBadge, EffortBadge } from './FieldBadges';
import { DialogOverlay } from '@/components/ui/dialog-overlay';
import { useToast } from '@/components/ui/toast';

interface WorkItemDetailProps {
  id: string;
}

export function WorkItemDetail({ id }: WorkItemDetailProps) {
  const t = useTranslations('workItems');
  const tCommon = useTranslations('common');
  const router = useRouter();
  useActiveOrg();
  const { toast } = useToast();
  const { item, isLoading, error } = useWorkItem(id);
  const deleteWorkItem = useDeleteWorkItem();

  const [showDelete, setShowDelete] = useState(false);
  const [isDeleting, setDeleting] = useState(false);
  const dialogRef = useRef<HTMLDivElement>(null);

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await deleteWorkItem(id);
      toast({ title: t('delete.confirm'), variant: 'success' });
      router.push('/work-items');
    } catch {
      toast({ title: t('loadError'), variant: 'destructive' });
    } finally {
      setDeleting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-6">
        <div className="h-8 w-64 rounded-lg bg-slate-200" />
        <div className="h-4 w-40 rounded-lg bg-slate-100" />
        <div className="h-32 rounded-xl bg-slate-100" />
      </div>
    );
  }

  if (error || !item) {
    return <ErrorMessage message={t('loadError')} />;
  }

  return (
    <>
      {/* Breadcrumbs */}
      <div className="flex items-center gap-2 text-sm text-slate-500">
        <Link href="/work-items" className="hover:text-slate-700">
          {t('breadcrumbs.list')}
        </Link>
        <span aria-hidden="true">/</span>
        <span className="text-slate-900">{item.title}</span>
      </div>

      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="flex items-center gap-3">
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{item.title}</h1>
          <StatusBadge status={item.status} />
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => setShowDelete(true)}
            className="rounded-lg border border-red-200 bg-white px-4 py-2 text-sm font-medium text-red-600 transition hover:bg-red-50"
          >
            {t('actions.delete')}
          </button>
          <Link
            href={`/work-items/${id}/edit`}
            className="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98]"
          >
            {t('actions.edit')}
          </Link>
        </div>
      </div>

      {/* Content */}
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        {/* Description */}
        <div className="mb-6">
          <h2 className="mb-2 text-sm font-medium text-slate-600">{t('detail.description')}</h2>
          <p className="whitespace-pre-wrap text-sm text-slate-900">
            {item.description || t('detail.noDescription')}
          </p>
        </div>

        {/* Properties */}
        {(item.priority || item.type || item.dueDateUtc || item.estimatedEffort) && (
          <div className="mb-6 grid grid-cols-2 gap-4 border-t border-slate-100 pt-6 sm:grid-cols-4">
            {item.priority && (
              <div>
                <p className="text-xs font-medium uppercase tracking-wider text-slate-400">
                  {t('detail.priority')}
                </p>
                <div className="mt-1">
                  <PriorityBadge priority={item.priority} />
                </div>
              </div>
            )}
            {item.type && (
              <div>
                <p className="text-xs font-medium uppercase tracking-wider text-slate-400">
                  {t('detail.type')}
                </p>
                <div className="mt-1">
                  <TypeBadge type={item.type} />
                </div>
              </div>
            )}
            {item.dueDateUtc && (
              <div>
                <p className="text-xs font-medium uppercase tracking-wider text-slate-400">
                  {t('detail.dueDate')}
                </p>
                <p className="mt-1 text-sm text-slate-900">{formatDateLong(item.dueDateUtc)}</p>
              </div>
            )}
            {item.estimatedEffort && (
              <div>
                <p className="text-xs font-medium uppercase tracking-wider text-slate-400">
                  {t('detail.effort')}
                </p>
                <div className="mt-1">
                  <EffortBadge effort={item.estimatedEffort} />
                </div>
              </div>
            )}
          </div>
        )}

        {/* Metadata */}
        <div className="grid grid-cols-1 gap-4 border-t border-slate-100 pt-6 sm:grid-cols-3">
          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-slate-400">
              {t('detail.created')}
            </p>
            <p className="mt-1 text-sm text-slate-900">{formatDateLong(item.createdAtUtc)}</p>
          </div>
          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-slate-400">
              {t('detail.updated')}
            </p>
            <p className="mt-1 text-sm text-slate-900">{formatDateLong(item.updatedAtUtc)}</p>
          </div>
          <div>
            <p className="text-xs font-medium uppercase tracking-wider text-slate-400">
              {t('detail.id')}
            </p>
            <p className="mt-1 font-mono text-sm text-slate-500">{item.id}</p>
          </div>
        </div>
      </div>

      {/* Back link */}
      <Link
        href="/work-items"
        className="inline-flex items-center gap-1.5 text-sm text-slate-500 transition hover:text-slate-700"
      >
        <span className="material-symbols-outlined text-sm" aria-hidden="true">arrow_back</span>
        {t('actions.backToList')}
      </Link>

      {/* Delete dialog */}
      {showDelete && (
        <DialogOverlay
          dialogRef={dialogRef}
          isProcessing={isDeleting}
          onClose={() => setShowDelete(false)}
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
                onClick={() => setShowDelete(false)}
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
    </>
  );
}
