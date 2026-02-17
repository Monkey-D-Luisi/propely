// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { brand } from '@/config/brand';

export default function TermsPage() {
  const t = useTranslations('legal');

  const acceptableUseItems = t('terms.acceptableUseItems').split(';');

  return (
    <div className="min-h-screen bg-surface">
      {/* Navigation */}
      <nav className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <Link href="/" className="flex items-center gap-2">
            <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary-600 text-white">
              <span className="material-symbols-outlined text-base" aria-hidden="true">layers</span>
            </div>
            <span className="text-lg font-bold tracking-tight text-slate-900">{brand.name}</span>
          </Link>
          <Link href="/" className="text-sm text-slate-500 transition-colors hover:text-primary-600">
            {t('backToHome')}
          </Link>
        </div>
      </nav>

      {/* Content */}
      <div className="mx-auto max-w-4xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="rounded-xl border border-slate-200 bg-white p-8 shadow-sm sm:p-12">
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('terms.title')}</h1>
          <p className="mt-2 text-sm text-slate-500">{t('lastUpdated', { date: t('terms.lastUpdatedDate') })}</p>

          <div className="mt-8 space-y-8">
            {/* Agreement to Terms */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.agreementTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.agreement')}</p>
            </section>

            {/* Description of Service */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.descriptionTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.description')}</p>
            </section>

            {/* Account Registration and Security */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.accountTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.account')}</p>
            </section>

            {/* Acceptable Use Policy */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.acceptableUseTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.acceptableUseIntro')}</p>
              <ul className="mt-4 list-disc space-y-2 pl-6 text-base leading-relaxed text-slate-600">
                {acceptableUseItems.map((item, i) => (
                  <li key={i}>{item}</li>
                ))}
              </ul>
            </section>

            {/* Subscription and Billing */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.billingTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.billing')}</p>
            </section>

            {/* Intellectual Property */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.ipTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.ip')}</p>
            </section>

            {/* User Content */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.userContentTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.userContent')}</p>
            </section>

            {/* Third-Party Services */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.thirdPartyTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.thirdParty')}</p>
            </section>

            {/* Disclaimer of Warranties */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.disclaimerTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.disclaimer')}</p>
            </section>

            {/* Limitation of Liability */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.liabilityTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.liability')}</p>
            </section>

            {/* Indemnification */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.indemnificationTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.indemnification')}</p>
            </section>

            {/* Termination */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.terminationTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.termination')}</p>
            </section>

            {/* Governing Law */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.governingLawTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.governingLaw')}</p>
            </section>

            {/* Changes to Terms */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.changesToTermsTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.changesToTerms')}</p>
            </section>

            {/* Contact Us */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('terms.contactTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('terms.contact')}</p>
              <p className="mt-2 text-base font-medium text-primary-700">{t('contactEmail')}</p>
            </section>
          </div>
        </div>
      </div>

      {/* Footer */}
      <footer className="border-t border-slate-200 bg-white py-8">
        <div className="mx-auto flex max-w-5xl flex-col items-center gap-4 px-4 sm:px-6 md:flex-row md:justify-between lg:px-8">
          <span className="text-sm text-slate-500">&copy; {new Date().getFullYear()} {brand.name}</span>
          <div className="flex space-x-6 text-sm text-slate-500">
            <Link href="/privacy" className="transition-colors hover:text-primary-600">{t('privacy.title')}</Link>
            <span className="font-medium text-slate-900">{t('terms.title')}</span>
          </div>
        </div>
      </footer>
    </div>
  );
}
