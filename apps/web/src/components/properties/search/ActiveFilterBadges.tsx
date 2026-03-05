// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { PropertyFilters } from '@/hooks/properties';

interface ActiveFilterBadgesProps {
  filters: PropertyFilters;
  onRemove: (key: keyof PropertyFilters) => void;
  onClearAll: () => void;
}

function formatPrice(value: number): string {
  if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
  if (value >= 1_000) return `${(value / 1_000).toFixed(0)}k`;
  return String(value);
}

export function ActiveFilterBadges({ filters, onRemove, onClearAll }: ActiveFilterBadgesProps) {
  const t = useTranslations('properties');
  const tSearch = useTranslations('properties.search');

  const badges: { key: keyof PropertyFilters; label: string }[] = [];

  if (filters.type) {
    badges.push({ key: 'type', label: `${t('table.type')}: ${t(`type.${filters.type}`)}` });
  }
  if (filters.operation) {
    badges.push({ key: 'operation', label: `${t('table.operation')}: ${t(`operation.${filters.operation}`)}` });
  }
  if (filters.status) {
    badges.push({ key: 'status', label: `${t('table.status')}: ${t(`status.${filters.status}`)}` });
  }
  if (filters.minPrice != null && filters.maxPrice != null) {
    badges.push({ key: 'minPrice', label: `${tSearch('priceRange')}: €${formatPrice(filters.minPrice)}-€${formatPrice(filters.maxPrice)}` });
  } else if (filters.minPrice != null) {
    badges.push({ key: 'minPrice', label: `${tSearch('priceRange')}: €${formatPrice(filters.minPrice)}+` });
  } else if (filters.maxPrice != null) {
    badges.push({ key: 'maxPrice', label: `${tSearch('priceRange')}: ≤€${formatPrice(filters.maxPrice)}` });
  }
  if (filters.minArea != null && filters.maxArea != null) {
    badges.push({ key: 'minArea', label: `${tSearch('areaRange')}: ${filters.minArea}-${filters.maxArea} m²` });
  } else if (filters.minArea != null) {
    badges.push({ key: 'minArea', label: `${tSearch('areaRange')}: ${filters.minArea}+ m²` });
  } else if (filters.maxArea != null) {
    badges.push({ key: 'maxArea', label: `${tSearch('areaRange')}: ≤${filters.maxArea} m²` });
  }
  if (filters.minBedrooms != null) {
    badges.push({ key: 'minBedrooms', label: `${tSearch('minBedrooms')}: ${filters.minBedrooms}+` });
  }
  if (filters.minBathrooms != null) {
    badges.push({ key: 'minBathrooms', label: `${tSearch('minBathrooms')}: ${filters.minBathrooms}+` });
  }
  if (filters.city) {
    badges.push({ key: 'city', label: `City: ${filters.city}` });
  }
  if (filters.hasPool) {
    badges.push({ key: 'hasPool', label: t('detail.pool') });
  }
  if (filters.hasGarden) {
    badges.push({ key: 'hasGarden', label: t('detail.garden') });
  }
  if (filters.hasGarage) {
    badges.push({ key: 'hasGarage', label: t('detail.garage') });
  }
  if (filters.hasElevator) {
    badges.push({ key: 'hasElevator', label: t('detail.elevator') });
  }
  if (filters.hasTerrace) {
    badges.push({ key: 'hasTerrace', label: t('detail.terrace') });
  }

  if (badges.length === 0) return null;

  return (
    <div className="flex flex-wrap items-center gap-2" data-testid="active-filter-badges">
      {badges.map(({ key, label }) => (
        <span
          key={key}
          className="inline-flex items-center gap-1 rounded-full border border-primary-200 bg-primary-50 px-3 py-1 text-sm text-primary-700"
        >
          {label}
          <button
            type="button"
            onClick={() => onRemove(key)}
            className="ml-0.5 inline-flex h-4 w-4 items-center justify-center rounded-full hover:bg-primary-200 transition"
            aria-label={`Remove ${label}`}
          >
            <span className="material-symbols-outlined text-xs" aria-hidden="true">close</span>
          </button>
        </span>
      ))}
      <button
        type="button"
        onClick={onClearAll}
        className="text-sm font-medium text-primary-600 hover:text-primary-700 transition"
      >
        {tSearch('clearAll')}
      </button>
    </div>
  );
}
