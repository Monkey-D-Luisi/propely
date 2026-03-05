// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { PropertyTypeType, OperationTypeType, PropertyStatusType } from '@/lib/schemas';

const propertyTypes: PropertyTypeType[] = [
  'Apartment', 'House', 'Villa', 'Penthouse', 'Studio',
  'Commercial', 'Land', 'Garage', 'StorageRoom', 'Building', 'Office',
];
const operationTypes: OperationTypeType[] = ['Sale', 'Rent', 'SaleOrRent', 'Transfer', 'Vacation'];
const statusTypes: PropertyStatusType[] = ['Draft', 'Active', 'Reserved', 'Sold', 'Rented', 'Archived'];

interface PropertyFilterBarProps {
  search: string;
  onSearchChange: (value: string) => void;
  type: string;
  onTypeChange: (value: string) => void;
  operation: string;
  onOperationChange: (value: string) => void;
  status: string;
  onStatusChange: (value: string) => void;
  sortBy: string;
  onSortByChange: (value: string) => void;
}

export function PropertyFilterBar({
  search, onSearchChange,
  type, onTypeChange,
  operation, onOperationChange,
  status, onStatusChange,
  sortBy, onSortByChange,
}: PropertyFilterBarProps) {
  const t = useTranslations('properties');

  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:flex-wrap">
      <div className="relative flex-1 min-w-[200px]">
        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm text-slate-400" aria-hidden="true">
          search
        </span>
        <input
          type="text"
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder={t('searchPlaceholder')}
          className="w-full rounded-lg border border-slate-200 py-2 pl-9 pr-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
        />
      </div>
      <select
        value={type}
        onChange={(e) => onTypeChange(e.target.value)}
        aria-label={t('allTypes')}
        className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
      >
        <option value="">{t('allTypes')}</option>
        {propertyTypes.map((pt) => (
          <option key={pt} value={pt}>{t(`type.${pt}`)}</option>
        ))}
      </select>
      <select
        value={operation}
        onChange={(e) => onOperationChange(e.target.value)}
        aria-label={t('allOperations')}
        className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
      >
        <option value="">{t('allOperations')}</option>
        {operationTypes.map((op) => (
          <option key={op} value={op}>{t(`operation.${op}`)}</option>
        ))}
      </select>
      <select
        value={status}
        onChange={(e) => onStatusChange(e.target.value)}
        aria-label={t('allStatuses')}
        className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
      >
        <option value="">{t('allStatuses')}</option>
        {statusTypes.map((s) => (
          <option key={s} value={s}>{t(`status.${s}`)}</option>
        ))}
      </select>
      <select
        value={sortBy}
        onChange={(e) => onSortByChange(e.target.value)}
        aria-label={t('sortBy')}
        className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
      >
        <option value="">{t('sortBy')}</option>
        <option value="price">{t('sort.price')}</option>
        <option value="createdAtUtc">{t('sort.createdAtUtc')}</option>
        <option value="builtArea">{t('sort.builtArea')}</option>
        <option value="title">{t('sort.title')}</option>
      </select>
    </div>
  );
}
