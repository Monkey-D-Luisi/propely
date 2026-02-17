// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { ProfileForm } from '@/components/profile/ProfileForm';
import { ChangePasswordForm } from '@/components/profile/ChangePasswordForm';
import { DeleteAccountSection } from '@/components/profile/DeleteAccountSection';
import { Link } from '@/i18n/navigation';
import { useTranslations } from 'next-intl';

export default function ProfilePage() {
  const t = useTranslations('profile');

  return (
    <div className="flex-grow px-4 py-10 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-5xl space-y-8">
        <div className="flex items-center">
          <Link
            href="/orgs/mine"
            className="group flex items-center text-sm font-medium text-slate-500 transition-colors hover:text-primary-600"
          >
            <span className="material-symbols-outlined mr-1 text-base transition-transform group-hover:-translate-x-1" aria-hidden="true">arrow_back</span>
            {t('backToOrgs')}
          </Link>
        </div>

        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('pageTitle')}</h1>
          <p className="mt-2 text-sm text-slate-500">{t('pageDescription')}</p>
        </div>

        <ProfileForm />
        <ChangePasswordForm />
        <DeleteAccountSection />
      </div>
    </div>
  );
}
