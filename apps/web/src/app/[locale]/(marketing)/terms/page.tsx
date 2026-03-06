// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';

export default function TermsPage() {
  const t = useTranslations('legal');

  const acceptableUseItems = t('terms.acceptableUseItems').split(';');

  return (
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
  );
}
