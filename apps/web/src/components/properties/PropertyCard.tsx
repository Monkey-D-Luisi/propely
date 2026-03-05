// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import type { PropertyListItem } from '@/lib/schemas';
import { PropertyStatusBadge } from './PropertyStatusBadge';

interface PropertyCardProps {
  item: PropertyListItem;
}

function formatPrice(price: number | null | undefined): string {
  if (price == null) return '—';
  return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR', maximumFractionDigits: 0 }).format(price);
}

export function PropertyCard({ item }: PropertyCardProps) {
  const t = useTranslations('properties');

  return (
    <Link
      href={`/properties/${item.id}`}
      className="group flex flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm transition hover:shadow-md"
    >
      <div className="relative h-40 bg-slate-100 flex items-center justify-center">
        <span className="material-symbols-outlined text-4xl text-slate-300" aria-hidden="true">apartment</span>
        <div className="absolute top-2 right-2">
          <PropertyStatusBadge status={item.status} label={t(`status.${item.status}`)} />
        </div>
      </div>
      <div className="flex flex-1 flex-col gap-2 p-4">
        <h3 className="truncate font-semibold text-slate-900 group-hover:text-primary-600">{item.title}</h3>
        {item.city && (
          <p className="flex items-center gap-1 text-xs text-slate-500">
            <span className="material-symbols-outlined text-xs" aria-hidden="true">location_on</span>
            {item.city}
          </p>
        )}
        <div className="mt-auto flex items-center justify-between pt-2">
          <span className="text-lg font-bold text-slate-900">{formatPrice(item.price)}</span>
          <span className="text-xs text-slate-500">{t(`type.${item.propertyType}`)}</span>
        </div>
        <div className="flex gap-3 text-xs text-slate-500">
          {item.bedrooms != null && (
            <span className="flex items-center gap-0.5">
              <span className="material-symbols-outlined text-xs" aria-hidden="true">bed</span>
              {item.bedrooms}
            </span>
          )}
          {item.bathrooms != null && (
            <span className="flex items-center gap-0.5">
              <span className="material-symbols-outlined text-xs" aria-hidden="true">bathtub</span>
              {item.bathrooms}
            </span>
          )}
          {item.builtArea != null && (
            <span className="flex items-center gap-0.5">
              <span className="material-symbols-outlined text-xs" aria-hidden="true">square_foot</span>
              {item.builtArea} m²
            </span>
          )}
        </div>
      </div>
    </Link>
  );
}
