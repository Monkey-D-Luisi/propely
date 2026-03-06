// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { useRequireAuth } from '@/hooks/useRequireAuth';
import { VersionInfo } from '@/components/admin/VersionInfo';

export default function VersionPage() {
  const t = useTranslations('version');
  const { user, isLoading } = useRequireAuth();

  if (isLoading) {
    return (
      <main className="flex-1 w-full max-w-[1200px] mx-auto px-4 lg:px-8 py-8">
        <div className="h-8 w-48 animate-pulse rounded bg-slate-100" />
        <div className="mt-6 h-40 animate-pulse rounded-xl bg-slate-100" />
      </main>
    );
  }

  if (!user) {
    return null;
  }

  return (
    <main className="flex-1 w-full max-w-[1200px] mx-auto px-4 lg:px-8 py-8 space-y-6">
      <div className="flex items-center gap-3">
        <h1 className="text-3xl font-bold leading-tight tracking-tight text-slate-900">{t('title')}</h1>
        <span className="bg-red-100 text-red-700 text-xs font-semibold px-2.5 py-0.5 rounded-full border border-red-200">
          Admin Only
        </span>
      </div>
      <p className="text-slate-500 text-sm font-medium">{t('subtitle')}</p>
      <VersionInfo />
    </main>
  );
}
