// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { rankWith, scopeEndsWith } from '@jsonforms/core';
import type { ControlProps } from '@jsonforms/core';
import { withJsonFormsControlProps } from '@jsonforms/react';

const PROPERTY_TYPES = [
  { value: 'Apartment', icon: '\ud83c\udfe2' },
  { value: 'House', icon: '\ud83c\udfe0' },
  { value: 'Villa', icon: '\ud83c\udfd8\ufe0f' },
  { value: 'Penthouse', icon: '\ud83c\udf06' },
  { value: 'Studio', icon: '\ud83d\udecb\ufe0f' },
  { value: 'Commercial', icon: '\ud83c\udfea' },
  { value: 'Land', icon: '\ud83c\udf33' },
  { value: 'Garage', icon: '\ud83d\ude97' },
  { value: 'StorageRoom', icon: '\ud83d\udce6' },
  { value: 'Building', icon: '\ud83c\udfd7\ufe0f' },
  { value: 'Office', icon: '\ud83d\udcbc' },
] as const;

function PropertyTypeRendererComponent({ data, handleChange, path, label }: ControlProps) {
  return (
    <div className="flex flex-col gap-2">
      <label className="text-sm font-medium text-slate-700">{label || 'Property Type'}</label>
      <div className="grid grid-cols-3 sm:grid-cols-4 gap-2" role="radiogroup" aria-label="Property Type">
        {PROPERTY_TYPES.map(({ value, icon }) => (
          <button
            key={value}
            type="button"
            role="radio"
            aria-checked={data === value}
            onClick={() => handleChange(path, value)}
            className={`flex flex-col items-center gap-1 p-2.5 rounded-lg text-xs font-medium transition-all border ${
              data === value
                ? 'border-primary-600 bg-primary-50 text-primary-700 ring-1 ring-primary-600'
                : 'border-slate-200 bg-white text-slate-600 hover:bg-slate-50'
            }`}
          >
            <span className="text-lg">{icon}</span>
            <span>{value}</span>
          </button>
        ))}
      </div>
    </div>
  );
}

export const propertyTypeTester = rankWith(5, scopeEndsWith('propertyType'));
export const PropertyTypeRenderer = withJsonFormsControlProps(PropertyTypeRendererComponent);
