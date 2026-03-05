// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { Skeleton } from '@/components/ui/skeleton';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useAgency } from '@/hooks/agencies';
import { useCurrentUser } from '@/hooks/orgs';
import { BranchCard } from './BranchCard';

interface AgencyDashboardProps {
  agencyId: string;
}

export function AgencyDashboard({ agencyId }: AgencyDashboardProps) {
  const t = useTranslations('agencies');
  const tCommon = useTranslations('common');
  const { agency, isLoading, error } = useAgency(agencyId);
  const { user } = useCurrentUser();

  if (isLoading) {
    return (
      <div className="mx-auto w-full max-w-4xl px-4 py-12">
        <div className="mb-10 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <Skeleton className="h-9 w-48" />
            <Skeleton className="mt-1 h-4 w-64" />
          </div>
          <Skeleton className="h-10 w-32 rounded-lg" />
        </div>
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <div
              key={i}
              className="flex flex-col rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
            >
              <div className="mb-4 flex items-start justify-between">
                <Skeleton className="h-12 w-12 rounded-lg" />
                <Skeleton className="h-6 w-16 rounded-full" />
              </div>
              <Skeleton className="h-5 w-32" />
              <Skeleton className="mt-2 h-4 w-full" />
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error || !agency) {
    return (
      <div className="mx-auto w-full max-w-4xl px-4 py-12">
        <ErrorMessage message={t('dashboard.loadError')} />
      </div>
    );
  }

  const isOwner = user?.id === agency.createdByUserId;

  return (
    <div className="mx-auto w-full max-w-4xl px-4 py-12">
      {/* Section Header */}
      <div className="mb-10 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{agency.name}</h1>
          <p className="mt-1 text-slate-500">
            {t('dashboard.subtitle', { slug: agency.slug })}
          </p>
        </div>
        <div className="flex gap-3">
          {isOwner && (
            <Link
              href={`/agencies/${agencyId}/settings`}
              className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-none transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
            >
              <span className="material-symbols-outlined text-sm" aria-hidden="true">settings</span>
              {t('dashboard.settings')}
            </Link>
          )}
        </div>
      </div>

      {/* Branch List */}
      <div className="mb-6 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-slate-900">
          {t('dashboard.branchesTitle')}
        </h2>
        <span className="text-sm text-slate-500">
          {t('dashboard.branchCount', { count: agency.branches.length })}
        </span>
      </div>

      {agency.branches.length === 0 ? (
        <div className="flex min-h-[220px] flex-col items-center justify-center rounded-xl border-2 border-dashed border-slate-300 bg-transparent p-6">
          <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 text-slate-400">
            <span className="material-symbols-outlined text-2xl" aria-hidden="true">store</span>
          </div>
          <span className="text-sm font-semibold text-slate-600">{t('dashboard.emptyBranches')}</span>
          <p className="mt-1 text-sm text-slate-400">{t('dashboard.emptyBranchesHint')}</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
          {agency.branches.map((branch) => (
            <BranchCard key={branch.id} branch={branch} />
          ))}
        </div>
      )}

      <div className="mt-8">
        <Link href="/orgs/mine" className="text-sm text-slate-500 hover:text-slate-700">
          ← {tCommon('backToOrgs')}
        </Link>
      </div>
    </div>
  );
}
