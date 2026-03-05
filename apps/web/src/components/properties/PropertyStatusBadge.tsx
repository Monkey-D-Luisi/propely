// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import type { PropertyStatusType } from '@/lib/schemas';

const statusConfig: Record<PropertyStatusType, { bg: string; text: string }> = {
  Draft: { bg: 'bg-slate-100', text: 'text-slate-600' },
  Active: { bg: 'bg-green-100', text: 'text-green-700' },
  Reserved: { bg: 'bg-amber-100', text: 'text-amber-700' },
  Sold: { bg: 'bg-blue-100', text: 'text-blue-700' },
  Rented: { bg: 'bg-purple-100', text: 'text-purple-700' },
  Archived: { bg: 'bg-red-100', text: 'text-red-700' },
};

interface PropertyStatusBadgeProps {
  status: PropertyStatusType;
  label: string;
}

export function PropertyStatusBadge({ status, label }: PropertyStatusBadgeProps) {
  const config = statusConfig[status] ?? statusConfig.Draft;
  return (
    <span
      data-testid={`status-badge-${status}`}
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${config.bg} ${config.text}`}
    >
      {label}
    </span>
  );
}
