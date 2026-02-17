// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { brand } from '@/config/brand';

export default function PrivacyPage() {
  const t = useTranslations('legal');

  const howWeUseItems = t('privacy.howWeUseItems').split(';');

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
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('privacy.title')}</h1>
          <p className="mt-2 text-sm text-slate-500">{t('lastUpdated', { date: t('privacy.lastUpdatedDate') })}</p>

          <div className="mt-8 space-y-8">
            {/* Introduction */}
            <p className="text-base leading-relaxed text-slate-600">{t('privacy.intro')}</p>

            {/* Information We Collect */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('privacy.infoCollectTitle')}</h2>
              <div className="mt-4 space-y-4">
                <div>
                  <h3 className="text-base font-medium text-slate-800">{t('privacy.accountInfoTitle')}</h3>
                  <p className="mt-1 text-base leading-relaxed text-slate-600">{t('privacy.accountInfo')}</p>
                </div>
                <div>
                  <h3 className="text-base font-medium text-slate-800">{t('privacy.usageDataTitle')}</h3>
                  <p className="mt-1 text-base leading-relaxed text-slate-600">{t('privacy.usageData')}</p>
                </div>
                <div>
                  <h3 className="text-base font-medium text-slate-800">{t('privacy.paymentDataTitle')}</h3>
                  <p className="mt-1 text-base leading-relaxed text-slate-600">{t('privacy.paymentData')}</p>
                </div>
                <div>
                  <h3 className="text-base font-medium text-slate-800">{t('privacy.aiServiceDataTitle')}</h3>
                  <p className="mt-1 text-base leading-relaxed text-slate-600">{t('privacy.aiServiceData')}</p>
                </div>
              </div>
            </section>

            {/* How We Use Your Information */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('privacy.howWeUseTitle')}</h2>
              <ul className="mt-4 list-disc space-y-2 pl-6 text-base leading-relaxed text-slate-600">
                {howWeUseItems.map((item, i) => (
                  <li key={i}>{item}</li>
                ))}
              </ul>
            </section>

            {/* Third-Party Services */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('privacy.thirdPartyTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('privacy.thirdPartyIntro')}</p>
              <div className="mt-4 space-y-4">
                <div>
                  <h3 className="text-base font-medium text-slate-800">{t('privacy.stripeTitle')}</h3>
                  <p className="mt-1 text-base leading-relaxed text-slate-600">{t('privacy.stripe')}</p>
                </div>
                <div>
                  <h3 className="text-base font-medium text-slate-800">{t('privacy.sendgridTitle')}</h3>
                  <p className="mt-1 text-base leading-relaxed text-slate-600">{t('privacy.sendgrid')}</p>
                </div>
                <div>
                  <h3 className="text-base font-medium text-slate-800">{t('privacy.googleOAuthTitle')}</h3>
                  <p className="mt-1 text-base leading-relaxed text-slate-600">{t('privacy.googleOAuth')}</p>
                </div>
                <div>
                  <h3 className="text-base font-medium text-slate-800">{t('privacy.githubOAuthTitle')}</h3>
                  <p className="mt-1 text-base leading-relaxed text-slate-600">{t('privacy.githubOAuth')}</p>
                </div>
                <div>
                  <h3 className="text-base font-medium text-slate-800">{t('privacy.openaiTitle')}</h3>
                  <p className="mt-1 text-base leading-relaxed text-slate-600">{t('privacy.openai')}</p>
                </div>
              </div>
            </section>

            {/* Data Storage and Security */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('privacy.storageTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('privacy.storage')}</p>
            </section>

            {/* GDPR Rights */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('privacy.gdprTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('privacy.gdprIntro')}</p>
              <ul className="mt-4 list-disc space-y-2 pl-6 text-base leading-relaxed text-slate-600">
                <li><strong>{t('privacy.gdprAccessTitle')}</strong> — {t('privacy.gdprAccessDesc')}</li>
                <li><strong>{t('privacy.gdprRectificationTitle')}</strong> — {t('privacy.gdprRectificationDesc')}</li>
                <li><strong>{t('privacy.gdprErasureTitle')}</strong> — {t('privacy.gdprErasureDesc')}</li>
                <li><strong>{t('privacy.gdprPortabilityTitle')}</strong> — {t('privacy.gdprPortabilityDesc')}</li>
                <li><strong>{t('privacy.gdprRestrictionTitle')}</strong> — {t('privacy.gdprRestrictionDesc')}</li>
                <li><strong>{t('privacy.gdprObjectionTitle')}</strong> — {t('privacy.gdprObjectionDesc')}</li>
              </ul>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('privacy.gdprExercise')}</p>
            </section>

            {/* Cookies */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('privacy.cookiesTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('privacy.cookies')}</p>
            </section>

            {/* Data Retention */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('privacy.retentionTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('privacy.retention')}</p>
            </section>

            {/* Children's Privacy */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('privacy.childrenTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('privacy.children')}</p>
            </section>

            {/* Changes to This Policy */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('privacy.changesTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('privacy.changes')}</p>
            </section>

            {/* Contact Us */}
            <section>
              <h2 className="text-xl font-semibold text-slate-900">{t('privacy.contactTitle')}</h2>
              <p className="mt-4 text-base leading-relaxed text-slate-600">{t('privacy.contact')}</p>
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
            <span className="font-medium text-slate-900">{t('privacy.title')}</span>
            <Link href="/terms" className="transition-colors hover:text-primary-600">{t('terms.title')}</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
