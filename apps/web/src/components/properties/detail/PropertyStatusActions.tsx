// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { Property, PropertyStatusType } from '@/lib/schemas';

/** Valid status transitions per current status (state machine). */
export const STATUS_TRANSITIONS: Record<PropertyStatusType, PropertyStatusType[]> = {
  Draft: ['Active', 'Archived'],
  Active: ['Reserved', 'Sold', 'Rented', 'Archived'],
  Reserved: ['Active', 'Sold', 'Rented', 'Archived'],
  Sold: [],
  Rented: [],
  Archived: ['Draft'],
};

/** Button label key in properties.actions + Tailwind class per target status. */
const TRANSITION_CONFIG: Record<PropertyStatusType, { labelKey: string; className: string }> = {
  Active: {
    labelKey: 'activate',
    className:
      'rounded-lg bg-green-600 px-3 py-1.5 text-sm font-medium text-white shadow-sm transition hover:bg-green-700 active:scale-[0.98] disabled:opacity-50',
  },
  Reserved: {
    labelKey: 'reserve',
    className:
      'rounded-lg bg-amber-600 px-3 py-1.5 text-sm font-medium text-white shadow-sm transition hover:bg-amber-700 active:scale-[0.98] disabled:opacity-50',
  },
  Sold: {
    labelKey: 'markSold',
    className:
      'rounded-lg bg-blue-600 px-3 py-1.5 text-sm font-medium text-white shadow-sm transition hover:bg-blue-700 active:scale-[0.98] disabled:opacity-50',
  },
  Rented: {
    labelKey: 'markRented',
    className:
      'rounded-lg bg-purple-600 px-3 py-1.5 text-sm font-medium text-white shadow-sm transition hover:bg-purple-700 active:scale-[0.98] disabled:opacity-50',
  },
  Archived: {
    labelKey: 'archive',
    className:
      'rounded-lg bg-red-600 px-3 py-1.5 text-sm font-medium text-white shadow-sm transition hover:bg-red-700 active:scale-[0.98] disabled:opacity-50',
  },
  Draft: {
    labelKey: 'activate',
    className:
      'rounded-lg bg-slate-600 px-3 py-1.5 text-sm font-medium text-white shadow-sm transition hover:bg-slate-700 active:scale-[0.98] disabled:opacity-50',
  },
};

interface PropertyStatusActionsProps {
  property: Property;
  onStatusChange: (newStatus: PropertyStatusType) => void;
}

export function PropertyStatusActions({ property, onStatusChange }: PropertyStatusActionsProps) {
  const t = useTranslations('properties.actions');
  const transitions = STATUS_TRANSITIONS[property.status] ?? [];

  if (transitions.length === 0) return null;

  return (
    <div className="flex flex-wrap items-center gap-2" data-testid="status-actions">
      {transitions.map((target) => {
        const config = TRANSITION_CONFIG[target];
        if (!config) return null;

        // When releasing a reservation (Reserved -> Active), use the release label
        const isRelease = property.status === 'Reserved' && target === 'Active';
        const label = isRelease ? t('release') : t(config.labelKey);

        return (
          <button
            key={target}
            type="button"
            onClick={() => onStatusChange(target)}
            className={config.className}
            data-testid={`status-action-${target}`}
          >
            {label}
          </button>
        );
      })}
    </div>
  );
}
