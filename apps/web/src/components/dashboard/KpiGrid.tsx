// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';

type KpiCardProps = {
  icon: string;
  value: number;
  label: string;
  accentColor: string;
  iconBg: string;
};

export function KpiCard({ icon, value, label, accentColor, iconBg }: KpiCardProps) {
  const t = useTranslations('dashboard');

  return (
    <div
      className={`rounded-xl bg-white p-5 shadow-sm border-l-4 ${accentColor}`}
      data-testid="kpi-card"
    >
      <div className="flex items-center gap-4">
        <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${iconBg}`}>
          <span className="material-symbols-outlined text-xl">{icon}</span>
        </div>
        <div>
          <p className="text-2xl font-bold text-slate-900">{value}</p>
          <p className="text-sm text-slate-500">{label}</p>
        </div>
      </div>
    </div>
  );
}

type KpiGridProps = {
  activeListings: number;
  openLeads: number;
  upcomingAppointments: number;
  convertedThisMonth: number;
};

export function KpiGrid({
  activeListings,
  openLeads,
  upcomingAppointments,
  convertedThisMonth,
}: KpiGridProps) {
  const t = useTranslations('dashboard');

  return (
    <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
      <KpiCard
        icon="home"
        value={activeListings}
        label={t('activeListings')}
        accentColor="border-primary-600"
        iconBg="bg-primary-600/10 text-primary-600"
      />
      <KpiCard
        icon="people"
        value={openLeads}
        label={t('openLeads')}
        accentColor="border-amber-500"
        iconBg="bg-amber-500/10 text-amber-500"
      />
      <KpiCard
        icon="calendar_today"
        value={upcomingAppointments}
        label={t('upcomingAppointments')}
        accentColor="border-emerald-500"
        iconBg="bg-emerald-500/10 text-emerald-500"
      />
      <KpiCard
        icon="trending_up"
        value={convertedThisMonth}
        label={t('convertedThisMonth')}
        accentColor="border-sky-500"
        iconBg="bg-sky-500/10 text-sky-500"
      />
    </div>
  );
}
