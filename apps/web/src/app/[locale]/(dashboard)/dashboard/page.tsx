// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useDashboard } from '@/hooks/use-dashboard';
import { useCurrentUser } from '@/hooks/orgs';
import { useCommandBar } from '@/hooks/use-command-bar';
import { KpiGrid } from '@/components/dashboard/KpiGrid';
import { StatusPieChart } from '@/components/dashboard/charts/StatusPieChart';
import { StatusBarChart } from '@/components/dashboard/charts/StatusBarChart';
import { QuickActions } from '@/components/dashboard/QuickActions';
import { SuggestionFeed } from '@/components/suggestions/SuggestionFeed';

function sumValues(data: Record<string, number> | undefined, keys: string[]): number {
  if (!data) return 0;
  return keys.reduce((sum, key) => sum + (data[key] ?? 0), 0);
}

function totalValues(data: Record<string, number> | undefined): number {
  if (!data) return 0;
  return Object.values(data).reduce((sum, v) => sum + v, 0);
}

export default function DashboardPage() {
  const t = useTranslations('dashboard');
  const { user } = useCurrentUser();
  const { open: openCommandBar } = useCommandBar();
  const { properties, leads, appointments, isLoading } = useDashboard();

  const activeListings = properties?.byStatus?.['Active'] ?? 0;
  const openLeads = sumValues(leads?.byStatus, ['New', 'Contacted', 'Qualified']);
  const upcomingAppointments = appointments?.upcoming ?? 0;
  const convertedThisMonth = leads?.byStatus?.['Converted'] ?? 0;

  const totalProperties = totalValues(properties?.byStatus);
  const totalLeads = totalValues(leads?.byStatus);
  const isEmpty = !isLoading && totalProperties === 0 && totalLeads === 0 && upcomingAppointments === 0;

  if (isLoading) {
    return (
      <div className="flex-1 overflow-y-auto bg-surface p-6 lg:p-8">
        <div className="mx-auto max-w-6xl">
          <div className="mb-6">
            <div className="h-8 w-48 animate-pulse rounded-lg bg-slate-200" />
            <div className="mt-2 h-5 w-64 animate-pulse rounded-lg bg-slate-100" />
          </div>
          <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="h-24 animate-pulse rounded-xl bg-white shadow-sm" />
            ))}
          </div>
          <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
            <div className="h-80 animate-pulse rounded-xl bg-white shadow-sm" />
            <div className="h-80 animate-pulse rounded-xl bg-white shadow-sm" />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 overflow-y-auto bg-surface p-6 lg:p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-slate-500">
            {t('welcome', { name: user?.name?.split(' ')[0] ?? '' })}
          </p>
        </div>

        {isEmpty ? (
          /* Zero-state onboarding */
          <div className="rounded-xl border border-slate-200 bg-white p-8 text-center shadow-sm">
            <span className="material-symbols-outlined text-[56px] text-primary-600">rocket_launch</span>
            <h2 className="mt-4 text-xl font-semibold text-slate-900">{t('getStartedTitle')}</h2>
            <p className="mx-auto mt-2 max-w-md text-sm text-slate-500">{t('getStartedSubtitle')}</p>

            <div className="mt-6 flex flex-wrap items-center justify-center gap-3">
              <Link
                href="/properties/new"
                className="inline-flex items-center gap-2 rounded-lg bg-primary-600 px-4 py-2.5 text-sm font-medium text-white shadow-sm transition-all hover:bg-primary-600/90 active:scale-[0.98]"
              >
                <span className="material-symbols-outlined text-[18px]">add_home</span>
                {t('getStartedAddProperty')}
              </Link>
              <button
                type="button"
                onClick={openCommandBar}
                className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 shadow-sm transition-all hover:bg-slate-50 active:scale-[0.98]"
              >
                <span className="material-symbols-outlined text-[18px]">auto_awesome</span>
                {t('getStartedTryAi')}
              </button>
            </div>

            <div className="mx-auto mt-8 max-w-xs">
              <div className="flex flex-col gap-3 text-left">
                {(['getStartedStep1', 'getStartedStep2', 'getStartedStep3'] as const).map((key, i) => (
                  <div key={key} className="flex items-center gap-3">
                    <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary-600/10 text-xs font-semibold text-primary-600">
                      {i + 1}
                    </span>
                    <span className="text-sm text-slate-600">{t(key)}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        ) : (
          /* Normal dashboard with data */
          <>
            <KpiGrid
              activeListings={activeListings}
              openLeads={openLeads}
              upcomingAppointments={upcomingAppointments}
              convertedThisMonth={convertedThisMonth}
            />

            <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
              <StatusPieChart
                data={properties?.byStatus ?? {}}
                title={t('propertiesByStatus')}
              />
              <StatusBarChart
                data={leads?.byStatus ?? {}}
                title={t('leadsByStatus')}
              />
            </div>
          </>
        )}

        <div className="mt-6">
          <SuggestionFeed />
        </div>

        <div className="mt-6">
          <QuickActions />
        </div>
      </div>
    </div>
  );
}
