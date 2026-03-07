// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { getTranslations } from 'next-intl/server';
import { Link } from '@/i18n/navigation';
import { TrustedBySection } from '@/components/common/TrustedBySection';

export default async function Page() {
  const t = await getTranslations('common');

  return (
    <div className="overflow-hidden pb-0">
      {/* Hero Section — Stitch landing.html */}
      <section className="w-full max-w-6xl mx-auto px-6 py-24 flex flex-col items-center text-center gap-8">
        {/* Abstract Background Decorations */}
        <div className="pointer-events-none absolute left-1/2 top-0 -z-10 h-full w-full -translate-x-1/2">
          <div className="absolute left-[20%] top-[-10%] h-[500px] w-[500px] rounded-full bg-primary-600/5 opacity-50 blur-3xl" />
          <div className="absolute right-[20%] top-[10%] h-[400px] w-[400px] rounded-full bg-purple-500/5 opacity-50 blur-3xl" />
        </div>

        {/* Pill Badge — Stitch landing.html */}
        <div className="mb-8 inline-flex items-center rounded-full border border-primary-600/30 bg-primary-600/10 px-3 py-1 text-sm font-medium text-primary-600">
          {t('landing.badge')}
        </div>

        {/* Main Heading — Stitch: text-5xl md:text-6xl font-black */}
        <h1 className="mx-auto mb-6 max-w-4xl text-5xl md:text-6xl font-black leading-tight tracking-tight text-slate-900">
          {t('landing.heroPrefix')}{' '}
          <span className="text-primary-600">
            {t('landing.heroHighlight')}
          </span>
        </h1>

        {/* Subtitle — Stitch: text-lg md:text-xl */}
        <p className="mx-auto mb-10 max-w-2xl text-lg md:text-xl text-slate-600">
          {t('appDescription')}
        </p>

        {/* CTA Buttons — Stitch: h-12 px-6 */}
        <div className="flex flex-wrap gap-4 justify-center mt-4">
          <Link
            href="/register"
            className="rounded-lg h-12 px-6 bg-primary-600 hover:bg-primary-600/90 text-white text-base font-bold shadow-lg shadow-primary-600/20 inline-flex items-center justify-center transition-colors"
          >
            {t('landing.getStarted')}
          </Link>
          <Link
            href="/pricing"
            className="rounded-lg h-12 px-6 bg-white border-2 border-slate-200 hover:border-primary-600 text-slate-900 text-base font-bold inline-flex items-center justify-center transition-colors"
          >
            {t('landing.viewDemo')}
          </Link>
        </div>

        {/* Features Grid — Stitch: 6 feature cards in 3-col grid */}
        <div className="mt-12 grid w-full grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 text-left">
          {/* Feature 1 — AI Action Engine */}
          <div className="flex flex-col gap-4 p-6 rounded-xl bg-white border border-slate-200 hover:shadow-lg transition-shadow">
            <div className="size-12 rounded-lg bg-primary-600/10 flex items-center justify-center text-primary-600">
              <span className="material-symbols-outlined" aria-hidden="true">smart_toy</span>
            </div>
            <h3 className="text-xl font-bold text-slate-900">{t('landing.feature1Title')}</h3>
            <p className="text-slate-600">{t('landing.feature1Description')}</p>
          </div>

          {/* Feature 2 — Property Management */}
          <div className="flex flex-col gap-4 p-6 rounded-xl bg-white border border-slate-200 hover:shadow-lg transition-shadow">
            <div className="size-12 rounded-lg bg-emerald-500/10 flex items-center justify-center text-emerald-500">
              <span className="material-symbols-outlined" aria-hidden="true">domain</span>
            </div>
            <h3 className="text-xl font-bold text-slate-900">{t('landing.feature2Title')}</h3>
            <p className="text-slate-600">{t('landing.feature2Description')}</p>
          </div>

          {/* Feature 3 — Lead Tracking */}
          <div className="flex flex-col gap-4 p-6 rounded-xl bg-white border border-slate-200 hover:shadow-lg transition-shadow">
            <div className="size-12 rounded-lg bg-amber-500/10 flex items-center justify-center text-amber-500">
              <span className="material-symbols-outlined" aria-hidden="true">track_changes</span>
            </div>
            <h3 className="text-xl font-bold text-slate-900">{t('landing.feature3Title')}</h3>
            <p className="text-slate-600">{t('landing.feature3Description')}</p>
          </div>

          {/* Feature 4 — Appointments */}
          <div className="flex flex-col gap-4 p-6 rounded-xl bg-white border border-slate-200 hover:shadow-lg transition-shadow">
            <div className="size-12 rounded-lg bg-rose-500/10 flex items-center justify-center text-rose-500">
              <span className="material-symbols-outlined" aria-hidden="true">calendar_month</span>
            </div>
            <h3 className="text-xl font-bold text-slate-900">{t('landing.feature4Title')}</h3>
            <p className="text-slate-600">{t('landing.feature4Description')}</p>
          </div>

          {/* Feature 5 — Team Collaboration */}
          <div className="flex flex-col gap-4 p-6 rounded-xl bg-white border border-slate-200 hover:shadow-lg transition-shadow">
            <div className="size-12 rounded-lg bg-indigo-500/10 flex items-center justify-center text-indigo-500">
              <span className="material-symbols-outlined" aria-hidden="true">groups</span>
            </div>
            <h3 className="text-xl font-bold text-slate-900">{t('landing.feature5Title')}</h3>
            <p className="text-slate-600">{t('landing.feature5Description')}</p>
          </div>

          {/* Feature 6 — Work Items */}
          <div className="flex flex-col gap-4 p-6 rounded-xl bg-white border border-slate-200 hover:shadow-lg transition-shadow">
            <div className="size-12 rounded-lg bg-cyan-500/10 flex items-center justify-center text-cyan-500">
              <span className="material-symbols-outlined" aria-hidden="true">work</span>
            </div>
            <h3 className="text-xl font-bold text-slate-900">{t('landing.feature6Title')}</h3>
            <p className="text-slate-600">{t('landing.feature6Description')}</p>
          </div>
        </div>

        {/* Dashboard Preview */}
        <div className="mt-20 w-full max-w-5xl overflow-hidden rounded-xl border border-slate-200 bg-white shadow-2xl">
          <div className="flex h-8 items-center space-x-2 border-b border-slate-200 bg-slate-100 px-4">
            <div className="h-3 w-3 rounded-full bg-red-400" />
            <div className="h-3 w-3 rounded-full bg-amber-400" />
            <div className="h-3 w-3 rounded-full bg-green-400" />
          </div>
          <div className="flex aspect-[16/9] w-full items-center justify-center bg-slate-50">
            {/* Placeholder dashboard illustration */}
            <svg className="h-full w-full" viewBox="0 0 960 540" fill="none" xmlns="http://www.w3.org/2000/svg">
              {/* Sidebar */}
              <rect x="0" y="0" width="200" height="540" fill="#f8fafc" />
              <rect x="0" y="0" width="200" height="540" stroke="#e2e8f0" strokeWidth="1" fill="none" />
              <rect x="24" y="24" width="32" height="32" rx="8" fill="#5048e5" />
              <rect x="64" y="30" width="80" height="16" rx="4" fill="#cbd5e1" />
              <rect x="24" y="80" width="152" height="12" rx="4" fill="#e2e8f0" />
              <rect x="24" y="104" width="152" height="12" rx="4" fill="#5048e5" opacity="0.15" />
              <rect x="24" y="128" width="152" height="12" rx="4" fill="#e2e8f0" />
              <rect x="24" y="152" width="152" height="12" rx="4" fill="#e2e8f0" />
              <rect x="24" y="176" width="152" height="12" rx="4" fill="#e2e8f0" />

              {/* Main area header */}
              <rect x="224" y="24" width="200" height="20" rx="4" fill="#0f172a" opacity="0.1" />
              <rect x="224" y="52" width="140" height="14" rx="4" fill="#cbd5e1" />

              {/* Stats cards */}
              <rect x="224" y="88" width="230" height="100" rx="12" fill="white" stroke="#e2e8f0" strokeWidth="1" />
              <rect x="248" y="108" width="80" height="12" rx="4" fill="#cbd5e1" />
              <rect x="248" y="132" width="120" height="24" rx="4" fill="#0f172a" opacity="0.1" />

              <rect x="470" y="88" width="230" height="100" rx="12" fill="white" stroke="#e2e8f0" strokeWidth="1" />
              <rect x="494" y="108" width="80" height="12" rx="4" fill="#cbd5e1" />
              <rect x="494" y="132" width="120" height="24" rx="4" fill="#0f172a" opacity="0.1" />

              <rect x="716" y="88" width="220" height="100" rx="12" fill="white" stroke="#e2e8f0" strokeWidth="1" />
              <rect x="740" y="108" width="80" height="12" rx="4" fill="#cbd5e1" />
              <rect x="740" y="132" width="120" height="24" rx="4" fill="#0f172a" opacity="0.1" />

              {/* Chart area */}
              <rect x="224" y="208" width="476" height="308" rx="12" fill="white" stroke="#e2e8f0" strokeWidth="1" />
              <rect x="248" y="228" width="100" height="14" rx="4" fill="#0f172a" opacity="0.1" />
              <polyline points="260,480 320,420 380,440 440,380 500,360 560,320 620,340 680,280" stroke="#5048e5" strokeWidth="2.5" fill="none" strokeLinecap="round" strokeLinejoin="round" />
              <polyline points="260,480 320,450 380,460 440,430 500,420 560,400 620,410 680,370" stroke="#a78bfa" strokeWidth="2" fill="none" strokeLinecap="round" strokeLinejoin="round" opacity="0.5" />

              {/* Table area */}
              <rect x="716" y="208" width="220" height="308" rx="12" fill="white" stroke="#e2e8f0" strokeWidth="1" />
              <rect x="740" y="228" width="80" height="14" rx="4" fill="#0f172a" opacity="0.1" />
              <rect x="740" y="260" width="176" height="10" rx="4" fill="#e2e8f0" />
              <rect x="740" y="284" width="176" height="10" rx="4" fill="#f1f5f9" />
              <rect x="740" y="308" width="176" height="10" rx="4" fill="#e2e8f0" />
              <rect x="740" y="332" width="176" height="10" rx="4" fill="#f1f5f9" />
              <rect x="740" y="356" width="176" height="10" rx="4" fill="#e2e8f0" />
              <rect x="740" y="380" width="176" height="10" rx="4" fill="#f1f5f9" />
              <rect x="740" y="404" width="176" height="10" rx="4" fill="#e2e8f0" />
            </svg>
          </div>
        </div>

        {/* Logo Cloud / Trusted By */}
        <div className="mt-24 w-full border-t border-slate-200 pt-16">
          <TrustedBySection heading={t('landing.trustedBy')} />
        </div>
      </section>
    </div>
  );
}
