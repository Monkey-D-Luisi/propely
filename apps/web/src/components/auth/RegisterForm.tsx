// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useLocale, useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from '@/i18n/navigation';
import { AuthLayout, AuthBrand, AuthCard, AuthFooterLinks } from '@/components/auth/auth-layout';
import { OAuthButtons } from '@/components/auth/OAuthButtons';
import { FormField, FormSubmitButton, FormError } from '@/components/ui/form';
import { apiFetch, isApiError } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { useCurrentUser } from '@/hooks/orgs';
import { createRegisterFormSchema, PASSWORD_MIN_LENGTH, type RegisterFormData } from '@/lib/schemas';

export function RegisterForm() {
  const t = useTranslations('auth');
  const locale = useLocale();
  const router = useRouter();
  const searchParams = useSearchParams();
  const { user, isLoading: isUserLoading } = useCurrentUser();
  const [csrfToken, setCsrfToken] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const nextParam = useMemo(() => {
    const raw = searchParams.get('next');
    if (!raw) return '/';

    let decoded: string;
    try {
      decoded = decodeURIComponent(raw);
    } catch {
      decoded = raw;
    }

    if (decoded.startsWith('/') && !decoded.startsWith('//')) {
      return decoded;
    }

    return '/';
  }, [searchParams]);
  const oauthError = useMemo(() => searchParams.get('oauthError'), [searchParams]);

  const schema = useMemo(
    () =>
      createRegisterFormSchema({
        emailRequired: t('validation.emailRequired'),
        emailInvalid: t('validation.emailInvalid'),
        passwordRequired: t('validation.passwordRequired'),
        passwordMinLength: t('validation.passwordMinLength', { min: PASSWORD_MIN_LENGTH }),
        confirmPasswordRequired: t('register.confirmPasswordRequired'),
        confirmPasswordMatch: t('register.confirmPasswordMismatch'),
      }),
    [t]
  );

  const methods = useForm<RegisterFormData>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', email: '', password: '', confirmPassword: '' },
  });

  useEffect(() => {
    if (!isUserLoading && user) {
      router.replace(nextParam as never);
    }
  }, [isUserLoading, user, nextParam, router]);

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
    return () => { active = false; };
  }, [t]);

  const onSubmit = async (data: RegisterFormData) => {
    if (!csrfToken) {
      setError(t('errors.csrfError'));
      return;
    }

    setError(null);
    try {
      await apiFetch('/auth/register', {
        method: 'POST',
        body: JSON.stringify({
          email: data.email,
          password: data.password,
          name: data.name || undefined,
          locale,
        }),
        headers: {
          'x-csrf-token': csrfToken,
        },
      });

      if (typeof window !== 'undefined') {
        window.location.assign(nextParam);
      } else {
        router.replace(nextParam as never);
      }
    } catch (err) {
      if (isApiError(err)) {
        if (err.status === 409) {
          setError(t('errors.emailExists'));
        } else if (err.status === 403) {
          setError(t('errors.csrfError'));
        } else if (err.status === 429) {
          const seconds = err.retryAfterSeconds ?? 60;
          setError(t('errors.rateLimited', { seconds }));
        } else if (err.body && typeof err.body === 'object' && 'error' in err.body) {
          setError(String((err.body as { error: string }).error));
        } else {
          setError(t('errors.genericError'));
        }
      } else {
        setError(t('errors.genericError'));
      }
    }
  };

  return (
    <AuthLayout>
      <AuthBrand>
        <p className="text-slate-500 font-medium">{t('register.description')}</p>
      </AuthBrand>

      <AuthCard>
        <FormProvider {...methods}>
          <form className="flex flex-col gap-4" noValidate onSubmit={methods.handleSubmit(onSubmit)}>
            <FormField<RegisterFormData>
              name="name"
              label={t('register.nameLabel')}
              type="text"
              autoComplete="name"
            />

            <FormField<RegisterFormData>
              name="email"
              label={t('register.emailLabel')}
              type="email"
              autoComplete="email"
              placeholder="name@company.com"
            />

            <FormField<RegisterFormData>
              name="password"
              label={t('register.passwordLabel')}
              type="password"
              autoComplete="new-password"
              placeholder="••••••••"
            />

            <FormField<RegisterFormData>
              name="confirmPassword"
              label={t('register.confirmPasswordLabel')}
              type="password"
              autoComplete="new-password"
              placeholder="••••••••"
            />

            <FormError message={error ?? (oauthError
              ? (oauthError === 'provider_already_linked' ? t('errors.providerAlreadyLinked') : t('errors.oauthError'))
              : null)} />

            <FormSubmitButton
              disabled={!csrfToken}
              loadingText={t('register.submitting')}
              className="mt-2 w-full bg-primary-600 hover:bg-primary-600/90 text-white font-semibold text-sm py-2.5 rounded-lg transition-colors shadow-sm"
            >
              {t('register.submit')}
            </FormSubmitButton>
          </form>
        </FormProvider>

        {/* Divider — Stitch style */}
        <div className="relative flex items-center">
          <div className="flex-grow border-t border-slate-200" />
          <span className="flex-shrink-0 mx-4 text-sm text-slate-500">{t('oauth.orContinueWith')}</span>
          <div className="flex-grow border-t border-slate-200" />
        </div>

        <OAuthButtons nextPath={nextParam} />
      </AuthCard>

      <div className="mt-8 text-center">
        <p className="text-sm text-slate-600">
          {t('register.hasAccount')}{' '}
          <Link href="/login" className="font-medium text-primary-600 hover:text-primary-600/80 transition-colors">
            {t('register.signIn')}
          </Link>
        </p>
      </div>

      <AuthFooterLinks />
    </AuthLayout>
  );
}
