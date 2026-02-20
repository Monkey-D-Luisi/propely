// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { Suspense } from 'react';
import { useLocale, useTranslations } from 'next-intl';
import { usePathname, useRouter } from '@/i18n/navigation';
import { useSearchParams } from 'next/navigation';
import type { Locale } from '@/i18n/routing';

const localeLabels: Record<Locale, string> = {
  en: 'EN',
  es: 'ES',
};

function LanguageSwitcherInner() {
  const locale = useLocale() as Locale;
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const t = useTranslations('common');

  const otherLocale: Locale = locale === 'en' ? 'es' : 'en';

  function handleSwitch() {
    const query = searchParams.toString();
    const href = query ? `${pathname}?${query}` : pathname;
    router.replace(href, { locale: otherLocale });
  }

  return (
    <button
      type="button"
      onClick={handleSwitch}
      className="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
      aria-label={t(locale === 'en' ? 'switchToEs' : 'switchToEn')}
    >
      <span aria-hidden="true">🌐</span>
      {localeLabels[otherLocale]}
    </button>
  );
}

export function LanguageSwitcher() {
  return (
    <Suspense>
      <LanguageSwitcherInner />
    </Suspense>
  );
}
