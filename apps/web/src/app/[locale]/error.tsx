// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { AlertTriangleIcon } from '@/components/ui/icons';

interface ErrorPageProps {
  error: Error & { digest?: string };
  reset: () => void;
}

export default function ErrorPage({ error: _error, reset }: ErrorPageProps) {
  const t = useTranslations('errors.runtime');

  return (
    <div className="mx-auto flex min-h-[60vh] w-full max-w-md flex-col items-center justify-center px-6 py-16 text-center">
      <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-red-100">
        <AlertTriangleIcon className="h-7 w-7 text-red-600" />
      </div>
      <h1 className="mt-6 text-2xl font-bold text-slate-900">{t('title')}</h1>
      <p className="mt-2 text-sm text-slate-500">{t('description')}</p>
      <div className="mt-8 flex gap-3">
        <button
          type="button"
          onClick={reset}
          className="inline-flex h-10 items-center rounded-lg bg-primary-600 px-4 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 active:scale-[0.98]"
        >
          {t('tryAgain')}
        </button>
        <Link
          href="/"
          className="inline-flex h-10 items-center rounded-lg border border-slate-200 bg-white px-4 text-sm font-medium text-slate-700 transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 active:scale-[0.98]"
        >
          {t('goHome')}
        </Link>
      </div>
    </div>
  );
}
