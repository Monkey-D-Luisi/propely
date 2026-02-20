// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useMemo, useRef, useState } from 'react';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { apiFetch, isApiError } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { useCurrentUser } from '@/hooks/orgs';
import { AuthFooterLinks } from '@/components/auth/auth-layout';

type Status = 'idle' | 'processing' | 'success' | 'error' | 'redirecting';

export function AcceptInvite() {
  const t = useTranslations('orgs');
  const tCommon = useTranslations('common');
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();
  const paramsString = useMemo(() => searchParams.toString(), [searchParams]);
  const nextTarget = useMemo(
    () => `${pathname}${paramsString ? `?${paramsString}` : ''}`,
    [pathname, paramsString],
  );
  const encodedNext = useMemo(() => encodeURIComponent(nextTarget), [nextTarget]);
  const { user, isLoading: isUserLoading } = useCurrentUser();
  const token = searchParams.get('token');
  const [status, setStatus] = useState<Status>('idle');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [attemptId, setAttemptId] = useState(0);
  const redirectingRef = useRef(false);

  useEffect(() => {
    if (!token) {
      setStatus('error');
      setErrorMessage(t('acceptInvite.errorCodes.missingToken'));
      return;
    }

    if (isUserLoading) {
      return;
    }

    if (!user) {
      if (!redirectingRef.current) {
        redirectingRef.current = true;
        setStatus('redirecting');
        router.replace(`/login?next=${encodedNext}&reauth=1`);
      }
      return;
    }

    let cancelled = false;

    const accept = async () => {
      setStatus('processing');
      setErrorMessage(null);

      try {
        const csrfToken = await ensureCsrfToken();
        if (!csrfToken) {
          throw new Error('CSRF missing');
        }

        await apiFetch('/orgs/accept-invite', {
          method: 'POST',
          body: JSON.stringify({ token }),
          headers: {
            'x-csrf-token': csrfToken,
          },
        });

        if (cancelled) return;

        setStatus('success');
      } catch (error) {
        if (cancelled) return;

        if (isApiError(error) && error.status === 401) {
          redirectingRef.current = true;
          setStatus('redirecting');
          router.replace(`/login?next=${encodedNext}&reauth=1`);
          return;
        }

        setStatus('error');
        if (isApiError(error)) {
          if (error.status === 400) {
            const code = (error.body != null && typeof error.body === 'object' && 'error' in error.body)
              ? String((error.body as Record<string, unknown>).error)
              : undefined;
            switch (code) {
              case 'INVALID_TOKEN':
                setErrorMessage(t('acceptInvite.errorCodes.invalidToken'));
                break;
              case 'ALREADY_USED':
                setErrorMessage(t('acceptInvite.errorCodes.alreadyUsed'));
                break;
              case 'EXPIRED':
                setErrorMessage(t('acceptInvite.errorCodes.expired'));
                break;
              case 'USER_EMAIL_MISMATCH':
                setErrorMessage(t('acceptInvite.errorCodes.emailMismatch'));
                break;
              default:
                setErrorMessage(t('acceptInvite.error.defaultDescription'));
            }
          } else {
            setErrorMessage(t('acceptInvite.error.defaultDescription'));
          }
        } else {
          setErrorMessage(t('acceptInvite.error.tryAgainDescription'));
        }
      }
    };

    void accept();

    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps -- status intentionally excluded to prevent
    // the effect from re-running (and cancelling the in-flight request) when setStatus('processing') fires.
  }, [token, isUserLoading, user, router, encodedNext, attemptId, t]);

  const renderMessage = () => {
    if (status === 'processing') {
      return {
        title: t('acceptInvite.processing.title'),
        description: t('acceptInvite.processing.description'),
      };
    }

    if (status === 'redirecting') {
      return {
        title: t('acceptInvite.redirecting.title'),
        description: t('acceptInvite.redirecting.description'),
      };
    }

    if (status === 'error') {
      return {
        title: t('acceptInvite.error.title'),
        description: errorMessage ?? t('acceptInvite.error.defaultDescription'),
      };
    }

    return {
      title: t('acceptInvite.success.title'),
      description: t('acceptInvite.success.description'),
    };
  };

  const message = renderMessage();

  return (
    <div className="flex min-h-dvh flex-col items-center justify-center px-4 py-12">
      <div className="w-full max-w-md rounded-2xl bg-white p-8 text-center shadow-xl">
        {/* Status icon */}
        {(status === 'processing' || status === 'redirecting' || status === 'idle') && (
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-primary-100">
            <svg
              className="h-8 w-8 animate-spin text-primary-600"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
            >
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
          </div>
        )}
        {status === 'success' && (
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-emerald-100">
            <svg className="h-8 w-8 text-emerald-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="2" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
        )}
        {status === 'error' && (
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-red-100">
            <svg className="h-8 w-8 text-red-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="2" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M9.75 9.75l4.5 4.5m0-4.5l-4.5 4.5M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
        )}

        <h1 className="mt-6 text-xl font-semibold text-slate-900">{message.title}</h1>
        <p className="mt-2 text-sm text-slate-500">{message.description}</p>

        {status === 'error' && (
          <div className="mt-6 space-y-4">
            <div className="rounded-xl bg-slate-50 p-4 text-sm text-slate-600">
              <p>{t('acceptInvite.error.persistHint')}</p>
            </div>
            <div className="flex flex-col items-center gap-3 sm:flex-row sm:justify-center">
              <button
                type="button"
                onClick={() => {
                  setStatus('idle');
                  setAttemptId((value) => value + 1);
                }}
                className="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
              >
                {tCommon('tryAgain')}
              </button>
              <a
                href={`/login?next=${encodedNext}&reauth=1`}
                className="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
              >
                {t('acceptInvite.error.signInAgain')}
              </a>
            </div>
          </div>
        )}

        {status === 'success' && (
          <div className="mt-6">
            <Link
              href="/orgs/mine"
              className="inline-flex w-full items-center justify-center rounded-lg bg-primary-600 py-3 text-sm font-medium text-white shadow-lg shadow-primary-600/25 transition-all hover:bg-primary-600/90 active:scale-[0.98] focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
            >
              {tCommon('continueToOrgs')}
            </Link>
          </div>
        )}
      </div>

      <AuthFooterLinks />
    </div>
  );
}