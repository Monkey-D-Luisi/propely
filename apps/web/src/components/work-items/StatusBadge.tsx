// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { WorkItemStatusType } from '@/lib/schemas';

interface StatusBadgeProps {
  status: WorkItemStatusType;
}

const statusStyles: Record<string, string> = {
  Pending: 'bg-amber-50 text-amber-700 border-amber-200',
  Active: 'bg-emerald-50 text-emerald-700 border-emerald-200',
  Deactivated: 'bg-slate-50 text-slate-600 border-slate-200',
  Expired: 'bg-red-50 text-red-700 border-red-200',
};

export function StatusBadge({ status }: StatusBadgeProps) {
  const t = useTranslations('workItems.status');
  const style = statusStyles[status] ?? statusStyles.Pending;
  const label = t.has(status) ? t(status) : status;
  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${style}`}
    >
      {label}
    </span>
  );
}
