// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';

const PROPERTY_TYPES = [
  'Apartment', 'House', 'Villa', 'Penthouse', 'Studio',
  'Commercial', 'Land', 'Garage', 'StorageRoom', 'Building', 'Office',
] as const;

const PROPERTY_TYPE_ICONS: Record<string, string> = {
  Apartment: 'apartment',
  House: 'home',
  Villa: 'villa',
  Penthouse: 'roofing',
  Studio: 'studio',
  Commercial: 'store',
  Land: 'terrain',
  Garage: 'garage',
  StorageRoom: 'warehouse',
  Building: 'domain',
  Office: 'business',
};

const OPERATION_TYPES = ['Sale', 'Rent', 'SaleOrRent', 'Transfer', 'Vacation'] as const;

interface BasicInfoStepProps {
  data: Record<string, unknown>;
  onChange: (data: Record<string, unknown>) => void;
}

export function BasicInfoStep({ data, onChange }: BasicInfoStepProps) {
  const t = useTranslations('properties');
  const tForm = useTranslations('properties.form');

  const title = (data.title as string) ?? '';
  const propertyType = (data.propertyType as string) ?? '';
  const operationType = (data.operationType as string) ?? '';

  const updateField = (field: string, value: unknown) => {
    onChange({ ...data, [field]: value });
  };

  return (
    <div className="space-y-6">
      {/* Title */}
      <div>
        <label htmlFor="property-title" className="mb-1.5 block text-sm font-medium text-slate-700">
          Title <span className="text-red-500">*</span>
        </label>
        <input
          id="property-title"
          type="text"
          value={title}
          onChange={(e) => updateField('title', e.target.value)}
          maxLength={200}
          placeholder="e.g. Modern 3-Bedroom Apartment in Madrid"
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
        />
        <p className="mt-1 text-xs text-slate-400">{title.length}/200</p>
        {title.length === 0 && (
          <p className="mt-1 text-xs text-red-500">{tForm('validation.titleRequired')}</p>
        )}
      </div>

      {/* Property Type */}
      <div>
        <label className="mb-2 block text-sm font-medium text-slate-700">
          Property Type <span className="text-red-500">*</span>
        </label>
        <div className="grid grid-cols-3 gap-2 sm:grid-cols-4 md:grid-cols-6">
          {PROPERTY_TYPES.map((type) => (
            <button
              key={type}
              type="button"
              onClick={() => updateField('propertyType', type)}
              className={`flex flex-col items-center gap-1 rounded-lg border px-3 py-3 text-xs font-medium transition ${
                propertyType === type
                  ? 'border-primary-600 bg-primary-50 text-primary-700'
                  : 'border-slate-200 bg-white text-slate-600 hover:border-slate-300 hover:bg-slate-50'
              }`}
            >
              <span className="material-symbols-outlined text-lg" aria-hidden="true">
                {PROPERTY_TYPE_ICONS[type] ?? 'home'}
              </span>
              {t(`type.${type}`)}
            </button>
          ))}
        </div>
      </div>

      {/* Operation Type */}
      <div>
        <label className="mb-2 block text-sm font-medium text-slate-700">
          Operation Type <span className="text-red-500">*</span>
        </label>
        <div className="flex flex-wrap gap-2">
          {OPERATION_TYPES.map((op) => (
            <button
              key={op}
              type="button"
              onClick={() => updateField('operationType', op)}
              className={`rounded-lg border px-4 py-2 text-sm font-medium transition ${
                operationType === op
                  ? 'border-primary-600 bg-primary-600 text-white'
                  : 'border-slate-200 bg-white text-slate-600 hover:border-slate-300 hover:bg-slate-50'
              }`}
            >
              {t(`operation.${op}`)}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
