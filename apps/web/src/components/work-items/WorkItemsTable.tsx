// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import type { WorkItem } from '@/lib/schemas';
import { formatDateShort } from '@/lib/format';
import { StatusBadge } from './StatusBadge';
import { PriorityBadge, TypeBadge } from './FieldBadges';

interface WorkItemsTableProps {
  items: WorkItem[];
  isLoading: boolean;
  onDelete: (id: string) => void;
}

function LoadingSkeleton() {
  return (
    <>
      {Array.from({ length: 5 }).map((_, i) => (
        <tr key={i} className="animate-pulse">
          <td className="px-4 py-3"><div className="h-4 w-48 rounded bg-slate-100" /></td>
          <td className="px-4 py-3"><div className="h-5 w-20 rounded-full bg-slate-100" /></td>
          <td className="hidden px-4 py-3 sm:table-cell"><div className="h-5 w-16 rounded-full bg-slate-100" /></td>
          <td className="hidden px-4 py-3 sm:table-cell"><div className="h-5 w-16 rounded-full bg-slate-100" /></td>
          <td className="px-4 py-3"><div className="h-4 w-24 rounded bg-slate-100" /></td>
          <td className="px-4 py-3"><div className="h-4 w-24 rounded bg-slate-100" /></td>
          <td className="px-4 py-3"><div className="h-4 w-28 rounded bg-slate-100" /></td>
        </tr>
      ))}
    </>
  );
}

export function WorkItemsTable({ items, isLoading, onDelete }: WorkItemsTableProps) {
  const t = useTranslations('workItems');

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-slate-200 bg-slate-50/50">
            <th className="px-4 py-3 font-medium text-slate-600">{t('columns.title')}</th>
            <th className="px-4 py-3 font-medium text-slate-600">{t('columns.status')}</th>
            <th className="hidden px-4 py-3 font-medium text-slate-600 sm:table-cell">{t('columns.priority')}</th>
            <th className="hidden px-4 py-3 font-medium text-slate-600 sm:table-cell">{t('columns.type')}</th>
            <th className="hidden px-4 py-3 font-medium text-slate-600 md:table-cell">{t('columns.created')}</th>
            <th className="hidden px-4 py-3 font-medium text-slate-600 lg:table-cell">{t('columns.updated')}</th>
            <th className="px-4 py-3 font-medium text-slate-600">{t('columns.actions')}</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {isLoading ? (
            <LoadingSkeleton />
          ) : items.length === 0 ? (
            <tr>
              <td colSpan={7} className="px-4 py-12 text-center text-slate-500">
                {t('emptyState')}
              </td>
            </tr>
          ) : (
            items.map((item) => (
              <tr key={item.id} className="transition-colors hover:bg-slate-50/50">
                <td className="px-4 py-3">
                  <Link
                    href={`/work-items/${item.id}`}
                    className="font-medium text-slate-900 hover:text-primary-600"
                  >
                    {item.title}
                  </Link>
                </td>
                <td className="px-4 py-3">
                  <StatusBadge status={item.status} />
                </td>
                <td className="hidden px-4 py-3 sm:table-cell">
                  {item.priority && <PriorityBadge priority={item.priority} />}
                </td>
                <td className="hidden px-4 py-3 sm:table-cell">
                  {item.type && <TypeBadge type={item.type} />}
                </td>
                <td className="hidden px-4 py-3 text-slate-500 md:table-cell">
                  {formatDateShort(item.createdAtUtc)}
                </td>
                <td className="hidden px-4 py-3 text-slate-500 lg:table-cell">
                  {formatDateShort(item.updatedAtUtc)}
                </td>
                <td className="px-4 py-3">
                  <div className="flex items-center gap-3">
                    <Link
                      href={`/work-items/${item.id}`}
                      className="text-sm text-primary-600 hover:text-primary-700"
                    >
                      {t('actions.view')}
                    </Link>
                    <Link
                      href={`/work-items/${item.id}/edit`}
                      className="text-sm text-primary-600 hover:text-primary-700"
                    >
                      {t('actions.edit')}
                    </Link>
                    <button
                      type="button"
                      onClick={() => onDelete(item.id)}
                      className="text-sm text-red-600 hover:text-red-700"
                    >
                      {t('actions.delete')}
                    </button>
                  </div>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
