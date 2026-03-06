// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { Role } from '@/lib/schemas';

const styles: Record<Role, string> = {
  owner: 'bg-primary-600/10 text-primary-600 border border-primary-600/20',
  admin: 'bg-purple-100 text-purple-700 border border-purple-200',
  agent: 'bg-blue-100 text-blue-700 border border-blue-200',
  viewer: 'bg-slate-100 text-slate-700 border border-slate-200',
};

export function RoleBadge({ role }: { role: Role }) {
  const t = useTranslations('orgs');

  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${styles[role]}`}>
      {t(`roles.${role}`)}
    </span>
  );
}
