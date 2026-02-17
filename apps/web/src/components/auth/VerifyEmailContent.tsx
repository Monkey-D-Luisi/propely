// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { useLocale, useTranslations } from 'next-intl';
import { Link, useRouter } from '@/i18n/navigation';
import { apiFetch, isApiError } from '@/lib/api';
import { dispatchAuthUserUpdatedEvent } from '@/lib/auth-events';
import { ensureCsrfToken } from '@/lib/csrf';
import { AuthLayout, AuthFooterLinks } from '@/components/auth/auth-layout';
import { SpinnerIcon, CheckIcon, XMarkIcon, ArrowLeftIcon } from '@/components/ui/icons';

type VerificationState = 'loading' | 'success' | 'error';
type VerificationError = 'missing_token' | 'expired_or_invalid' | 'generic';
type ResendState = 'idle' | 'sending' | 'sent' | 'rate_limited' | 'already_verified' | 'error';

type ResendVerificationResponse = {
  ok: boolean;
  sent: boolean;
  alreadyVerified: boolean;
};

export function VerifyEmailContent() {
  const t = useTranslations('auth');
  const locale = useLocale();
  const router = useRouter();
  const searchParams = useSearchParams();

  const token = useMemo(() => searchParams.get('token'), [searchParams]);
  const [state, setState] = useState<VerificationState>('loading');
  const [error, setError] = useState<VerificationError>('generic');
  const [resendState, setResendState] = useState<ResendState>('idle');

  useEffect(() => {
    let active = true;

    const verifyEmail = async () => {
      if (!token) {
        if (!active) return;
        setError('missing_token');
        setState('error');
        return;
      }

      const csrfToken = await ensureCsrfToken();
      if (!csrfToken) {
        if (!active) return;
        setError('generic');
        setState('error');
        return;
      }

      try {
        await apiFetch('/auth/verify-email', {
          method: 'POST',
          body: JSON.stringify({ token }),
          headers: {
            'x-csrf-token': csrfToken,
          },
        });

        if (!active) return;
        setState('success');
        dispatchAuthUserUpdatedEvent();
        const destination = await resolvePostVerificationDestination();
        if (!active) return;
        window.setTimeout(() => {
          router.replace(destination);
          router.refresh();
        }, 1200);
      } catch (err) {
        if (!active) return;

        if (isApiError(err)) {
          const detail = extractDetail(err.body);
          if (err.status === 400 && detail === 'INVALID_OR_EXPIRED_VERIFICATION_TOKEN') {
            setError('expired_or_invalid');
            setState('error');
            return;
          }
        }

        setError('generic');
        setState('error');
      }
    };

    void verifyEmail();

    return () => {
      active = false;
    };
  }, [token, router]);

  const onResend = async () => {
    setResendState('sending');
    const csrfToken = await ensureCsrfToken();
    if (!csrfToken) {
      setResendState('error');
      return;
    }

    try {
      const response = await apiFetch<ResendVerificationResponse>('/auth/resend-verification', {
        method: 'POST',
        body: JSON.stringify({ locale }),
        headers: {
          'x-csrf-token': csrfToken,
        },
      });

      if (response.alreadyVerified) {
        setResendState('already_verified');
        return;
      }

      if (response.sent) {
        setResendState('sent');
        return;
      }

      setResendState('error');
    } catch (err) {
      if (isApiError(err) && (err.status === 401 || err.status === 403)) {
        router.replace('/login');
        return;
      }

      if (isApiError(err) && err.status === 429) {
        setResendState('rate_limited');
        return;
      }

      setResendState('error');
    }
  };

  return (
    <AuthLayout>
      {state === 'loading' ? (
        <div className="rounded-2xl bg-white shadow-xl">
          <div className="h-1 w-full rounded-t-2xl bg-gradient-to-r from-transparent via-primary-500 to-transparent opacity-80" />
          <div className="p-8 text-center">
            <div className="relative mb-8 flex items-center justify-center">
              <div className="absolute h-24 w-24 animate-pulse rounded-full bg-primary-500/10" />
              <div className="relative flex h-16 w-16 items-center justify-center rounded-full bg-primary-50">
                <SpinnerIcon className="h-8 w-8 text-primary-600" />
              </div>
            </div>
            <h1 className="mb-3 text-2xl font-semibold tracking-tight text-slate-900">{t('verifyEmail.loadingTitle')}</h1>
            <p className="mb-8 text-sm leading-relaxed text-slate-500">{t('verifyEmail.loadingDescription')}</p>
            <div className="sr-only" role="status" aria-live="polite">{t('verifyEmail.loadingTitle')}</div>
          </div>
        </div>
      ) : null}

      {state === 'success' ? (
        <div className="rounded-2xl bg-white shadow-xl">
          <div className="h-1 w-full rounded-t-2xl bg-gradient-to-r from-emerald-400 to-primary-500" />
          <div className="p-8 text-center">
            <div className="relative mb-6">
              <div className="absolute inset-0 mx-auto h-16 w-16 animate-ping rounded-full bg-emerald-100 opacity-75" />
              <div className="relative mx-auto flex h-16 w-16 items-center justify-center rounded-full border border-emerald-100 bg-emerald-50">
                <CheckIcon className="h-8 w-8 text-emerald-600" />
              </div>
            </div>
            <h1 className="mb-3 text-2xl font-bold tracking-tight text-slate-900">{t('verifyEmail.successTitle')}</h1>
            <p className="mb-8 text-sm leading-relaxed text-slate-500">{t('verifyEmail.successDescription')}</p>
          </div>
          <div className="h-2 w-full bg-slate-50" />
        </div>
      ) : null}

      {state === 'error' ? (
        <div className="rounded-2xl bg-white shadow-xl">
          <div className="h-1.5 w-full rounded-t-2xl bg-red-500/80" />
          <div className="p-8 text-center">
            <div className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-red-100">
              <XMarkIcon className="h-8 w-8 text-red-600" />
            </div>
            <h1 className="mb-3 text-2xl font-bold tracking-tight text-slate-900">{t('verifyEmail.errorTitle')}</h1>
            <p className="mb-8 text-sm leading-relaxed text-slate-500">{renderVerificationError(error, t)}</p>
            <div className="space-y-4">
              <button
                type="button"
                onClick={() => void onResend()}
                disabled={resendState === 'sending'}
                className="w-full rounded-lg bg-primary-600 px-4 py-3 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-60"
              >
                {resendState === 'sending' ? t('verifyEmail.resendSending') : t('verifyEmail.resendAction')}
              </button>
              <Link
                href="/login"
                className="group flex items-center justify-center gap-1 text-sm font-medium text-slate-500 transition hover:text-slate-900"
              >
                <ArrowLeftIcon className="h-4 w-4 transition group-hover:-translate-x-0.5" />
                {t('verifyEmail.goToLogin')}
              </Link>
            </div>
            {(() => {
              const feedback = renderResendFeedback(resendState, t);
              return feedback ? (
                <p className="mt-4 text-xs text-slate-500">{feedback}</p>
              ) : null;
            })()}
          </div>
        </div>
      ) : null}

      <AuthFooterLinks />
    </AuthLayout>
  );
}

async function resolvePostVerificationDestination(): Promise<string> {
  try {
    await apiFetch('/auth/me', { method: 'GET' });
    return '/';
  } catch {
    return '/login';
  }
}

function extractDetail(body: unknown): string | null {
  if (!body || typeof body !== 'object') {
    return null;
  }

  if ('detail' in body && typeof body.detail === 'string') {
    return body.detail;
  }

  return null;
}

function renderVerificationError(error: VerificationError, t: ReturnType<typeof useTranslations>) {
  if (error === 'missing_token') {
    return t('verifyEmail.missingTokenDescription');
  }

  if (error === 'expired_or_invalid') {
    return t('verifyEmail.expiredDescription');
  }

  return t('verifyEmail.genericDescription');
}

function renderResendFeedback(state: ResendState, t: ReturnType<typeof useTranslations>) {
  if (state === 'sent') {
    return t('verifyEmail.resendSent');
  }

  if (state === 'already_verified') {
    return t('verificationBanner.alreadyVerified');
  }

  if (state === 'rate_limited') {
    return t('verifyEmail.resendRateLimited');
  }

  if (state === 'error') {
    return t('verifyEmail.resendError');
  }

  return '';
}
