// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';

interface ErrorPageProps {
  error: Error & { digest?: string };
  reset: () => void;
}

export default function ErrorPage({ error, reset }: ErrorPageProps) {
  const t = useTranslations('errors.runtime');

  return (
    <div className="flex-1 flex flex-col items-center justify-center p-6">
      <div className="max-w-[600px] w-full flex flex-col items-center bg-white p-10 rounded-xl shadow-sm border border-slate-200">
        {/* Warning icon */}
        <div className="w-20 h-20 bg-red-50 rounded-full flex items-center justify-center mb-6">
          <span className="material-symbols-outlined text-4xl text-red-500">warning</span>
        </div>

        {/* Text content */}
        <div className="flex flex-col items-center gap-3 mb-8 text-center">
          <h1 className="text-3xl font-bold leading-tight tracking-[-0.015em]">
            {t('title')}
          </h1>
          <p className="text-slate-600 text-base font-normal leading-relaxed max-w-[480px]">
            {t('description')}
          </p>
          {error.digest && (
            <div className="mt-2 inline-flex items-center gap-2 bg-slate-100 px-3 py-1.5 rounded text-sm font-mono text-slate-500 border border-slate-200">
              Error ID: <span className="font-bold text-slate-700">{error.digest}</span>
            </div>
          )}
        </div>

        {/* Buttons */}
        <div className="flex flex-col sm:flex-row w-full gap-4 justify-center mb-8">
          <button
            type="button"
            onClick={reset}
            className="flex min-w-[140px] items-center justify-center rounded-lg h-12 px-6 bg-primary-600 hover:bg-primary-600/90 text-white text-base font-medium transition-colors"
          >
            {t('tryAgain')}
          </button>
          <Link
            href="/"
            className="flex min-w-[140px] items-center justify-center rounded-lg h-12 px-6 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 text-base font-medium transition-colors"
          >
            {t('goHome')}
          </Link>
        </div>
      </div>
    </div>
  );
}
