// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useMemo, useState } from 'react';
import { useLocale, useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from '@/i18n/navigation';
import { AuthLayout, AuthBrand, AuthCard, AuthFooterLinks } from '@/components/auth/auth-layout';
import { ArrowLeftIcon, LockIcon, MailIcon } from '@/components/ui/icons';
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
        {/* Icon & heading */}
        <div className="mb-8 text-center">
          <div className="mx-auto mb-5 flex h-12 w-12 items-center justify-center rounded-lg border border-slate-100 bg-white shadow-sm">
            <LockIcon className="h-6 w-6 text-slate-400" />
          </div>
          <h1 className="mb-2 text-2xl font-semibold tracking-tight text-slate-900">{t('forgotPassword.title')}</h1>
          <p className="mx-auto max-w-[280px] text-sm leading-relaxed text-slate-500">{t('forgotPassword.description')}</p>
        </div>

        {success ? (
          <div className="text-center">
            <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-primary-50">
              <MailIcon className="h-8 w-8 text-primary-600" />
            </div>
            <p className="mt-4 text-sm font-medium text-slate-900">{t('forgotPassword.successTitle')}</p>
            <p className="mt-1 text-sm text-slate-500">{t('forgotPassword.successDescription')}</p>
            <div className="mt-6">
              <Link href="/login" className="inline-flex items-center gap-1.5 font-medium text-primary-600 hover:underline">
                <ArrowLeftIcon className="h-4 w-4" />
                {t('forgotPassword.backToLogin')}
              </Link>
            </div>
          </div>
        ) : (
          <>
            <FormProvider {...methods}>
              <form className="space-y-5" noValidate onSubmit={methods.handleSubmit(onSubmit)}>
                <FormField<ForgotPasswordFormData>
                  name="email"
                  label={t('forgotPassword.emailLabel')}
                  type="email"
                  autoComplete="email"
                  placeholder="name@company.com"
                  className="shadow-sm"
                />

                <FormError message={error} />

                <FormSubmitButton
                  disabled={!csrfToken}
                  loadingText={t('forgotPassword.submitting')}
                  className="w-full py-3 shadow-lg shadow-primary-600/25"
                >
                  {t('forgotPassword.submit')}
                </FormSubmitButton>
              </form>
            </FormProvider>

            <div className="mt-6 text-center">
              <Link href="/login" className="inline-flex items-center gap-1.5 font-medium text-primary-600 hover:underline">
                <ArrowLeftIcon className="h-4 w-4" />
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
