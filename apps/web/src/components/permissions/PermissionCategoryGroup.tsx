// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { ReactNode } from 'react';

interface PermissionCategoryGroupProps {
  category: string;
  icon: ReactNode;
  children: ReactNode;
}

export function PermissionCategoryGroup({
  category,
  icon,
  children,
}: PermissionCategoryGroupProps) {
  const t = useTranslations('permissions');

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2">
        <span className="text-slate-400">{icon}</span>
        <h3 className="text-sm font-semibold uppercase tracking-wider text-slate-500">
          {t(`categories.${category}` as Parameters<typeof t>[0])}
        </h3>
      </div>
      <div className="divide-y divide-slate-100 rounded-xl border border-slate-200 bg-white shadow-sm">
        {children}
      </div>
    </div>
  );
}
