// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';

export default function NotFound() {
  const t = useTranslations('errors.notFound');

  return (
    <div className="flex-1 flex flex-col items-center justify-center px-4 py-12 relative z-10 w-full max-w-[960px] mx-auto text-center">
      {/* Decorative background glow */}
      <div className="fixed inset-0 pointer-events-none z-0 flex items-center justify-center">
        <div className="w-[300px] h-[300px] md:w-[500px] md:h-[500px] bg-primary-600/10 rounded-full blur-[100px] md:blur-[150px]" />
      </div>

      {/* Illustration area */}
      <div className="relative w-full flex flex-col items-center justify-center mb-8">
        <p className="text-[140px] md:text-[240px] font-black text-slate-200 leading-none tracking-tighter select-none relative z-10" aria-hidden="true">
          404
        </p>
      </div>

      <h1 className="text-[28px] md:text-[36px] font-bold tracking-tight text-slate-900 mb-4 px-4">
        {t('title')}
      </h1>
      <p className="text-slate-500 text-base md:text-lg max-w-[480px] mx-auto mb-10 px-4 leading-relaxed">
        {t('description')}
      </p>

      {/* Buttons */}
      <div className="flex flex-col sm:flex-row gap-4 w-full max-w-[480px] px-4 justify-center">
        <Link
          href="/"
          className="flex-1 min-w-[140px] flex items-center justify-center gap-2 h-12 px-6 rounded-xl bg-primary-600 text-white font-semibold text-sm hover:bg-primary-600/90 focus:ring-4 focus:ring-primary-600/20 outline-none transition"
        >
          <span className="material-symbols-outlined text-[20px]" aria-hidden="true">home</span>
          {t('goHome')}
        </Link>
        <button
          type="button"
          onClick={() => history.back()}
          className="flex-1 min-w-[140px] flex items-center justify-center gap-2 h-12 px-6 rounded-xl bg-white text-slate-900 border border-slate-200 font-semibold text-sm hover:bg-slate-50 focus:ring-4 focus:ring-slate-200 outline-none transition"
        >
          <span className="material-symbols-outlined text-[20px]" aria-hidden="true">arrow_back</span>
          {t('goBack')}
        </button>
      </div>
    </div>
  );
}
