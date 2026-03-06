// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useLocale, useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import type { AgencyBranch } from '@/lib/schemas';

interface BranchCardProps {
  branch: AgencyBranch;
}

export function BranchCard({ branch }: BranchCardProps) {
  const t = useTranslations('agencies');
  const locale = useLocale();

  const formattedDate = new Date(branch.createdAtUtc).toLocaleDateString(locale, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });

  return (
    <Link
      href={`/orgs/${branch.id}/members`}
      className="group flex flex-col rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition-colors hover:border-primary-600/50"
    >
      <div className="mb-3 flex items-start justify-between">
        <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-600/10 text-primary-600">
          <span className="material-symbols-outlined text-[22px]" aria-hidden="true">store</span>
        </div>
        <span className="inline-flex items-center rounded-full bg-slate-100 px-2.5 py-0.5 text-xs font-medium text-slate-600">
          {t('dashboard.memberCount', { count: branch.memberCount })}
        </span>
      </div>
      <h3 className="text-sm font-semibold text-slate-900">{branch.name}</h3>
      <p className="text-xs text-slate-500 mt-1">{t('dashboard.createdOn', { date: formattedDate })}</p>
      <div className="mt-auto flex items-center justify-end pt-3">
        <span className="flex items-center text-sm font-semibold text-primary-600 transition-transform group-hover:translate-x-1">
          {t('dashboard.viewBranch')}
          <span className="material-symbols-outlined ml-1 text-[16px]" aria-hidden="true">chevron_right</span>
        </span>
      </div>
    </Link>
  );
}
