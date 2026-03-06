// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import type { PropertyInterest } from '@/hooks/useContacts';

interface PropertyInterestsListProps {
  interests: PropertyInterest[];
}

function formatDate(dateStr: string | null | undefined): string {
  if (!dateStr) return '\u2014';
  return new Intl.DateTimeFormat('es-ES', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(dateStr));
}

export function PropertyInterestsList({ interests }: PropertyInterestsListProps) {
  const t = useTranslations('contacts.detail');

  if (interests.length === 0) {
    return (
      <p className="text-sm text-slate-500" data-testid="no-interests">{t('noInterests')}</p>
    );
  }

  return (
    <div className="space-y-2" data-testid="property-interests">
      {interests.map((interest) => (
        <div
          key={interest.id}
          className="flex items-center justify-between rounded-lg border border-slate-200 bg-slate-50 px-4 py-3"
        >
          <div className="flex items-center gap-3">
            <span className="material-symbols-outlined text-lg text-slate-400" aria-hidden="true">home</span>
            <div>
              <Link
                href={`/properties/${interest.propertyId}`}
                className="text-sm font-medium text-primary-600 hover:text-primary-700"
              >
                {interest.propertyId.slice(0, 8)}...
              </Link>
              <p className="text-xs text-slate-500">
                {t(`interestType.${interest.interestType}`)}
                {interest.notes && ` \u2014 ${interest.notes}`}
              </p>
            </div>
          </div>
          <span className="text-xs text-slate-400">{formatDate(interest.createdAtUtc)}</span>
        </div>
      ))}
    </div>
  );
}
