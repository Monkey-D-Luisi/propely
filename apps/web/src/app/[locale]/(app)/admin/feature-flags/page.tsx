// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { useRequireAuth } from '@/hooks/useRequireAuth';
import { FeatureFlagTable } from '@/components/admin/FeatureFlagTable';

export default function FeatureFlagsPage() {
  const t = useTranslations('featureFlags');
  const { user, isLoading } = useRequireAuth();

  if (isLoading) {
    return (
      <main className="flex-1 w-full max-w-[1440px] mx-auto px-10 py-8">
        <div className="h-8 w-48 animate-pulse rounded bg-slate-100" />
        <div className="mt-6 space-y-3">
          {[1, 2, 3].map((i) => (
            <div
              key={i}
              className="h-14 animate-pulse rounded-md bg-slate-100"
            />
          ))}
        </div>
      </main>
    );
  }

  if (!user) {
    return null;
  }

  return (
    <main className="flex-1 w-full max-w-[1440px] mx-auto px-10 py-8">
      <div className="flex flex-wrap items-center justify-between gap-4 mb-8">
        <div className="flex items-center gap-3">
          <h1 className="text-3xl font-bold leading-tight tracking-tight text-slate-900">{t('title')}</h1>
          <span className="bg-red-100 text-red-700 text-xs font-semibold px-2.5 py-0.5 rounded-full border border-red-200">
            Admin Only
          </span>
        </div>
      </div>
      <FeatureFlagTable />
    </main>
  );
}
