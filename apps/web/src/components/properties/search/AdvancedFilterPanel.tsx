// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import type { PropertyTypeType, OperationTypeType, PropertyStatusType } from '@/lib/schemas';

const propertyTypes: PropertyTypeType[] = [
  'Apartment', 'House', 'Villa', 'Penthouse', 'Studio',
  'Commercial', 'Land', 'Garage', 'StorageRoom', 'Building', 'Office',
];
const operationTypes: OperationTypeType[] = ['Sale', 'Rent', 'SaleOrRent', 'Transfer', 'Vacation'];
const statusTypes: PropertyStatusType[] = ['Draft', 'Active', 'Reserved', 'Sold', 'Rented', 'Archived'];

export interface AdvancedFilters {
  types: PropertyTypeType[];
  operations: OperationTypeType[];
  statuses: PropertyStatusType[];
  minPrice?: number;
  maxPrice?: number;
  minArea?: number;
  maxArea?: number;
  minBedrooms?: number;
  minBathrooms?: number;
  city?: string;
  province?: string;
  hasPool?: boolean;
  hasGarden?: boolean;
  hasGarage?: boolean;
  hasElevator?: boolean;
  hasTerrace?: boolean;
}

const emptyFilters: AdvancedFilters = {
  types: [],
  operations: [],
  statuses: [],
};

interface AdvancedFilterPanelProps {
  filters: AdvancedFilters;
  onFiltersChange: (filters: AdvancedFilters) => void;
  onClear: () => void;
}

export function AdvancedFilterPanel({ filters, onFiltersChange, onClear }: AdvancedFilterPanelProps) {
  const t = useTranslations('properties');
  const tSearch = useTranslations('properties.search');
  const [isOpen, setOpen] = useState(false);

  const activeCount = [
    filters.types.length > 0,
    filters.operations.length > 0,
    filters.statuses.length > 0,
    filters.minPrice != null,
    filters.maxPrice != null,
    filters.minArea != null,
    filters.maxArea != null,
    filters.minBedrooms != null,
    filters.minBathrooms != null,
    !!filters.city,
    !!filters.province,
    filters.hasPool,
    filters.hasGarden,
    filters.hasGarage,
    filters.hasElevator,
    filters.hasTerrace,
  ].filter(Boolean).length;

  const toggleMulti = <T extends string>(arr: T[], value: T): T[] =>
    arr.includes(value) ? arr.filter((v) => v !== value) : [...arr, value];

  return (
    <div>
      <button
        type="button"
        onClick={() => setOpen(!isOpen)}
        className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
      >
        <span className="material-symbols-outlined text-sm" aria-hidden="true">tune</span>
        {tSearch('advanced')}
        {activeCount > 0 && (
          <span className="flex h-5 w-5 items-center justify-center rounded-full bg-primary-600 text-[10px] font-bold text-white">
            {activeCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="mt-3 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {/* Property Type multi-select */}
            <div>
              <label className="mb-2 block text-xs font-medium text-slate-500">{t('table.type')}</label>
              <div className="flex flex-wrap gap-1.5">
                {propertyTypes.map((pt) => (
                  <button
                    key={pt}
                    type="button"
                    onClick={() => onFiltersChange({ ...filters, types: toggleMulti(filters.types, pt) })}
                    className={`rounded-full px-2.5 py-1 text-xs font-medium transition ${
                      filters.types.includes(pt)
                        ? 'bg-primary-100 text-primary-700'
                        : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                    }`}
                  >
                    {t(`type.${pt}`)}
                  </button>
                ))}
              </div>
            </div>

            {/* Operation Type multi-select */}
            <div>
              <label className="mb-2 block text-xs font-medium text-slate-500">{t('table.operation')}</label>
              <div className="flex flex-wrap gap-1.5">
                {operationTypes.map((op) => (
                  <button
                    key={op}
                    type="button"
                    onClick={() => onFiltersChange({ ...filters, operations: toggleMulti(filters.operations, op) })}
                    className={`rounded-full px-2.5 py-1 text-xs font-medium transition ${
                      filters.operations.includes(op)
                        ? 'bg-primary-100 text-primary-700'
                        : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                    }`}
                  >
                    {t(`operation.${op}`)}
                  </button>
                ))}
              </div>
            </div>

            {/* Status multi-select */}
            <div>
              <label className="mb-2 block text-xs font-medium text-slate-500">{t('table.status')}</label>
              <div className="flex flex-wrap gap-1.5">
                {statusTypes.map((s) => (
                  <button
                    key={s}
                    type="button"
                    onClick={() => onFiltersChange({ ...filters, statuses: toggleMulti(filters.statuses, s) })}
                    className={`rounded-full px-2.5 py-1 text-xs font-medium transition ${
                      filters.statuses.includes(s)
                        ? 'bg-primary-100 text-primary-700'
                        : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                    }`}
                  >
                    {t(`status.${s}`)}
                  </button>
                ))}
              </div>
            </div>

            {/* Price range */}
            <div>
              <label className="mb-2 block text-xs font-medium text-slate-500">{tSearch('priceRange')}</label>
              <div className="flex items-center gap-2">
                <input
                  type="number"
                  placeholder="Min"
                  value={filters.minPrice ?? ''}
                  onChange={(e) => onFiltersChange({ ...filters, minPrice: e.target.value ? Number(e.target.value) : undefined })}
                  className="w-full rounded-lg border border-slate-200 px-3 py-1.5 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
                />
                <span className="text-slate-400">—</span>
                <input
                  type="number"
                  placeholder="Max"
                  value={filters.maxPrice ?? ''}
                  onChange={(e) => onFiltersChange({ ...filters, maxPrice: e.target.value ? Number(e.target.value) : undefined })}
                  className="w-full rounded-lg border border-slate-200 px-3 py-1.5 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
                />
              </div>
            </div>

            {/* Area range */}
            <div>
              <label className="mb-2 block text-xs font-medium text-slate-500">{tSearch('areaRange')} (m²)</label>
              <div className="flex items-center gap-2">
                <input
                  type="number"
                  placeholder="Min"
                  value={filters.minArea ?? ''}
                  onChange={(e) => onFiltersChange({ ...filters, minArea: e.target.value ? Number(e.target.value) : undefined })}
                  className="w-full rounded-lg border border-slate-200 px-3 py-1.5 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
                />
                <span className="text-slate-400">—</span>
                <input
                  type="number"
                  placeholder="Max"
                  value={filters.maxArea ?? ''}
                  onChange={(e) => onFiltersChange({ ...filters, maxArea: e.target.value ? Number(e.target.value) : undefined })}
                  className="w-full rounded-lg border border-slate-200 px-3 py-1.5 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
                />
              </div>
            </div>

            {/* Bedrooms & Bathrooms */}
            <div>
              <div className="space-y-3">
                <div>
                  <label className="mb-1 block text-xs font-medium text-slate-500">{tSearch('minBedrooms')}</label>
                  <input
                    type="number"
                    min={0}
                    value={filters.minBedrooms ?? ''}
                    onChange={(e) => onFiltersChange({ ...filters, minBedrooms: e.target.value ? Number(e.target.value) : undefined })}
                    className="w-full rounded-lg border border-slate-200 px-3 py-1.5 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
                  />
                </div>
                <div>
                  <label className="mb-1 block text-xs font-medium text-slate-500">{tSearch('minBathrooms')}</label>
                  <input
                    type="number"
                    min={0}
                    value={filters.minBathrooms ?? ''}
                    onChange={(e) => onFiltersChange({ ...filters, minBathrooms: e.target.value ? Number(e.target.value) : undefined })}
                    className="w-full rounded-lg border border-slate-200 px-3 py-1.5 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
                  />
                </div>
              </div>
            </div>

            {/* Amenities */}
            <div className="sm:col-span-2 lg:col-span-3">
              <label className="mb-2 block text-xs font-medium text-slate-500">{tSearch('amenities')}</label>
              <div className="flex flex-wrap gap-2">
                {(['hasPool', 'hasGarden', 'hasGarage', 'hasElevator', 'hasTerrace'] as const).map((amenity) => (
                  <button
                    key={amenity}
                    type="button"
                    onClick={() => onFiltersChange({ ...filters, [amenity]: filters[amenity] ? undefined : true })}
                    className={`rounded-full px-3 py-1.5 text-xs font-medium transition ${
                      filters[amenity]
                        ? 'bg-green-100 text-green-700'
                        : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                    }`}
                  >
                    {t(`detail.${amenity === 'hasPool' ? 'pool' : amenity === 'hasGarden' ? 'garden' : amenity === 'hasGarage' ? 'garage' : amenity === 'hasElevator' ? 'elevator' : 'terrace'}`)}
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Clear all */}
          {activeCount > 0 && (
            <div className="mt-4 border-t border-slate-100 pt-4">
              <button
                type="button"
                onClick={onClear}
                className="text-sm font-medium text-primary-600 hover:text-primary-700"
              >
                {tSearch('clearAll')}
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
