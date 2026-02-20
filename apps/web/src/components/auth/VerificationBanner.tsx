// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState } from 'react';
import { useLocale, useTranslations } from 'next-intl';
import { apiFetch, isApiError } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';

type ResendState = 'idle' | 'sending' | 'sent' | 'rate_limited' | 'already_verified' | 'error';

type ResendVerificationResponse = {
  ok: boolean;
  sent: boolean;
  alreadyVerified: boolean;
};

export interface VerificationBannerProps {
  show: boolean;
}

export function VerificationBanner({ show }: VerificationBannerProps) {
  const t = useTranslations('auth');
  const locale = useLocale();
  const [state, setState] = useState<ResendState>('idle');

  if (!show) {
    return null;
  }

  const onResend = async () => {
    setState('sending');

    const csrfToken = await ensureCsrfToken();
    if (!csrfToken) {
      setState('error');
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
        setState('already_verified');
        return;
      }

      setState('sent');
    } catch (error) {
      if (isApiError(error) && error.status === 429) {
        setState('rate_limited');
        return;
      }

      setState('error');
    }
  };

  return (
    <div className="border-t border-amber-200 bg-amber-50">
      <div className="mx-auto flex w-full max-w-6xl flex-col gap-2 px-6 py-3 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-sm text-amber-900">{t('verificationBanner.message')}</p>
        <button
          type="button"
          onClick={() => void onResend()}
          disabled={state === 'sending'}
          className="inline-flex items-center justify-center rounded-md border border-amber-300 bg-white px-3 py-1.5 text-xs font-medium text-amber-900 transition hover:bg-amber-100 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {state === 'sending' ? t('verificationBanner.resendSending') : t('verificationBanner.resendAction')}
        </button>
      </div>
      <div className="mx-auto w-full max-w-6xl px-6 pb-3">
        <BannerFeedback state={state} />
      </div>
    </div>
  );

  function BannerFeedback({ state }: { state: ResendState }) {
    if (state === 'idle' || state === 'sending') {
      return null;
    }

    if (state === 'sent') {
      return <p className="text-xs text-emerald-700">{t('verificationBanner.sent')}</p>;
    }

    if (state === 'already_verified') {
      return <p className="text-xs text-emerald-700">{t('verificationBanner.alreadyVerified')}</p>;
    }

    if (state === 'rate_limited') {
      return <p className="text-xs text-amber-800">{t('verificationBanner.rateLimited')}</p>;
    }

    return <p className="text-xs text-rose-700">{t('verificationBanner.resendError')}</p>;
  }
}
