// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import type { ReactNode } from 'react';
import { Link } from '@/i18n/navigation';

type Breadcrumb = {
  label: string;
  href?: string;
};

type DashboardHeaderProps = {
  breadcrumbs?: Breadcrumb[];
  title: string;
  count?: number;
  actions?: ReactNode;
};

export function DashboardHeader({ breadcrumbs, title, count, actions }: DashboardHeaderProps) {
  return (
    <header className="bg-white border-b border-slate-200 px-8 py-4 flex-shrink-0">
      {/* Breadcrumbs */}
      {breadcrumbs && breadcrumbs.length > 0 && (
        <div className="flex items-center gap-2 text-sm text-slate-500 mb-4">
          {breadcrumbs.map((crumb, i) => (
            <span key={i} className="flex items-center gap-2">
              {i > 0 && (
                <span className="material-symbols-outlined text-[16px]">chevron_right</span>
              )}
              {crumb.href ? (
                <Link href={crumb.href} className="hover:text-primary-600 transition-colors font-medium">
                  {crumb.label}
                </Link>
              ) : (
                <span className="text-slate-900 font-medium">{crumb.label}</span>
              )}
            </span>
          ))}
        </div>
      )}

      {/* Title & Actions */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div className="flex items-center gap-4">
          <h2 className="text-2xl font-bold">{title}</h2>
          {count !== undefined && (
            <span className="px-2.5 py-1 rounded-full bg-slate-100 text-xs font-medium text-slate-600 border border-slate-200">
              {count} total
            </span>
          )}
        </div>
        {actions}
      </div>
    </header>
  );
}
