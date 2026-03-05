// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import type { Property, LocalizedText } from '@/lib/schemas';

interface PropertyInfoSectionsProps {
  property: Property;
}

function formatPrice(price: number | null | undefined): string {
  if (price == null) return '\u2014';
  return new Intl.NumberFormat('es-ES', {
    style: 'currency',
    currency: 'EUR',
    maximumFractionDigits: 0,
  }).format(price);
}

function InfoItem({ label, value }: { label: string; value: string | number | null | undefined }) {
  if (value == null || value === '') return null;
  return (
    <div data-testid="info-item">
      <dt className="text-sm text-slate-500">{label}</dt>
      <dd className="text-sm font-medium text-slate-900">{value}</dd>
    </div>
  );
}

function AmenityBadge({ label }: { label: string }) {
  return (
    <span className="bg-primary-50 text-primary-700 border border-primary-200 rounded-full px-3 py-1 text-sm">
      {label}
    </span>
  );
}

const LANGUAGE_OPTIONS = [
  { key: 'es' as const, label: 'Espanol' },
  { key: 'en' as const, label: 'English' },
  { key: 'pt' as const, label: 'Portugues' },
  { key: 'fr' as const, label: 'Francais' },
  { key: 'de' as const, label: 'Deutsch' },
  { key: 'nl' as const, label: 'Nederlands' },
] as const;

export function PropertyInfoSections({ property }: PropertyInfoSectionsProps) {
  const t = useTranslations('properties.detail');
  const tType = useTranslations('properties.type');
  const tOp = useTranslations('properties.operation');
  const features = property.features;
  const financials = property.financials;
  const address = property.address;
  const desc = property.description;

  // Amenity checks
  const amenities = features
    ? [
        { key: 'hasPool', label: t('pool'), active: !!features.hasPool },
        { key: 'hasGarden', label: t('garden'), active: !!features.hasGarden },
        { key: 'hasGarage', label: t('garage'), active: !!features.hasGarage },
        { key: 'hasElevator', label: t('elevator'), active: !!features.hasElevator },
        { key: 'hasTerrace', label: t('terrace'), active: !!features.hasTerrace },
        { key: 'airConditioning', label: t('airConditioning'), active: !!features.airConditioning },
        { key: 'heating', label: t('heating'), active: !!features.heating },
        { key: 'furnished', label: t('furnished'), active: !!features.furnished },
      ].filter((a) => a.active)
    : [];

  return (
    <div className="space-y-6" data-testid="property-info-sections">
      {/* Basic Information */}
      <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm" data-testid="section-basic-info">
        <h2 className="mb-4 text-lg font-semibold text-slate-900">{t('basicInfo')}</h2>
        <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <InfoItem label={t('propertyType')} value={tType(property.propertyType)} />
          <InfoItem label={t('operationType')} value={tOp(property.operationType)} />
          <InfoItem label={t('title')} value={property.title} />
        </dl>
      </section>

      {/* Location */}
      <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm" data-testid="section-location">
        <h2 className="mb-4 text-lg font-semibold text-slate-900">{t('location')}</h2>
        <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <InfoItem label={t('street')} value={address?.street} />
          <InfoItem label={t('city')} value={address?.city} />
          <InfoItem label={t('province')} value={address?.province} />
          <InfoItem label={t('postalCode')} value={address?.postalCode} />
          <InfoItem label={t('country')} value={address?.country} />
        </dl>
        {address?.latitude != null && address?.longitude != null && (
          <div
            className="mt-4 flex h-48 items-center justify-center rounded-lg bg-slate-100"
            data-testid="map-placeholder"
          >
            <div className="text-center">
              <span className="material-symbols-outlined text-3xl text-slate-400" aria-hidden="true">
                location_on
              </span>
              <p className="mt-1 text-sm text-slate-400">
                {t('mapPlaceholder')} ({address.latitude.toFixed(6)}, {address.longitude.toFixed(6)})
              </p>
            </div>
          </div>
        )}
      </section>

      {/* Features */}
      <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm" data-testid="section-features">
        <h2 className="mb-4 text-lg font-semibold text-slate-900">{t('features')}</h2>
        <dl className="grid grid-cols-2 gap-4 sm:grid-cols-3">
          <InfoItem label={t('bedrooms')} value={features?.bedrooms} />
          <InfoItem label={t('bathrooms')} value={features?.bathrooms} />
          <InfoItem label={t('builtArea')} value={features?.builtArea ? `${features.builtArea} m\u00b2` : null} />
          <InfoItem label={t('usableArea')} value={features?.usableArea ? `${features.usableArea} m\u00b2` : null} />
          <InfoItem label={t('plotArea')} value={features?.plotArea ? `${features.plotArea} m\u00b2` : null} />
          <InfoItem label={t('floor')} value={features?.floor} />
          <InfoItem label={t('yearBuilt')} value={features?.yearBuilt} />
          <InfoItem label={t('orientation')} value={features?.orientation} />
          <InfoItem label={t('energyRating')} value={features?.energyRating} />
        </dl>

        {/* Amenities */}
        {amenities.length > 0 && (
          <div className="mt-6" data-testid="amenities-section">
            <h3 className="mb-3 text-sm font-medium text-slate-700">{t('amenities')}</h3>
            <div className="flex flex-wrap gap-2">
              {amenities.map((amenity) => (
                <AmenityBadge key={amenity.key} label={amenity.label} />
              ))}
            </div>
          </div>
        )}
      </section>

      {/* Financial */}
      <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm" data-testid="section-financial">
        <h2 className="mb-4 text-lg font-semibold text-slate-900">{t('financial')}</h2>
        {financials?.price != null && (
          <div className="mb-4">
            <p className="text-3xl font-bold text-slate-900" data-testid="price-display">
              {formatPrice(financials.price)}
            </p>
            {property.pricePerSqm != null && (
              <p className="mt-1 text-sm text-slate-500" data-testid="price-per-sqm">
                {t('pricePerSqm')}: {formatPrice(property.pricePerSqm)}
              </p>
            )}
          </div>
        )}
        <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <InfoItem
            label={t('communityFees')}
            value={financials?.communityFees ? `${formatPrice(financials.communityFees)}/mo` : null}
          />
          <InfoItem
            label={t('ibiTax')}
            value={financials?.ibiTax ? `${formatPrice(financials.ibiTax)}/yr` : null}
          />
          <InfoItem label={t('catastroRef')} value={financials?.catastroReference} />
        </dl>
      </section>

      {/* Descriptions */}
      <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm" data-testid="section-descriptions">
        <h2 className="mb-4 text-lg font-semibold text-slate-900">{t('descriptions')}</h2>
        {desc ? (
          <DescriptionTabs description={desc} noDescriptionText={t('noDescription')} />
        ) : (
          <p className="text-sm text-slate-500">{t('noDescription')}</p>
        )}
      </section>
    </div>
  );
}

function DescriptionTabs({
  description,
  noDescriptionText,
}: {
  description: NonNullable<Property['description']>;
  noDescriptionText: string;
}) {
  // Filter to only languages that have non-null, non-empty content
  const languages = LANGUAGE_OPTIONS.filter(
    (lang) => description[lang.key] != null && description[lang.key] !== '',
  );

  const [activeTab, setActiveTab] = useState(languages[0]?.key ?? 'es');

  if (languages.length === 0) {
    return <p className="text-sm text-slate-500">{noDescriptionText}</p>;
  }

  return (
    <div>
      <div className="flex gap-1 border-b border-slate-200" role="tablist">
        {languages.map((lang) => (
          <button
            key={lang.key}
            type="button"
            role="tab"
            aria-selected={activeTab === lang.key}
            onClick={() => setActiveTab(lang.key)}
            className={`px-4 py-2 text-sm font-medium transition ${
              activeTab === lang.key
                ? 'border-b-2 border-primary-600 text-primary-700'
                : 'text-slate-500 hover:text-slate-700'
            }`}
            data-testid={`lang-tab-${lang.key}`}
          >
            {lang.label}
          </button>
        ))}
      </div>
      <div
        className="mt-4 whitespace-pre-wrap text-sm leading-relaxed text-slate-700"
        role="tabpanel"
        data-testid="description-content"
      >
        {description[activeTab as keyof LocalizedText] || ''}
      </div>
    </div>
  );
}
