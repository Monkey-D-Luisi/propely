// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { useDashboard } from '@/hooks/use-dashboard';
import { useCurrentUser } from '@/hooks/orgs';
import { KpiGrid } from '@/components/dashboard/KpiGrid';
import { StatusPieChart } from '@/components/dashboard/charts/StatusPieChart';
import { StatusBarChart } from '@/components/dashboard/charts/StatusBarChart';
import { QuickActions } from '@/components/dashboard/QuickActions';
import { SuggestionFeed } from '@/components/suggestions/SuggestionFeed';

function sumValues(data: Record<string, number> | undefined, keys: string[]): number {
  if (!data) return 0;
  return keys.reduce((sum, key) => sum + (data[key] ?? 0), 0);
}

export default function DashboardPage() {
  const t = useTranslations('dashboard');
  const { user } = useCurrentUser();
  const { properties, leads, appointments, isLoading } = useDashboard();

  const activeListings = properties?.byStatus?.['Active'] ?? 0;
  const openLeads = sumValues(leads?.byStatus, ['New', 'Contacted', 'Qualified']);
  const upcomingAppointments = appointments?.upcoming ?? 0;
  const convertedThisMonth = leads?.byStatus?.['Converted'] ?? 0;

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
