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
import { CheckIcon, ArrowLeftIcon, AlertTriangleIcon, LockIcon } from '@/components/ui/icons';
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
        <AuthBrand icon={<LockIcon className="h-7 w-7 text-white" />} />
        <div className="overflow-hidden rounded-2xl bg-white shadow-xl">
          <div className="h-1 w-full bg-gradient-to-r from-transparent via-primary-500 to-transparent opacity-50" />
          <div className="p-8 text-center">
            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-red-100">
              <AlertTriangleIcon className="h-6 w-6 text-red-600" />
            </div>
            <h1 className="mt-4 text-2xl font-semibold tracking-tight text-slate-900">{t('resetPassword.title')}</h1>
            <p className="mt-2 text-sm text-red-600">{t('resetPassword.errors.missingToken')}</p>
          </div>
          <div className="flex justify-center border-t border-slate-100 bg-slate-50 px-8 py-4">
            <Link href="/forgot-password" className="group inline-flex items-center gap-1.5 text-sm font-medium text-primary-600 transition hover:text-primary-700">
              <ArrowLeftIcon className="h-4 w-4 transition group-hover:-translate-x-0.5" />
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
      <AuthBrand icon={<LockIcon className="h-7 w-7 text-white" />} />
      <div className="overflow-hidden rounded-2xl bg-white shadow-xl">
        <div className="h-1 w-full bg-gradient-to-r from-transparent via-primary-500 to-transparent opacity-50" />
        <div className="p-8">
          <div className="mb-8 text-center">
            <h1 className="mb-2 text-2xl font-semibold tracking-tight text-slate-900">{t('resetPassword.title')}</h1>
            <p className="text-sm text-slate-500">{t('resetPassword.description')}</p>
          </div>

          {success ? (
            <div className="text-center">
              <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-emerald-100">
                <CheckIcon className="h-6 w-6 text-emerald-600" />
              </div>
              <p className="mt-4 text-sm font-medium text-slate-900">{t('resetPassword.successTitle')}</p>
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

                <div className="pt-2">
                  <FormSubmitButton
                    disabled={!csrfToken}
                    loadingText={t('resetPassword.submitting')}
                    className="w-full py-3 shadow-lg shadow-primary-600/25"
                  >
                    {t('resetPassword.submit')}
                  </FormSubmitButton>
                </div>
              </form>
            </FormProvider>
          )}
        </div>
        <div className="flex justify-center border-t border-slate-100 bg-slate-50 px-8 py-4">
          <Link href="/login" className="group inline-flex items-center gap-1.5 text-sm font-medium text-primary-600 transition hover:text-primary-700">
            <ArrowLeftIcon className="h-4 w-4 transition group-hover:-translate-x-0.5" />
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
