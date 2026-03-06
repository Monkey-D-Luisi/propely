// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { usePathname } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';

interface OrgSubNavProps {
  orgId: string;
}

const tabs = [
  { key: 'members', segment: 'members' },
  { key: 'settings', segment: 'settings' },
  { key: 'billing', segment: 'billing' },
  { key: 'permissions', segment: 'permissions' },
] as const;

export function OrgSubNav({ orgId }: OrgSubNavProps) {
  const t = useTranslations('orgs');
  const pathname = usePathname();

  return (
    <nav className="bg-white border-b border-slate-200 shrink-0">
      <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex gap-8 overflow-x-auto">
          {tabs.map(({ key, segment }) => {
            const href = `/orgs/${orgId}/${segment}`;
            const isActive = pathname.includes(`/${segment}`);

            return (
              <Link
                key={key}
                href={href}
                className={`whitespace-nowrap border-b-[3px] pb-[13px] pt-4 text-sm font-bold leading-normal tracking-[0.015em] transition-colors ${
                  isActive
                    ? 'border-b-primary-600 text-primary-600'
                    : 'border-b-transparent text-slate-500 hover:border-b-slate-300 hover:text-slate-800'
                }`}
              >
                {t(`subNav.${key}`)}
              </Link>
            );
          })}
        </div>
      </div>
    </nav>
  );
}
