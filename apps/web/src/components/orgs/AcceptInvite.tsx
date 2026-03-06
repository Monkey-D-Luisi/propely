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
      <div className="w-full max-w-md bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden flex flex-col">
        <div className="p-8 flex flex-col items-center text-center">
          {/* Status icon */}
          {(status === 'processing' || status === 'redirecting' || status === 'idle') && (
            <div className="w-16 h-16 text-primary-600 animate-spin mb-6">
              <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
              </svg>
            </div>
          )}
          {status === 'success' && (
            <div className="w-20 h-20 bg-green-50 rounded-full flex items-center justify-center mb-6">
              <span className="material-symbols-outlined text-green-500 text-5xl" aria-hidden="true">check_circle</span>
            </div>
          )}
          {status === 'error' && (
            <div className="w-20 h-20 bg-red-50 rounded-full flex items-center justify-center mb-6">
              <span className="material-symbols-outlined text-red-500 text-4xl" aria-hidden="true">cancel</span>
            </div>
          )}

          <h1 className="text-2xl font-bold text-slate-900 mb-3">{message.title}</h1>
          <p className="text-slate-500 text-base leading-relaxed">{message.description}</p>

          {status === 'error' && (
            <div className="mt-8 w-full space-y-4">
              <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start text-left gap-3">
                <span className="material-symbols-outlined text-red-500 mt-0.5" aria-hidden="true">error</span>
                <div>
                  <p className="text-red-800 font-medium text-sm">{t('acceptInvite.error.title')}</p>
                  <p className="text-red-600 text-sm mt-1">{errorMessage ?? t('acceptInvite.error.defaultDescription')}</p>
                </div>
              </div>
              <div className="flex flex-col w-full gap-3">
                <button
                  type="button"
                  onClick={() => {
                    setStatus('idle');
                    setAttemptId((value) => value + 1);
                  }}
                  className="flex items-center justify-center w-full rounded-xl h-12 px-5 bg-primary-600 hover:bg-primary-600/90 text-white text-base font-bold leading-normal transition-colors"
                >
                  {tCommon('tryAgain')}
                </button>
                <a
                  href={`/login?next=${encodedNext}&reauth=1`}
                  className="flex items-center justify-center w-full rounded-xl h-12 px-5 bg-slate-100 hover:bg-slate-200 text-slate-900 text-base font-bold leading-normal transition-colors"
                >
                  {t('acceptInvite.error.signInAgain')}
                </a>
              </div>
            </div>
          )}

          {status === 'success' && (
            <div className="mt-8 w-full">
              <Link
                href="/orgs/mine"
                className="w-full bg-primary-600 hover:bg-primary-600/90 text-white font-medium py-3 px-6 rounded-lg transition-colors flex items-center justify-center gap-2 group"
              >
                {tCommon('continueToOrgs')}
                <span className="material-symbols-outlined group-hover:translate-x-1 transition-transform" aria-hidden="true">arrow_right_alt</span>
              </Link>
            </div>
          )}
        </div>

        {status === 'success' && (
          <div className="bg-slate-50 p-4 border-t border-slate-100 text-center">
            <p className="text-slate-500 text-sm italic flex items-center justify-center gap-2">
              <span className="material-symbols-outlined text-[18px] animate-spin" aria-hidden="true">autorenew</span>
              {t('acceptInvite.success.autoRedirect')}
            </p>
          </div>
        )}
      </div>

      <AuthFooterLinks />
    </div>
  );
}