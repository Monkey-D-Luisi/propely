// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { TrustedBySection } from '@/components/common/TrustedBySection';

export default function Page() {
  const t = useTranslations('common');

  return (
    <div className="overflow-hidden pb-0">
      {/* Hero Section */}
      <section className="relative flex flex-col items-center px-4 pb-16 pt-24 text-center sm:px-6 sm:pb-24 lg:px-8 lg:pb-32">
        {/* Abstract Background Decorations */}
        <div className="pointer-events-none absolute left-1/2 top-0 -z-10 h-full w-full -translate-x-1/2">
          <div className="absolute left-[20%] top-[-10%] h-[500px] w-[500px] rounded-full bg-primary-600/5 opacity-50 blur-3xl" />
          <div className="absolute right-[20%] top-[10%] h-[400px] w-[400px] rounded-full bg-purple-500/5 opacity-50 blur-3xl" />
        </div>

        {/* Pill Badge */}
        <div className="mb-8 inline-flex items-center gap-2 rounded-full border border-primary-600/20 bg-primary-600/10 px-3 py-1 text-sm font-medium text-primary-600">
          <span className="relative flex h-2 w-2">
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-primary-600 opacity-75" />
            <span className="relative inline-flex h-2 w-2 rounded-full bg-primary-600" />
          </span>
          {t('landing.badge')}
        </div>

        {/* Main Heading */}
        <h1 className="mx-auto mb-6 max-w-4xl text-5xl font-extrabold leading-[1.1] tracking-tight sm:text-6xl lg:text-7xl">
          {t('landing.heroPrefix')}{' '}
          <span className="bg-gradient-to-r from-primary-600 to-purple-600 bg-clip-text text-transparent">
            {t('landing.heroHighlight')}
          </span>
        </h1>

        {/* Subtitle */}
        <p className="mx-auto mb-10 max-w-2xl text-lg leading-relaxed text-slate-600 sm:text-xl">
          {t('appDescription')}
        </p>

        {/* CTA Buttons */}
        <div className="flex w-full flex-col items-center gap-4 sm:w-auto sm:flex-row">
          <Link
            href="/orgs/mine"
            className="inline-flex w-full items-center justify-center rounded-lg bg-primary-600 px-8 py-3.5 text-base font-semibold text-white shadow-sm shadow-primary-600/30 transition-all hover:-translate-y-0.5 hover:bg-primary-600/90 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 sm:w-auto"
          >
            {t('landing.getStarted')}
            <span className="material-symbols-outlined ml-2 text-[20px]" aria-hidden="true">arrow_forward</span>
          </Link>
          <a
            href="https://github.com"
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex w-full items-center justify-center rounded-lg border border-slate-200 bg-white px-8 py-3.5 text-base font-medium text-slate-700 shadow-sm transition-all hover:-translate-y-0.5 hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-slate-200 focus:ring-offset-2 sm:w-auto"
          >
            <svg className="mr-2 h-5 w-5 text-slate-500" fill="currentColor" viewBox="0 0 24 24">
              <path fillRule="evenodd" clipRule="evenodd" d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z" />
            </svg>
            {t('landing.viewOnGithub')}
          </a>
        </div>

        {/* Features Grid */}
        <div className="mt-20 grid w-full max-w-5xl grid-cols-1 gap-8 text-left md:grid-cols-3">
          {/* Feature 1 — Authentication */}
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm transition-shadow hover:shadow-md">
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-lg bg-blue-50 text-blue-600">
              <span className="material-symbols-outlined" aria-hidden="true">fingerprint</span>
            </div>
            <h3 className="mb-2 text-lg font-semibold text-slate-900">{t('landing.feature1Title')}</h3>
            <p className="text-sm leading-relaxed text-slate-600">
              {t('landing.feature1Description')}
            </p>
          </div>

          {/* Feature 2 — Billing & Subscriptions */}
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm transition-shadow hover:shadow-md">
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-lg bg-green-50 text-green-600">
              <span className="material-symbols-outlined" aria-hidden="true">credit_card</span>
            </div>
            <h3 className="mb-2 text-lg font-semibold text-slate-900">{t('landing.feature2Title')}</h3>
            <p className="text-sm leading-relaxed text-slate-600">
              {t('landing.feature2Description')}
            </p>
          </div>

          {/* Feature 3 — Multi-tenancy */}
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm transition-shadow hover:shadow-md">
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-lg bg-purple-50 text-purple-600">
              <span className="material-symbols-outlined" aria-hidden="true">apartment</span>
            </div>
            <h3 className="mb-2 text-lg font-semibold text-slate-900">{t('landing.feature3Title')}</h3>
            <p className="text-sm leading-relaxed text-slate-600">
              {t('landing.feature3Description')}
            </p>
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

      {/* Footer */}
      <footer className="border-t border-slate-200 bg-white py-12">
        <div className="mx-auto flex max-w-7xl flex-col items-center gap-4 px-4 sm:px-6 md:flex-row md:justify-between lg:px-8">
          <div className="flex items-center gap-2">
            <div className="flex h-6 w-6 items-center justify-center rounded bg-primary-600 text-white">
              <span className="material-symbols-outlined text-sm" aria-hidden="true">layers</span>
            </div>
            <span className="text-sm text-slate-500">{t('landing.copyright')}</span>
          </div>
          <div className="flex space-x-6 text-sm text-slate-500">
            <Link href="/privacy" className="transition-colors hover:text-primary-600">{t('landing.privacy')}</Link>
            <Link href="/terms" className="transition-colors hover:text-primary-600">{t('landing.terms')}</Link>
            <a href="https://github.com" target="_blank" rel="noopener noreferrer" className="transition-colors hover:text-primary-600">{t('landing.github')}</a>
            <a href="https://twitter.com" target="_blank" rel="noopener noreferrer" className="transition-colors hover:text-primary-600">{t('landing.twitter')}</a>
          </div>
        </div>
      </footer>
    </div>
  );
}
