// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import type { PropertyListItem, PropertyStatusType } from '@/lib/schemas';
import { PropertyStatusBadge } from './PropertyStatusBadge';
import { SkeletonTable } from '@/components/ui/skeleton';
import { useState, useRef, useEffect } from 'react';

interface PropertyTableProps {
  items: PropertyListItem[];
  isLoading: boolean;
  onDelete: (id: string) => void;
  onChangeStatus: (id: string, status: PropertyStatusType) => void;
}

function formatPrice(price: number | null | undefined): string {
  if (price == null) return '—';
  return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR', maximumFractionDigits: 0 }).format(price);
}

function formatDate(dateStr: string | null | undefined): string {
  if (!dateStr) return '—';
  return new Intl.DateTimeFormat('es-ES', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(dateStr));
}

const validTransitions: Record<PropertyStatusType, PropertyStatusType[]> = {
  Draft: ['Active', 'Archived'],
  Active: ['Reserved', 'Sold', 'Rented', 'Archived'],
  Reserved: ['Active', 'Sold', 'Rented'],
  Sold: ['Archived'],
  Rented: ['Archived'],
  Archived: ['Draft'],
};

function ActionsMenu({ item, onDelete, onChangeStatus }: {
  item: PropertyListItem;
  onDelete: (id: string) => void;
  onChangeStatus: (id: string, status: PropertyStatusType) => void;
}) {
  const t = useTranslations('properties');
  const [open, setOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [open]);

  const transitions = validTransitions[item.status] ?? [];

  return (
    <div className="relative" ref={menuRef}>
      <button
        type="button"
        onClick={() => setOpen(!open)}
        aria-label={t('table.actions')}
        className="rounded-lg p-1.5 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600"
      >
        <span className="material-symbols-outlined text-lg" aria-hidden="true">more_vert</span>
      </button>
      {open && (
        <div
          role="menu"
          className="absolute right-0 z-10 mt-1 w-48 rounded-xl border border-slate-200 bg-white py-1 shadow-lg"
        >
          <Link
            href={`/properties/${item.id}/edit`}
            role="menuitem"
            className="flex items-center gap-2 px-4 py-2 text-sm text-slate-700 hover:bg-slate-50"
            onClick={() => setOpen(false)}
          >
            <span className="material-symbols-outlined text-sm" aria-hidden="true">edit</span>
            {t('actions.edit')}
          </Link>
          {transitions.length > 0 && (
            <div className="border-t border-slate-100">
              {transitions.map((status) => (
                <button
                  key={status}
                  type="button"
                  role="menuitem"
                  onClick={() => { onChangeStatus(item.id, status); setOpen(false); }}
                  className="flex w-full items-center gap-2 px-4 py-2 text-sm text-slate-700 hover:bg-slate-50"
                >
                  <PropertyStatusBadge status={status} label={t(`status.${status}`)} />
                </button>
              ))}
            </div>
          )}
          <div className="border-t border-slate-100">
            <button
              type="button"
              role="menuitem"
              onClick={() => { onDelete(item.id); setOpen(false); }}
              className="flex w-full items-center gap-2 px-4 py-2 text-sm text-red-600 hover:bg-red-50"
            >
              <span className="material-symbols-outlined text-sm" aria-hidden="true">delete</span>
              {t('actions.delete')}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export function PropertyTable({ items, isLoading, onDelete, onChangeStatus }: PropertyTableProps) {
  const t = useTranslations('properties');

  if (isLoading) {
    return <SkeletonTable rows={5} columns={7} />;
  }

  return (
    <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-slate-100 bg-slate-50/50">
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.property')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.type')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.operation')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.price')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.status')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.updated')}</th>
            <th className="px-4 py-3 font-medium text-slate-500"><span className="sr-only">{t('table.actions')}</span></th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {items.map((item) => (
            <tr key={item.id} className="transition hover:bg-slate-50/50">
              <td className="px-4 py-3">
                <Link
                  href={`/properties/${item.id}`}
                  className="group flex items-center gap-3"
                >
                  <div className="h-12 w-16 flex-shrink-0 rounded-lg bg-slate-100 flex items-center justify-center">
                    <span className="material-symbols-outlined text-xl text-slate-300" aria-hidden="true">apartment</span>
                  </div>
                  <div className="min-w-0">
                    <p className="truncate font-medium text-slate-900 group-hover:text-primary-600">{item.title}</p>
                    {item.city && <p className="text-xs text-slate-500">{item.city}</p>}
                  </div>
                </Link>
              </td>
              <td className="px-4 py-3 text-slate-600">{t(`type.${item.propertyType}`)}</td>
              <td className="px-4 py-3 text-slate-600">{t(`operation.${item.operationType}`)}</td>
              <td className="px-4 py-3 font-medium text-slate-900">{formatPrice(item.price)}</td>
              <td className="px-4 py-3">
                <PropertyStatusBadge status={item.status} label={t(`status.${item.status}`)} />
              </td>
              <td className="px-4 py-3 text-slate-500">{formatDate(item.updatedAtUtc ?? item.createdAtUtc)}</td>
              <td className="px-4 py-3">
                <ActionsMenu
                  item={item}
                  onDelete={onDelete}
                  onChangeStatus={onChangeStatus}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
