// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';

export function MarketingFooter() {
  const t = useTranslations('common');

  return (
    <footer className="bg-slate-900 text-slate-400 py-16 px-6">
      <div className="max-w-6xl mx-auto grid grid-cols-2 md:grid-cols-4 gap-12">
        {/* Column 1 — Brand */}
        <div className="col-span-2 md:col-span-1 flex flex-col gap-4">
          <div className="flex items-center gap-2 text-white">
            <svg fill="none" viewBox="0 0 48 48" xmlns="http://www.w3.org/2000/svg" className="size-5">
              <g clipPath="url(#clip0_ft)">
                <path clipRule="evenodd" d="M47.2426 24L24 47.2426L0.757355 24L24 0.757355L47.2426 24ZM12.2426 21H35.7574L24 9.24264L12.2426 21Z" fill="currentColor" fillRule="evenodd" />
              </g>
              <defs>
                <clipPath id="clip0_ft"><rect fill="white" height="48" width="48" /></clipPath>
              </defs>
            </svg>
            <span className="text-lg font-bold">{t('appName')}</span>
          </div>
          <p className="text-sm leading-relaxed">
            {t('footerTagline')}
          </p>
        </div>

        {/* Column 2 — Product */}
        <div className="flex flex-col gap-4">
          <h4 className="text-white font-semibold">{t('product')}</h4>
          <a href="#features" className="text-sm hover:text-white transition-colors">{t('features')}</a>
          <Link href="/pricing" className="text-sm hover:text-white transition-colors">{t('pricing')}</Link>
        </div>

        {/* Column 3 — Company */}
        <div className="flex flex-col gap-4">
          <h4 className="text-white font-semibold">{t('company')}</h4>
          <a href="#" className="text-sm hover:text-white transition-colors">{t('aboutUs')}</a>
          <a href="#" className="text-sm hover:text-white transition-colors">{t('contact')}</a>
        </div>

        {/* Column 4 — Legal */}
        <div className="flex flex-col gap-4">
          <h4 className="text-white font-semibold">{t('legal')}</h4>
          <Link href="/privacy" className="text-sm hover:text-white transition-colors">{t('privacyPolicy')}</Link>
          <Link href="/terms" className="text-sm hover:text-white transition-colors">{t('termsOfService')}</Link>
        </div>
      </div>

      <div className="max-w-6xl mx-auto mt-16 pt-8 border-t border-slate-800 flex flex-col md:flex-row items-center justify-between gap-4">
        <p className="text-sm text-slate-500">{t('copyright')}</p>
      </div>
    </footer>
  );
}
