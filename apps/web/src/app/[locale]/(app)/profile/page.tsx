// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { ProfileForm } from '@/components/profile/ProfileForm';
import { ChangePasswordForm } from '@/components/profile/ChangePasswordForm';
import { DeleteAccountSection } from '@/components/profile/DeleteAccountSection';
import { useTranslations } from 'next-intl';

export default function ProfilePage() {
  const t = useTranslations('profile');

  return (
    <main className="flex-1 w-full max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">
      <div className="space-y-6">
        <div className="mb-8">
          <h1 className="text-3xl font-bold leading-tight tracking-tight text-slate-900">{t('pageTitle')}</h1>
          <p className="mt-2 text-slate-500">{t('pageDescription')}</p>
        </div>

        <ProfileForm />
        <ChangePasswordForm />
        <DeleteAccountSection />
      </div>
    </main>
  );
}
