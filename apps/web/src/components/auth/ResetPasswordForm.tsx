// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useLocale, useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from '@/i18n/navigation';
import { AuthLayout, AuthBrand, AuthFooterLinks } from '@/components/auth/auth-layout';
import { FormField, FormSubmitButton, FormError } from '@/components/ui/form';
import { apiFetch, isApiError } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { createResetPasswordFormSchema, type ResetPasswordFormData } from '@/lib/schemas';

export function ResetPasswordForm() {
  const t = useTranslations('auth');
  const router = useRouter();
  const searchParams = useSearchParams();
  const locale = useLocale();
  const [csrfToken, setCsrfToken] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const token = useMemo(() => searchParams.get('token') ?? '', [searchParams]);

  const schema = useMemo(
    () =>
      createResetPasswordFormSchema({
        newPasswordRequired: t('validation.passwordRequired'),
        newPasswordMinLength: t('validation.passwordMinLength', { min: 8 }),
        confirmPasswordRequired: t('resetPassword.confirmPasswordRequired'),
        confirmPasswordMatch: t('resetPassword.confirmPasswordMismatch'),
      }),
    [t]
  );

  const methods = useForm<ResetPasswordFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      newPassword: '',
      confirmPassword: '',
    },
  });

  useEffect(() => {
    let active = true;
    void (async () => {
      const csrf = await ensureCsrfToken();
      if (!active) return;
      if (csrf) {
        setCsrfToken(csrf);
      } else {
        setError(t('errors.csrfError'));
      }
    })();

    return () => {
      active = false;
    };
  }, [t]);

  const onSubmit = async (data: ResetPasswordFormData) => {
    if (!token) {
      setError(t('resetPassword.errors.missingToken'));
      return;
    }

    if (!csrfToken) {
      setError(t('errors.csrfError'));
      return;
    }

    setError(null);

    try {
      await apiFetch('/auth/reset-password', {
        method: 'POST',
        body: JSON.stringify({
          token,
          newPassword: data.newPassword,
        }),
        headers: {
          'x-csrf-token': csrfToken,
        },
      });

      setSuccess(true);
      router.replace(`/${locale}/login?reset=success` as never);
      router.refresh();
    } catch (err) {
      if (isApiError(err) && err.status === 403) {
        setError(t('errors.csrfError'));
        return;
      }

      if (isApiError(err) && err.status === 400) {
        const code = extractApiErrorCode(err.body);
        if (code === 'RESET_TOKEN_ALREADY_USED') {
          setError(t('resetPassword.errors.tokenUsed'));
        } else {
          setError(t('resetPassword.errors.tokenExpired'));
        }
        return;
      }

      setError(t('errors.genericError'));
    }
  };

  if (!token) {
    return (
      <AuthLayout>
        <AuthBrand />
        <div className="w-full bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
          <div className="px-6 py-8 sm:p-10 flex flex-col items-center text-center">
            <div className="w-16 h-16 bg-red-50 rounded-full flex items-center justify-center mb-6">
              <span className="material-symbols-outlined text-red-500 text-3xl">cancel</span>
            </div>
            <h1 className="text-xl sm:text-2xl font-bold text-slate-900 mb-2">{t('resetPassword.title')}</h1>
            <p className="text-slate-500 text-sm mb-6">{t('resetPassword.errors.missingToken')}</p>
          </div>
          <div className="bg-slate-50 border-t border-slate-200 px-6 py-4 flex justify-center">
            <Link href="/forgot-password" className="flex items-center gap-2 text-sm font-medium text-slate-600 hover:text-primary-600 transition-colors">
              <span className="material-symbols-outlined text-[18px]">arrow_back</span>
              {t('resetPassword.requestNewLink')}
            </Link>
          </div>
        </div>
        <AuthFooterLinks />
      </AuthLayout>
    );
  }

  return (
    <AuthLayout>
      <AuthBrand />
      <div className="w-full bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
        <div className="p-8">
          {/* Status icon & heading — Stitch reset-password.html */}
          <div className="flex flex-col items-center text-center mb-8">
            <div className="w-16 h-16 bg-primary-600/10 rounded-full flex items-center justify-center text-primary-600 mb-4">
              <span className="material-symbols-outlined text-3xl">lock_open</span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900 mb-2">{t('resetPassword.title')}</h1>
            <p className="text-slate-500 text-sm">{t('resetPassword.description')}</p>
          </div>

          {success ? (
            <div className="flex flex-col items-center text-center">
              <div className="w-16 h-16 bg-green-50 rounded-full flex items-center justify-center mb-6">
                <span className="material-symbols-outlined text-green-500 text-3xl">check_circle</span>
              </div>
              <p className="text-sm font-medium text-slate-900">{t('resetPassword.successTitle')}</p>
              <p className="mt-1 text-sm text-slate-500">{t('resetPassword.successDescription')}</p>
            </div>
          ) : (
            <FormProvider {...methods}>
              <form className="space-y-6" noValidate onSubmit={methods.handleSubmit(onSubmit)}>
                <FormField<ResetPasswordFormData>
                  name="newPassword"
                  label={t('resetPassword.newPasswordLabel')}
                  type="password"
                  autoComplete="new-password"
                  placeholder="••••••••"
                />

                <FormField<ResetPasswordFormData>
                  name="confirmPassword"
                  label={t('resetPassword.confirmPasswordLabel')}
                  type="password"
                  autoComplete="new-password"
                  placeholder="••••••••"
                />

                <FormError message={error} />

                <FormSubmitButton
                  disabled={!csrfToken}
                  loadingText={t('resetPassword.submitting')}
                  className="w-full bg-primary-600 hover:bg-primary-600/90 text-white font-medium text-sm py-3 rounded-lg transition-colors shadow-sm"
                >
                  {t('resetPassword.submit')}
                </FormSubmitButton>
              </form>
            </FormProvider>
          )}
        </div>
        <div className="bg-slate-50 border-t border-slate-200 px-8 py-4 flex justify-center">
          <Link href="/login" className="flex items-center gap-2 text-sm font-medium text-slate-600 hover:text-primary-600 transition-colors">
            <span className="material-symbols-outlined text-[18px]">arrow_back</span>
            {t('resetPassword.backToLogin')}
          </Link>
        </div>
      </div>
      <AuthFooterLinks />
    </AuthLayout>
  );
}

function extractApiErrorCode(body: unknown): string | null {
  if (!body || typeof body !== 'object') {
    return null;
  }

  if ('error' in body && typeof body.error === 'string') {
    return body.error;
  }

  if ('detail' in body && typeof body.detail === 'string') {
    return body.detail;
  }

  return null;
}
