// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import type { ContactRole } from '@/hooks/useContacts';

const roleConfig: Record<ContactRole, { bg: string; text: string }> = {
  Buyer: { bg: 'bg-blue-100', text: 'text-blue-700' },
  Seller: { bg: 'bg-green-100', text: 'text-green-700' },
  Tenant: { bg: 'bg-purple-100', text: 'text-purple-700' },
  Landlord: { bg: 'bg-amber-100', text: 'text-amber-700' },
  Professional: { bg: 'bg-slate-100', text: 'text-slate-600' },
};

interface ContactRoleBadgeProps {
  role: ContactRole;
  label: string;
}

export function ContactRoleBadge({ role, label }: ContactRoleBadgeProps) {
  const config = roleConfig[role] ?? roleConfig.Buyer;
  return (
    <span
      data-testid={`role-badge-${role}`}
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${config.bg} ${config.text}`}
    >
      {label}
    </span>
  );
}
