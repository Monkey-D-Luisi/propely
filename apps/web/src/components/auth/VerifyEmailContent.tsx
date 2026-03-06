// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { useLocale, useTranslations } from 'next-intl';
import { Link, useRouter } from '@/i18n/navigation';
import { apiFetch, isApiError } from '@/lib/api';
import { dispatchAuthUserUpdatedEvent } from '@/lib/auth-events';
import { ensureCsrfToken } from '@/lib/csrf';
import { AuthLayout, AuthBrand, AuthFooterLinks } from '@/components/auth/auth-layout';

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
      <AuthBrand />

      {/* Loading state — Stitch verify-email-loading.html */}
      {state === 'loading' ? (
        <div className="w-full bg-white rounded-xl border border-slate-200 shadow-sm p-8 flex flex-col items-center">
          <div className="mb-6">
            <div className="w-12 h-12 border-4 border-primary-600/20 border-t-primary-600 rounded-full animate-spin" />
          </div>
          <h1 className="text-xl font-bold text-slate-900 mb-2 text-center">{t('verifyEmail.loadingTitle')}</h1>
          <p className="text-slate-500 text-center text-sm leading-relaxed mb-8 max-w-[320px]">{t('verifyEmail.loadingDescription')}</p>
          {/* Skeleton loading card */}
          <div className="w-full border border-slate-100 rounded-lg p-5 bg-slate-50/50">
            <div className="flex items-center gap-4 mb-5">
              <div className="w-12 h-12 rounded-full bg-slate-200 animate-pulse" />
              <div className="flex-1 space-y-2">
                <div className="h-3 w-1/2 bg-slate-200 rounded animate-pulse" />
                <div className="h-2 w-1/3 bg-slate-200 rounded animate-pulse" />
              </div>
            </div>
            <div className="space-y-3">
              <div className="h-2 w-full bg-slate-200 rounded animate-pulse" />
              <div className="h-2 w-5/6 bg-slate-200 rounded animate-pulse" />
              <div className="h-2 w-4/6 bg-slate-200 rounded animate-pulse" />
            </div>
          </div>
          <div className="sr-only" role="status" aria-live="polite">{t('verifyEmail.loadingTitle')}</div>
        </div>
      ) : null}

      {/* Success state — Stitch verify-email-success.html */}
      {state === 'success' ? (
        <div className="w-full bg-white rounded-xl border border-slate-200 shadow-sm p-8 flex flex-col items-center text-center">
          <div className="w-16 h-16 bg-green-50 rounded-full flex items-center justify-center mb-6">
            <span className="material-symbols-outlined text-4xl text-green-500">check_circle</span>
          </div>
          <h1 className="text-2xl font-bold text-slate-900 mb-2">{t('verifyEmail.successTitle')}</h1>
          <p className="text-slate-500 mb-8">{t('verifyEmail.successDescription')}</p>
          <Link
            href="/"
            className="w-full h-12 bg-primary-600 hover:bg-primary-600/90 text-white rounded-lg font-semibold flex items-center justify-center gap-2 transition-colors"
          >
            {t('verifyEmail.goToDashboard')}
            <span className="material-symbols-outlined text-xl">arrow_right_alt</span>
          </Link>
          <p className="mt-6 text-xs italic text-slate-400">{t('verifyEmail.autoRedirect')}</p>
        </div>
      ) : null}

      {/* Error state — Stitch verify-email-error.html */}
      {state === 'error' ? (
        <div className="w-full bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
          <div className="px-6 py-8 sm:p-10 flex flex-col items-center text-center">
            <div className="w-16 h-16 bg-red-50 rounded-full flex items-center justify-center mb-6">
              <span className="material-symbols-outlined text-red-500 text-3xl">cancel</span>
            </div>
            <h1 className="text-xl sm:text-2xl font-bold text-slate-900 mb-2">{t('verifyEmail.errorTitle')}</h1>
            <p className="text-slate-500 text-sm mb-6 max-w-sm">{renderVerificationError(error, t)}</p>

            {/* Action buttons */}
            <div className="w-full flex flex-col gap-3">
              <button
                type="button"
                onClick={() => void onResend()}
                disabled={resendState === 'sending'}
                className="flex items-center justify-center gap-2 w-full h-11 px-4 bg-primary-600 hover:bg-primary-600/90 text-white rounded-lg text-sm font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-60"
              >
                <span className="material-symbols-outlined text-[20px]" aria-hidden="true">mail</span>
                {resendState === 'sending' ? t('verifyEmail.resendSending') : t('verifyEmail.resendAction')}
              </button>
            </div>
            {(() => {
              const feedback = renderResendFeedback(resendState, t);
              return feedback ? (
                <p className="mt-4 text-xs text-slate-500">{feedback}</p>
              ) : null;
            })()}
          </div>
          <div className="bg-slate-50 border-t border-slate-200 px-6 py-4 flex justify-center">
            <Link
              href="/login"
              className="flex items-center gap-2 text-sm text-slate-500 hover:text-slate-800 transition-colors font-medium"
            >
              <span className="material-symbols-outlined text-[18px]">arrow_back</span>
              {t('verifyEmail.goToLogin')}
            </Link>
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
