// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License.

'use client';

import { useTranslations, useLocale } from 'next-intl';
import { Link } from '@/i18n/navigation';
import type { ContactListItem } from '@/hooks/useContacts';
import { ContactRoleBadge } from './ContactRoleBadge';
import { SkeletonTable } from '@/components/ui/skeleton';

interface ContactsTableProps {
  items: ContactListItem[];
  isLoading: boolean;
  onDelete: (id: string) => void;
}

function formatDate(dateStr: string | null | undefined, locale: string): string {
  if (!dateStr) return '\u2014';
  return new Intl.DateTimeFormat(locale, { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(dateStr));
}

export function ContactsTable({ items, isLoading, onDelete }: ContactsTableProps) {
  const t = useTranslations('contacts');
  const locale = useLocale();

  if (isLoading) {
    return <SkeletonTable rows={5} columns={6} />;
  }

  return (
    <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-slate-100 bg-slate-50/50">
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.name')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.email')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.phone')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.roles')}</th>
            <th className="px-4 py-3 font-medium text-slate-500">{t('table.created')}</th>
            <th className="px-4 py-3 font-medium text-slate-500"><span className="sr-only">{t('table.actions')}</span></th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {items.map((item) => (
            <tr key={item.id} className="transition hover:bg-slate-50/50">
              <td className="px-4 py-3">
                <Link
                  href={`/contacts/${item.id}`}
                  className="font-medium text-slate-900 hover:text-primary-600"
                >
                  {item.firstName} {item.lastName}
                </Link>
                {item.company && <p className="text-xs text-slate-500">{item.company}</p>}
              </td>
              <td className="px-4 py-3 text-slate-600">{item.email}</td>
              <td className="px-4 py-3 text-slate-600">{item.phone ?? '\u2014'}</td>
              <td className="px-4 py-3">
                <div className="flex flex-wrap gap-1">
                  {item.roles.map((role) => (
                    <ContactRoleBadge key={role} role={role} label={t(`role.${role}`)} />
                  ))}
                </div>
              </td>
              <td className="px-4 py-3 text-slate-500">{formatDate(item.createdAtUtc, locale)}</td>
              <td className="px-4 py-3">
                <button
                  type="button"
                  onClick={() => onDelete(item.id)}
                  aria-label={t('delete.confirm')}
                  className="rounded-lg p-1.5 text-slate-400 transition hover:bg-red-50 hover:text-red-600"
                >
                  <span className="material-symbols-outlined text-lg" aria-hidden="true">delete</span>
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
