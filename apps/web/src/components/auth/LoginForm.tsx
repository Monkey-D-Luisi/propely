// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from '@/i18n/navigation';
import { AuthLayout, AuthBrand, AuthCard, AuthFooterLinks } from '@/components/auth/auth-layout';
import { FormField, FormSubmitButton, FormError } from '@/components/ui/form';
import { OAuthButtons } from '@/components/auth/OAuthButtons';
import { apiFetch, isApiError } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { useCurrentUser } from '@/hooks/orgs';
import { createLoginFormSchema, type LoginFormData } from '@/lib/schemas';

type LoginError =
  | 'INVALID_CREDENTIALS'
  | 'CSRF'
  | 'RATE_LIMITED'
  | 'OAUTH'
  | 'PROVIDER_ALREADY_LINKED'
  | 'UNKNOWN';

export function LoginForm() {
  const t = useTranslations('auth');
  const router = useRouter();
  const searchParams = useSearchParams();
  const { user, isLoading: isUserLoading } = useCurrentUser();
  const [csrfToken, setCsrfToken] = useState<string | null>(null);
  const [error, setError] = useState<LoginError | null>(null);
  const [retryAfterSeconds, setRetryAfterSeconds] = useState<number | null>(null);

  const schema = useMemo(
    () =>
      createLoginFormSchema({
        emailRequired: t('validation.emailRequired'),
        emailInvalid: t('validation.emailInvalid'),
        passwordRequired: t('validation.passwordRequired'),
      }),
    [t]
  );

  const methods = useForm<LoginFormData>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', password: '' },
  });

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

  const forceReauth = useMemo(() => searchParams.get('reauth') === '1', [searchParams]);
  const oauthErrorCode = useMemo(() => searchParams.get('oauthError'), [searchParams]);
  const oauthError = useMemo<LoginError | null>(() => {
    if (!oauthErrorCode) {
      return null;
    }

    return oauthErrorCode === 'provider_already_linked' ? 'PROVIDER_ALREADY_LINKED' : 'OAUTH';
  }, [oauthErrorCode]);

  useEffect(() => {
    if (!forceReauth && !isUserLoading && user) {
      router.replace(nextParam as never);
      router.refresh();
    }
  }, [forceReauth, isUserLoading, user, nextParam, router]);

  useEffect(() => {
    let active = true;
    void (async () => {
      const token = await ensureCsrfToken();
      if (!active) return;
      if (token) {
        setCsrfToken(token);
      } else {
        setError('CSRF');
      }
    })();
    return () => {
      active = false;
    };
  }, []);

  const onSubmit = async (data: LoginFormData) => {
    if (!csrfToken) {
      setError('CSRF');
      return;
    }

    setError(null);
    setRetryAfterSeconds(null);
    try {
      await apiFetch('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email: data.email, password: data.password }),
        headers: {
          'x-csrf-token': csrfToken,
        },
      });

      if (typeof window !== 'undefined') {
        window.location.assign(nextParam);
      } else {
        router.replace(nextParam as never);
        router.refresh();
      }
    } catch (err) {
      if (isApiError(err)) {
        if (err.status === 401) {
          setError('INVALID_CREDENTIALS');
        } else if (err.status === 403) {
          setError('CSRF');
        } else if (err.status === 429) {
          setError('RATE_LIMITED');
          setRetryAfterSeconds(err.retryAfterSeconds ?? 60);
        } else {
          setError('UNKNOWN');
        }
      } else {
        setError('UNKNOWN');
      }
    }
  };

  const renderErrorMessage = () => {
    const effectiveError = error ?? oauthError;
    if (!effectiveError) return null;

    switch (effectiveError) {
      case 'INVALID_CREDENTIALS':
        return t('errors.invalidCredentials');
      case 'CSRF':
        return t('errors.csrfError');
      case 'RATE_LIMITED':
        return t('errors.rateLimited', { seconds: retryAfterSeconds ?? 60 });
      case 'OAUTH':
        return t('errors.oauthError');
      case 'PROVIDER_ALREADY_LINKED':
        return t('errors.providerAlreadyLinked');
      default:
        return t('errors.genericError');
    }
  };

  return (
    <AuthLayout>
      <AuthBrand>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900">{t('login.title')}</h1>
        <p className="mt-1 text-sm text-slate-500">{t('login.description')}</p>
        {nextParam !== '/' ? (
          <p className="mt-1 text-xs text-slate-500">
            {t('login.redirectNotice', { destination: nextParam })}
          </p>
        ) : null}
      </AuthBrand>

      <AuthCard>
        <FormProvider {...methods}>
          <form className="space-y-6" noValidate onSubmit={methods.handleSubmit(onSubmit)}>
            <FormField<LoginFormData>
              name="email"
              label={t('login.emailLabel')}
              type="email"
              autoComplete="email"
              placeholder="name@company.com"
            />

            <FormField<LoginFormData>
              name="password"
              label={t('login.passwordLabel')}
              type="password"
              autoComplete="current-password"
              placeholder="••••••••"
              labelRight={
                <Link href="/forgot-password" className="text-xs font-semibold text-primary-600 hover:underline">
                  {t('login.forgotPassword')}
                </Link>
              }
            />

            <FormError message={renderErrorMessage()} />

            <FormSubmitButton
              disabled={!csrfToken}
              loadingText={t('login.submitting')}
              className="w-full py-3 shadow-lg shadow-primary-600/25"
            >
              {t('login.submit')}
            </FormSubmitButton>
          </form>
        </FormProvider>

        {/* Divider */}
        <div className="relative py-4">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-slate-200" />
          </div>
          <div className="relative flex justify-center text-xs uppercase">
            <span className="bg-white px-4 text-slate-500">{t('oauth.orDivider')}</span>
          </div>
        </div>

        <OAuthButtons nextPath={nextParam} />
      </AuthCard>

      <p className="mt-8 text-center text-sm text-slate-500">
        {t('login.noAccount')}{' '}
        <Link href="/register" className="ml-1 font-semibold text-primary-600 hover:underline">
          {t('login.createAccount')}
        </Link>
      </p>

      <AuthFooterLinks />
    </AuthLayout>
  );
}
