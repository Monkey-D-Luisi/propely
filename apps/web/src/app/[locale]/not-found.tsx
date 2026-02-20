// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { ArrowLeftIcon } from '@/components/ui/icons';

export default function NotFound() {
  const t = useTranslations('errors.notFound');

  return (
    <div className="mx-auto flex min-h-[60vh] w-full max-w-md flex-col items-center justify-center px-6 py-16 text-center">
      <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-slate-100">
        <span className="text-2xl font-bold text-slate-400" aria-hidden="true">404</span>
      </div>
      <h1 className="mt-6 text-2xl font-bold text-slate-900">{t('title')}</h1>
      <p className="mt-2 text-sm text-slate-500">{t('description')}</p>
      <Link
        href="/"
        className="mt-8 inline-flex h-10 items-center gap-2 rounded-lg bg-primary-600 px-5 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 active:scale-[0.98]"
      >
        <ArrowLeftIcon className="h-4 w-4" />
        {t('goHome')}
      </Link>
    </div>
  );
}
