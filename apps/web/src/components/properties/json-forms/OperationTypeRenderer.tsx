// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { rankWith, scopeEndsWith } from '@jsonforms/core';
import type { ControlProps } from '@jsonforms/core';
import { withJsonFormsControlProps } from '@jsonforms/react';
import { useTranslations } from 'next-intl';

const OPERATION_TYPES = [
  { value: 'Sale', icon: '\ud83d\udcb0' },
  { value: 'Rent', icon: '\ud83d\udd11' },
  { value: 'SaleOrRent', icon: '\ud83d\udd04' },
  { value: 'Transfer', icon: '\ud83d\udce4' },
  { value: 'Vacation', icon: '\ud83c\udfd6\ufe0f' },
] as const;

function OperationTypeRendererComponent({ data, handleChange, path, label }: ControlProps) {
  const t = useTranslations('properties');

  return (
    <div className="flex flex-col gap-2">
      <label className="text-sm font-medium text-slate-700">{label || t('form.operationType')}</label>
      <div className="flex gap-2" role="radiogroup" aria-label={label || t('form.operationType')}>
        {OPERATION_TYPES.map(({ value, icon }) => (
          <button
            key={value}
            type="button"
            role="radio"
            aria-checked={data === value}
            onClick={() => handleChange(path, value)}
            className={`flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all border ${
              data === value
                ? 'border-primary-600 bg-primary-50 text-primary-700 ring-1 ring-primary-600'
                : 'border-slate-200 bg-white text-slate-600 hover:bg-slate-50'
            }`}
          >
            <span>{icon}</span>
            <span>{t(`operation.${value}`)}</span>
          </button>
        ))}
      </div>
    </div>
  );
}

export const operationTypeTester = rankWith(5, scopeEndsWith('operationType'));
export const OperationTypeRenderer = withJsonFormsControlProps(OperationTypeRendererComponent);
