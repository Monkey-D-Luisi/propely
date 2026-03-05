// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import type { AgencyBranch } from '@/lib/schemas';

interface BranchCardProps {
  agencyId: string;
  branch: AgencyBranch;
}

export function BranchCard({ branch }: BranchCardProps) {
  const t = useTranslations('agencies');

  const formattedDate = new Date(branch.createdAtUtc).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });

  return (
    <Link
      href={`/orgs/${branch.id}/members`}
      className="group flex flex-col rounded-xl border border-slate-200 bg-white p-6 shadow-sm transition-shadow hover:shadow-md"
    >
      <div className="mb-4 flex items-start justify-between">
        <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary-600/10 text-primary-600">
          <span className="material-symbols-outlined" aria-hidden="true">store</span>
        </div>
        <span className="inline-flex items-center rounded-full bg-slate-100 px-2.5 py-0.5 text-xs font-medium text-slate-600">
          {t('dashboard.memberCount', { count: branch.memberCount })}
        </span>
      </div>
      <h3 className="mb-1 text-lg font-bold text-slate-900">{branch.name}</h3>
      <p className="text-sm text-slate-500">{t('dashboard.createdOn', { date: formattedDate })}</p>
      <div className="mt-auto flex items-center justify-end border-t border-slate-50 pt-4">
        <span className="flex items-center text-sm font-semibold text-primary-600 transition-transform group-hover:translate-x-1">
          {t('dashboard.viewBranch')} <span className="material-symbols-outlined ml-1 text-xs" aria-hidden="true">chevron_right</span>
        </span>
      </div>
    </Link>
  );
}
