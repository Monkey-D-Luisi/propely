// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { Role } from '@/lib/schemas';

const styles: Record<Role, string> = {
  owner: 'bg-primary-600/10 text-primary-600',
  admin: 'bg-slate-100 text-slate-600',
  member: 'bg-slate-100 text-slate-600',
  viewer: 'bg-emerald-100 text-emerald-600'
};

export function RoleBadge({ role }: { role: Role }) {
  const t = useTranslations('orgs');

  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold uppercase tracking-wider ${styles[role]}`}>
      {t(`roles.${role}`)}
    </span>
  );
}
