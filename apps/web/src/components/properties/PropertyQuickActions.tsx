// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useRef, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import type { PropertyListItem, PropertyStatusType } from '@/lib/schemas';
import { PropertyStatusBadge } from './PropertyStatusBadge';

const validTransitions: Record<PropertyStatusType, PropertyStatusType[]> = {
  Draft: ['Active', 'Archived'],
  Active: ['Reserved', 'Sold', 'Rented', 'Archived'],
  Reserved: ['Active', 'Sold', 'Rented', 'Archived'],
  Sold: [],
  Rented: [],
  Archived: ['Draft'],
};

interface PropertyQuickActionsProps {
  item: PropertyListItem;
  onDelete: (id: string) => void;
  onChangeStatus: (id: string, status: PropertyStatusType) => void;
}

export function PropertyQuickActions({ item, onDelete, onChangeStatus }: PropertyQuickActionsProps) {
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
