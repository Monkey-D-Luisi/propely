// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useRef, useState, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useAgencies } from '@/hooks/agencies';

export function BranchSwitcher() {
  const t = useTranslations('agencies');
  const { agencies, isLoading } = useAgencies();
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!isOpen) return;

    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    document.addEventListener('keydown', handleEscape);

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      document.removeEventListener('keydown', handleEscape);
    };
  }, [isOpen]);

  if (isLoading || agencies.length === 0) {
    return null;
  }

  return (
    <div ref={dropdownRef} className="relative">
      <button
        type="button"
        onClick={() => setIsOpen((prev) => !prev)}
        aria-expanded={isOpen}
        aria-haspopup="true"
        aria-label={t('switcher.label')}
        className="hidden items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm font-medium text-slate-600 transition hover:bg-slate-100 hover:text-slate-900 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 sm:inline-flex"
      >
        <span className="material-symbols-outlined text-base" aria-hidden="true">business</span>
        {t('switcher.agencies')}
        <span className="material-symbols-outlined text-xs" aria-hidden="true">
          {isOpen ? 'expand_less' : 'expand_more'}
        </span>
      </button>

      {isOpen && (
        <div className="absolute right-0 z-50 mt-1 w-64 rounded-xl border border-slate-200 bg-white shadow-lg">
          <div className="p-2" role="menu">
            {agencies.map((agency) => (
              <div key={agency.id} className="mb-1 last:mb-0">
                <Link
                  href={`/agencies/${agency.id}`}
                  role="menuitem"
                  onClick={() => setIsOpen(false)}
                  className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition hover:bg-slate-50"
                >
                  <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary-600/10 text-primary-600">
                    <span className="material-symbols-outlined text-sm" aria-hidden="true">business</span>
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium text-slate-900">{agency.name}</p>
                    <p className="truncate text-xs text-slate-500">
                      {t('switcher.branchCount', { count: agency.branchCount })}
                    </p>
                  </div>
                </Link>
              </div>
            ))}
          </div>
          <div className="border-t border-slate-100 p-2">
            <Link
              href="/agencies/new"
              role="menuitem"
              onClick={() => setIsOpen(false)}
              className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium text-primary-600 transition hover:bg-primary-600/5"
            >
              <span className="material-symbols-outlined text-sm" aria-hidden="true">add</span>
              {t('switcher.createAgency')}
            </Link>
          </div>
        </div>
      )}
    </div>
  );
}
