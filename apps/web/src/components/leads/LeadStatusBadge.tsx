// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import type { LeadStatus } from '@/hooks/useLeads';

const statusConfig: Record<LeadStatus, { bg: string; text: string }> = {
  New: { bg: 'bg-blue-100', text: 'text-blue-700' },
  Contacted: { bg: 'bg-amber-100', text: 'text-amber-700' },
  Qualified: { bg: 'bg-green-100', text: 'text-green-700' },
  Converted: { bg: 'bg-purple-100', text: 'text-purple-700' },
  Lost: { bg: 'bg-red-100', text: 'text-red-700' },
};

interface LeadStatusBadgeProps {
  status: LeadStatus;
  label: string;
}

export function LeadStatusBadge({ status, label }: LeadStatusBadgeProps) {
  const config = statusConfig[status] ?? statusConfig.New;
  return (
    <span
      data-testid={`lead-status-badge-${status}`}
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${config.bg} ${config.text}`}
    >
      {label}
    </span>
  );
}
