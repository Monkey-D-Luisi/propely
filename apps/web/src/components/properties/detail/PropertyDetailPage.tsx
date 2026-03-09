// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useProperty, usePropertyMedia, useChangePropertyStatus } from '@/hooks/properties';
import { PropertyStatusBadge } from '@/components/properties/PropertyStatusBadge';
import { PropertyPhotoGallery } from '@/components/properties/detail/PropertyPhotoGallery';
import { PropertyFloorPlans } from '@/components/properties/detail/PropertyFloorPlans';
import { PropertyInfoSections } from '@/components/properties/detail/PropertyInfoSections';
import { PropertyStatusActions } from '@/components/properties/detail/PropertyStatusActions';
import { StatusChangeDialog } from '@/components/properties/detail/StatusChangeDialog';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';
import type { PropertyStatusType } from '@/lib/schemas';

interface PropertyDetailPageProps {
  id: string;
}

export function PropertyDetailPage({ id }: PropertyDetailPageProps) {
  const t = useTranslations('properties.detail');
  const tProperties = useTranslations('properties');
  const { toast } = useToast();

  const { property, isLoading: propertyLoading, error, refetch } = useProperty(id);
  const { media, isLoading: mediaLoading } = usePropertyMedia(id);
  const changeStatus = useChangePropertyStatus();

  const [statusTarget, setStatusTarget] = useState<PropertyStatusType | null>(null);
  const [isChangingStatus, setChangingStatus] = useState(false);

  const handleStatusChangeRequest = (newStatus: PropertyStatusType) => {
    setStatusTarget(newStatus);
  };

  const handleStatusChangeConfirm = async () => {
    if (!statusTarget) return;
    setChangingStatus(true);
    try {
      await changeStatus(id, statusTarget);
      toast({ title: t('statusChangeSuccess'), variant: 'success' });
      setStatusTarget(null);
      void refetch();
    } catch {
      toast({ title: t('statusChangeError'), variant: 'destructive' });
    } finally {
      setChangingStatus(false);
    }
  };

  // Loading skeleton
  if (propertyLoading) {
    return (
      <div className="flex-1 overflow-y-auto p-8" data-testid="detail-loading">
        <div className="mx-auto max-w-6xl animate-pulse space-y-6">
          <div className="h-4 w-32 rounded-lg bg-slate-200" />
          <div className="h-8 w-64 rounded-lg bg-slate-200" />
          <div className="aspect-video w-full rounded-xl bg-slate-100" />
          <div className="space-y-4">
            <div className="h-48 rounded-xl bg-slate-100" />
            <div className="h-48 rounded-xl bg-slate-100" />
          </div>
        </div>
      </div>
    );
  }

  // Error state
  if (error || !property) {
    return (
      <div className="flex-1 overflow-y-auto p-8" data-testid="detail-error">
        <div className="mx-auto max-w-6xl">
          <ErrorMessage message={t('loadError')} />
          <Link
            href="/properties"
            className="mt-4 inline-flex items-center gap-1 text-sm text-primary-600 hover:text-primary-700"
          >
            <span className="material-symbols-outlined text-sm" aria-hidden="true">arrow_back</span>
            {t('backToList')}
          </Link>
        </div>
      </div>
    );
  }

  const floorPlanMedia = media.filter((m) => m.mediaType === 'FloorPlan');

  return (
    <div className="flex-1 overflow-y-auto p-8" data-testid="property-detail">
      <div className="mx-auto max-w-6xl">
      {/* Breadcrumb */}
      <nav className="mb-4 flex items-center gap-2 text-sm text-slate-500" aria-label="Breadcrumb">
        <Link href="/properties" className="hover:text-primary-600">
          {tProperties('title')}
        </Link>
        <span aria-hidden="true">/</span>
        <span className="text-slate-900">{property.title}</span>
      </nav>

      {/* Back link */}
      <Link
        href="/properties"
        className="mb-6 inline-flex items-center gap-1 text-sm text-slate-500 hover:text-primary-600"
        data-testid="back-link"
      >
        <span className="material-symbols-outlined text-sm" aria-hidden="true">arrow_back</span>
        {t('backToList')}
      </Link>

      {/* Header */}
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="flex items-start gap-3">
          <div>
            <h1 className="text-2xl font-bold text-slate-900" data-testid="property-title">
              {property.title}
            </h1>
            {property.address?.city && (
              <p className="mt-1 flex items-center gap-1 text-sm text-slate-500">
                <span className="material-symbols-outlined text-sm" aria-hidden="true">location_on</span>
                {[property.address.street, property.address.city, property.address.province]
                  .filter(Boolean)
                  .join(', ')}
              </p>
            )}
          </div>
          <PropertyStatusBadge
            status={property.status}
            label={tProperties(`status.${property.status}`)}
          />
        </div>
        <div className="flex items-center gap-2">
          <Link
            href={`/properties/${id}/edit`}
            className="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50"
            data-testid="edit-link"
          >
            <span className="material-symbols-outlined text-sm" aria-hidden="true">edit</span>
            {t('edit')}
          </Link>
        </div>
      </div>

      {/* Status action buttons */}
      <div className="mb-6">
        <PropertyStatusActions
          property={property}
          onStatusChange={handleStatusChangeRequest}
        />
      </div>

      {/* Photo gallery */}
      <div className="mb-6">
        <h2 className="mb-3 text-lg font-semibold text-slate-900">{t('media')}</h2>
        <PropertyPhotoGallery media={media} />
      </div>

      {/* Floor plans */}
      {floorPlanMedia.length > 0 && (
        <div className="mb-6">
          <h2 className="mb-3 text-lg font-semibold text-slate-900">{t('floorPlans')}</h2>
          <PropertyFloorPlans media={media} />
        </div>
      )}

      {/* Info sections */}
      <PropertyInfoSections property={property} />

      {/* Status change dialog */}
      <StatusChangeDialog
        isOpen={statusTarget !== null}
        onClose={() => setStatusTarget(null)}
        onConfirm={() => void handleStatusChangeConfirm()}
        fromStatus={property.status}
        toStatus={statusTarget ?? ''}
        isLoading={isChangingStatus}
      />
      </div>
    </div>
  );
}
