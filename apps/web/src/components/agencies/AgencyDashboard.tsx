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
      <main className="flex-1 bg-surface p-8">
        <div className="max-w-6xl mx-auto space-y-6">
          <Skeleton className="h-9 w-48" />
          <div className="bg-white rounded-xl p-6 shadow-sm border border-slate-200 flex items-center gap-6">
            <Skeleton className="size-16 rounded-lg" />
            <div className="flex-1">
              <Skeleton className="h-7 w-48" />
              <Skeleton className="mt-2 h-4 w-32" />
            </div>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="bg-white rounded-xl p-5 border border-slate-200 shadow-sm">
                <Skeleton className="h-4 w-20 mb-3" />
                <Skeleton className="h-8 w-16" />
              </div>
            ))}
          </div>
        </div>
      </main>
    );
  }

  if (error || !agency) {
    return (
      <main className="flex-1 bg-surface p-8">
        <div className="max-w-6xl mx-auto">
          <ErrorMessage message={t('dashboard.loadError')} />
        </div>
      </main>
    );
  }

  const isOwner = user?.id === agency.createdByUserId;

  return (
    <main className="flex-1 bg-surface p-8">
      <div className="max-w-6xl mx-auto space-y-6">
        {/* Breadcrumb */}
        <nav className="flex flex-wrap gap-2 text-sm" aria-label="Breadcrumb">
          <Link href="/orgs/mine" className="text-slate-500 hover:text-primary-600 transition-colors">
            {tCommon('backToOrgs')}
          </Link>
          <span className="text-slate-400">/</span>
          <span className="text-slate-900 font-medium">{agency.name}</span>
        </nav>

        {/* Agency Header Card */}
        <div className="bg-white rounded-xl p-6 shadow-sm border border-slate-200 flex flex-col md:flex-row items-start md:items-center justify-between gap-6">
          <div className="flex items-center gap-4">
            <div className="size-16 rounded-lg bg-primary-600/10 flex items-center justify-center text-primary-600 shrink-0">
              <span className="material-symbols-outlined text-3xl" aria-hidden="true">apartment</span>
            </div>
            <div>
              <div className="flex items-center gap-3">
                <h1 className="text-2xl font-bold text-slate-900 tracking-tight">{agency.name}</h1>
                <span className="px-2.5 py-0.5 rounded-full bg-emerald-100 text-emerald-700 text-xs font-semibold uppercase tracking-wide flex items-center gap-1">
                  <span className="size-1.5 rounded-full bg-emerald-500" />
                  {t('dashboard.active')}
                </span>
              </div>
              <p className="text-slate-500 flex items-center gap-1.5 text-sm mt-1">
                {t('dashboard.subtitle', { slug: agency.slug })}
              </p>
            </div>
          </div>
          <div className="flex gap-3">
            {isOwner && (
              <Link
                href={`/agencies/${agencyId}/settings`}
                className="px-4 py-2 rounded-lg bg-white border border-slate-200 text-slate-700 text-sm font-semibold hover:bg-slate-50 transition-colors flex items-center gap-2"
              >
                <span className="material-symbols-outlined text-[18px]" aria-hidden="true">edit</span>
                {t('dashboard.settings')}
              </Link>
            )}
          </div>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard
            label={t('dashboard.statBranches')}
            value={agency.branches.length}
            icon="home"
            iconColor="text-primary-600 bg-primary-600/10"
          />
          <StatCard
            label={t('dashboard.statAgents')}
            value={agency.branches.reduce((sum, b) => sum + b.memberCount, 0)}
            icon="group"
            iconColor="text-purple-600 bg-purple-100"
          />
          <StatCard
            label={t('dashboard.statLeads')}
            value={0}
            icon="forum"
            iconColor="text-amber-600 bg-amber-100"
          />
          <StatCard
            label={t('dashboard.statRevenue')}
            value="—"
            icon="payments"
            iconColor="text-emerald-600 bg-emerald-100"
          />
        </div>

        {/* Content Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Branches (left 2/3) */}
          <div className="lg:col-span-2">
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
              <div className="px-6 py-4 border-b border-slate-200 flex justify-between items-center bg-slate-50/80">
                <h2 className="font-semibold text-slate-900 flex items-center gap-2">
                  <span className="material-symbols-outlined text-slate-400" aria-hidden="true">store</span>
                  {t('dashboard.branchesTitle')}
                </h2>
                <span className="text-sm text-slate-500">
                  {t('dashboard.branchCount', { count: agency.branches.length })}
                </span>
              </div>

              {agency.branches.length === 0 ? (
                <div className="flex min-h-[220px] flex-col items-center justify-center p-6">
                  <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 text-slate-400">
                    <span className="material-symbols-outlined text-2xl" aria-hidden="true">store</span>
                  </div>
                  <span className="text-sm font-semibold text-slate-600">{t('dashboard.emptyBranches')}</span>
                  <p className="mt-1 text-sm text-slate-400">{t('dashboard.emptyBranchesHint')}</p>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-4">
                  {agency.branches.map((branch) => (
                    <BranchCard key={branch.id} branch={branch} />
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Agency Info (right 1/3) */}
          <div className="space-y-6">
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
              <div className="px-5 py-4 border-b border-slate-200 bg-slate-50/80">
                <h3 className="font-semibold text-slate-900 flex items-center gap-2">
                  <span className="material-symbols-outlined text-slate-400" aria-hidden="true">info</span>
                  {t('dashboard.infoTitle')}
                </h3>
              </div>
              <div className="p-5 space-y-4">
                <div>
                  <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-1">{t('dashboard.infoSlug')}</p>
                  <p className="text-sm text-slate-900 font-medium">{agency.slug}</p>
                </div>
                <div>
                  <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-1">{t('dashboard.infoCreated')}</p>
                  <p className="text-sm text-slate-900 font-medium">
                    {new Date(agency.createdAtUtc).toLocaleDateString()}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Back link */}
        <div>
          <Link href="/orgs/mine" className="text-sm text-slate-500 hover:text-slate-700">
            ← {tCommon('backToOrgs')}
          </Link>
        </div>
      </div>
    </main>
  );
}

function StatCard({
  label,
  value,
  icon,
  iconColor,
}: {
  label: string;
  value: number | string;
  icon: string;
  iconColor: string;
}) {
  return (
    <div className="bg-white rounded-xl p-5 border border-slate-200 shadow-sm flex flex-col gap-3">
      <div className="flex items-center justify-between">
        <span className="text-slate-500 text-sm font-medium">{label}</span>
        <span className={`material-symbols-outlined ${iconColor} p-1.5 rounded-md text-[20px]`} aria-hidden="true">{icon}</span>
      </div>
      <span className="text-slate-900 text-3xl font-bold">{value}</span>
    </div>
  );
}
