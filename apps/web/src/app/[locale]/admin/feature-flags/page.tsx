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
      <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
        <div className="h-8 w-48 animate-pulse rounded bg-slate-100" />
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div
              key={i}
              className="h-14 animate-pulse rounded-md bg-slate-100"
            />
          ))}
        </div>
      </div>
    );
  }

  if (!user) {
    return null;
  }

  return (
    <div className="mx-auto flex min-h-dvh w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
      <div className="border-b border-slate-100 pb-6">
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
        <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
      </div>
      <FeatureFlagTable />
    </div>
  );
}
