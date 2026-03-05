// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';

interface PermissionSourceBadgeProps {
  source: string;
}

export function PermissionSourceBadge({ source }: PermissionSourceBadgeProps) {
  const t = useTranslations('permissions');

  const isOverride = source === 'Override';
  const label = isOverride ? t('source.override') : t('source.roleDefault');
  const style = isOverride
    ? 'bg-amber-50 text-amber-700'
    : 'bg-slate-100 text-slate-600';

  return (
    <span
      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${style}`}
    >
      {label}
    </span>
  );
}
