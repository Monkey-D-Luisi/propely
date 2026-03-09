// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useParams } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useRequireAuth } from '@/hooks/useRequireAuth';
import { useActiveOrg } from '@/hooks/use-active-org';
import { useProperty } from '@/hooks/properties';
import { PropertyFormWizard } from '@/components/properties/form/PropertyFormWizard';
import { ErrorMessage } from '@/components/ui/ErrorMessage';

export default function EditPropertyPage() {
  const t = useTranslations('properties.form');
  const tProperties = useTranslations('properties');
  const params = useParams();
  const id = params.id as string;
  const { user, isLoading: userLoading } = useRequireAuth();
  const { hasOrgs, isLoading: orgsLoading } = useActiveOrg();
  const { property, isLoading: propertyLoading, error } = useProperty(id);

  if (userLoading || orgsLoading || propertyLoading) {
    return (
      <div className="flex-1 overflow-y-auto p-8">
        <div className="mx-auto max-w-6xl animate-pulse space-y-4">
          <div className="h-8 w-48 rounded-lg bg-slate-200" />
          <div className="h-4 w-72 rounded-lg bg-slate-100" />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex-1 overflow-y-auto p-8">
        <div className="mx-auto max-w-6xl">
          <ErrorMessage message={tProperties('loadError')} />
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
        <Link href={`/properties/${id}`} className="hover:text-primary-600">{property?.title ?? '...'}</Link>
        <span>/</span>
        <span className="text-slate-900">{t('editTitle')}</span>
      </div>
      <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('editTitle')}</h1>
      <PropertyFormWizard existingProperty={property} />
      </div>
    </div>
  );
}
