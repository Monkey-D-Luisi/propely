// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useRequireAuth } from '@/hooks/useRequireAuth';
import { useActiveOrg } from '@/hooks/use-active-org';
import { PropertyFormWizard } from '@/components/properties/form/PropertyFormWizard';

export default function NewPropertyPage() {
  const t = useTranslations('properties.form');
  const { user, isLoading: userLoading } = useRequireAuth();
  const { hasOrgs, isLoading: orgsLoading } = useActiveOrg();

  if (userLoading || orgsLoading) {
    return (
      <div className="flex-1 overflow-y-auto p-8">
        <div className="mx-auto max-w-6xl animate-pulse space-y-4">
          <div className="h-8 w-48 rounded-lg bg-slate-200" />
          <div className="h-4 w-72 rounded-lg bg-slate-100" />
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 overflow-y-auto p-8">
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      <div className="flex items-center gap-2 text-sm text-slate-500">
        <Link href="/properties" className="hover:text-primary-600">Properties</Link>
        <span>/</span>
        <span className="text-slate-900">{t('createTitle')}</span>
      </div>
      <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('createTitle')}</h1>
      <PropertyFormWizard />
    </div>
    </div>
  );
}
