// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useMemo, useState } from 'react';
import { useLocale, useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from '@/i18n/navigation';
import { AuthLayout, AuthBrand, AuthCard, AuthFooterLinks } from '@/components/auth/auth-layout';
import { FormField, FormSubmitButton, FormError } from '@/components/ui/form';
import { apiFetch, isApiError } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { createForgotPasswordFormSchema, type ForgotPasswordFormData } from '@/lib/schemas';

export function ForgotPasswordForm() {
  const t = useTranslations('auth');
  const locale = useLocale();
  const [csrfToken, setCsrfToken] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const schema = useMemo(
    () =>
      createForgotPasswordFormSchema({
        emailRequired: t('validation.emailRequired'),
        emailInvalid: t('validation.emailInvalid'),
      }),
    [t]
  );

  const methods = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(schema),
    defaultValues: { email: '' },
  });

  useEffect(() => {
    let active = true;
    void (async () => {
      const token = await ensureCsrfToken();
      if (!active) return;
      if (token) {
        setCsrfToken(token);
      } else {
        setError(t('errors.csrfError'));
      }
    })();

    return () => {
      active = false;
    };
  }, [t]);

  const onSubmit = async (data: ForgotPasswordFormData) => {
    if (!csrfToken) {
      setError(t('errors.csrfError'));
      return;
    }

    setError(null);

    try {
      await apiFetch('/auth/forgot-password', {
        method: 'POST',
        body: JSON.stringify({ email: data.email, locale }),
        headers: {
          'x-csrf-token': csrfToken,
        },
      });

      setSuccess(true);
      methods.reset();
    } catch (err) {
      if (isApiError(err) && err.status === 403) {
        setError(t('errors.csrfError'));
      } else if (isApiError(err) && err.status === 429) {
        setError(t('errors.rateLimited', { seconds: err.retryAfterSeconds ?? 60 }));
      } else {
        setError(t('errors.genericError'));
      }
    }
  };

  return (
    <AuthLayout>
      <AuthBrand />

      <AuthCard>
        {/* Status icon & heading — Stitch forgot-password.html */}
        <div className="flex flex-col items-center text-center">
          <div className="w-16 h-16 bg-primary-600/10 rounded-full flex items-center justify-center mb-6">
            <span className="material-symbols-outlined text-3xl text-primary-600">key</span>
          </div>
          <h1 className="text-2xl font-bold text-slate-900 mb-2">{t('forgotPassword.title')}</h1>
          <p className="text-slate-500 text-sm mb-8">{t('forgotPassword.description')}</p>
        </div>

        {success ? (
          <div className="flex flex-col items-center text-center">
            <div className="w-16 h-16 bg-green-50 rounded-full flex items-center justify-center mb-6">
              <span className="material-symbols-outlined text-green-500 text-3xl">check_circle</span>
            </div>
            <h2 className="text-2xl font-bold tracking-tight mb-3">{t('forgotPassword.successTitle')}</h2>
            <p className="text-slate-500 mb-8">{t('forgotPassword.successDescription')}</p>

            <div className="w-full border-t border-slate-100 pt-6 mt-2">
              <Link href="/login" className="flex items-center justify-center gap-2 text-slate-600 font-medium hover:text-slate-900 transition-colors">
                <span className="material-symbols-outlined text-[20px]">arrow_back</span>
                {t('forgotPassword.backToLogin')}
              </Link>
            </div>
          </div>
        ) : (
          <>
            <FormProvider {...methods}>
              <form className="flex flex-col gap-4" noValidate onSubmit={methods.handleSubmit(onSubmit)}>
                <FormField<ForgotPasswordFormData>
                  name="email"
                  label={t('forgotPassword.emailLabel')}
                  type="email"
                  autoComplete="email"
                  placeholder="name@company.com"
                />

                <FormError message={error} />

                <FormSubmitButton
                  disabled={!csrfToken}
                  loadingText={t('forgotPassword.submitting')}
                  className="w-full bg-primary-600 hover:bg-primary-600/90 text-white font-medium py-3 rounded-lg transition-colors mt-2"
                >
                  {t('forgotPassword.submit')}
                </FormSubmitButton>
              </form>
            </FormProvider>

            {/* Divider */}
            <div className="flex items-center gap-4 my-2">
              <div className="h-px bg-slate-200 flex-1" />
            </div>

            <div className="flex items-center justify-center">
              <Link href="/login" className="flex items-center gap-2 text-sm font-medium text-slate-600 hover:text-primary-600 transition-colors">
                <span className="material-symbols-outlined text-[18px]">arrow_back</span>
                {t('forgotPassword.backToLogin')}
              </Link>
            </div>
          </>
        )}
      </AuthCard>

      <AuthFooterLinks />
    </AuthLayout>
  );
}
