// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import Link from 'next/link';
import { useTranslations } from 'next-intl';

export function QuickActions() {
  const t = useTranslations('dashboard');

  return (
    <div className="rounded-xl bg-white p-6 shadow-sm" data-testid="quick-actions">
      <h3 className="mb-4 text-base font-semibold text-slate-900">{t('quickActions')}</h3>
      <div className="flex flex-wrap gap-3">
        <Link
          href="/properties/new"
          className="inline-flex items-center gap-2 rounded-lg bg-primary-600 px-4 py-2.5 text-sm font-medium text-white shadow-sm transition-colors hover:bg-primary-600/90 active:scale-[0.98]"
        >
          <span className="material-symbols-outlined text-lg">add_home</span>
          {t('newProperty')}
        </Link>
        <Link
          href="/leads"
          className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50"
        >
          <span className="material-symbols-outlined text-lg">person_add</span>
          {t('newLead')}
        </Link>
        <Link
          href="/appointments"
          className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50"
        >
          <span className="material-symbols-outlined text-lg">calendar_add_on</span>
          {t('scheduleAppointment')}
        </Link>
      </div>
    </div>
  );
}
