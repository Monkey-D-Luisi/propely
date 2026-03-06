// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';

const DiamondLogo = () => (
  <svg fill="none" viewBox="0 0 48 48" xmlns="http://www.w3.org/2000/svg" className="size-6">
    <g clipPath="url(#clip0_mkt)">
      <path clipRule="evenodd" d="M47.2426 24L24 47.2426L0.757355 24L24 0.757355L47.2426 24ZM12.2426 21H35.7574L24 9.24264L12.2426 21Z" fill="currentColor" fillRule="evenodd" />
    </g>
    <defs>
      <clipPath id="clip0_mkt"><rect fill="white" height="48" width="48" /></clipPath>
    </defs>
  </svg>
);

export function MarketingNav() {
  const t = useTranslations('common');

  return (
    <header className="sticky top-0 z-50 flex items-center justify-between whitespace-nowrap border-b border-slate-200 bg-white px-10 py-4 w-full">
      <Link href="/" className="flex items-center gap-4 text-primary-600">
        <DiamondLogo />
        <h2 className="text-slate-900 text-xl font-bold leading-tight tracking-[-0.015em]">{t('appName')}</h2>
      </Link>
      <div className="flex flex-1 justify-end gap-8">
        <nav className="flex items-center gap-9 hidden md:flex">
          <a href="#features" className="text-slate-600 hover:text-primary-600 text-sm font-medium leading-normal transition-colors">{t('features')}</a>
          <Link href="/pricing" className="text-slate-600 hover:text-primary-600 text-sm font-medium leading-normal transition-colors">{t('pricing')}</Link>
        </nav>
        <Link
          href="/register"
          className="flex min-w-[84px] max-w-[480px] cursor-pointer items-center justify-center overflow-hidden rounded-lg h-10 px-4 bg-primary-600 hover:bg-primary-600/90 text-white text-sm font-bold leading-normal tracking-[0.015em] transition-colors"
        >
          <span className="truncate">{t('getStarted')}</span>
        </Link>
      </div>
    </header>
  );
}
