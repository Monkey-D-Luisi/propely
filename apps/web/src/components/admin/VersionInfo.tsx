// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { useVersion, useUpdateCheck } from '@/hooks/version';
import { Button } from '@/components/ui/button';

export function VersionInfo() {
  const t = useTranslations('version');
  const { version, isLoading, error } = useVersion();
  const { updateInfo, isChecking, error: checkError, checkForUpdates } = useUpdateCheck();

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="h-32 animate-pulse rounded-xl bg-slate-100" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-xl border border-red-200 bg-red-50 p-6">
        <p className="text-sm text-red-600">{t('loadError')}</p>
      </div>
    );
  }

  const clientVersion = process.env.NEXT_PUBLIC_APP_VERSION ?? null;

  return (
    <div className="space-y-6">
      {/* Current Version Card */}
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-100 text-primary-600">
            <span className="material-symbols-outlined text-xl" aria-hidden="true">info</span>
          </div>
          <div>
            <h2 className="text-lg font-semibold text-slate-900">{t('currentVersion')}</h2>
            <p className="text-2xl font-bold tabular-nums text-slate-900">
              {version?.version ?? t('notAvailable')}
            </p>
          </div>
        </div>
        <dl className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <dt className="text-xs font-medium uppercase tracking-wider text-slate-500">{t('buildDate')}</dt>
            <dd className="mt-1 text-sm text-slate-700">{version?.buildDate ?? t('notAvailable')}</dd>
          </div>
          {clientVersion && (
            <div>
              <dt className="text-xs font-medium uppercase tracking-wider text-slate-500">Frontend</dt>
              <dd className="mt-1 text-sm text-slate-700">{clientVersion}</dd>
            </div>
          )}
        </dl>
      </div>

      {/* Update Check Card */}
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold text-slate-900">{t('checkForUpdates')}</h2>
          <Button
            type="button"
            onClick={() => void checkForUpdates()}
            disabled={isChecking}
            className="inline-flex items-center justify-center"
          >
            {isChecking ? t('checking') : t('checkForUpdates')}
          </Button>
        </div>

        {checkError != null && (
          <p className="mt-3 text-sm text-red-600">{t('checkError')}</p>
        )}

        {updateInfo && (
          <div className="mt-4">
            {!updateInfo.enabled ? (
              <div className="rounded-lg border border-amber-200 bg-amber-50 p-4">
                <p className="text-sm font-medium text-amber-800">{t('updateCheckDisabled')}</p>
                <p className="mt-1 text-xs text-amber-600">{t('enableHint')}</p>
              </div>
            ) : updateInfo.updateAvailable ? (
              <div className="rounded-lg border border-primary-200 bg-primary-50 p-4">
                <div className="flex items-center gap-2">
                  <span className="material-symbols-outlined text-primary-600" aria-hidden="true">upgrade</span>
                  <span className="text-sm font-semibold text-primary-800">{t('updateAvailable')}</span>
                </div>
                <p className="mt-2 text-sm text-primary-700">
                  {t('latestVersion')}: <span className="font-mono font-semibold">{updateInfo.latest}</span>
                </p>
                {updateInfo.releaseUrl && (
                  <a
                    href={updateInfo.releaseUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="mt-2 inline-flex items-center gap-1 text-sm font-medium text-primary-700 underline hover:text-primary-800"
                  >
                    {t('viewRelease')}
                    <span className="material-symbols-outlined text-sm" aria-hidden="true">open_in_new</span>
                  </a>
                )}
              </div>
            ) : updateInfo.message ? (
              <div className="rounded-lg border border-amber-200 bg-amber-50 p-4">
                <div className="flex items-center gap-2">
                  <span className="material-symbols-outlined text-amber-600" aria-hidden="true">warning</span>
                  <span className="text-sm font-semibold text-amber-800">{t('checkError')}</span>
                </div>
                <p className="mt-1 text-xs text-amber-600">{updateInfo.message}</p>
              </div>
            ) : (
              <div className="rounded-lg border border-green-200 bg-green-50 p-4">
                <div className="flex items-center gap-2">
                  <span className="material-symbols-outlined text-green-600" aria-hidden="true">check_circle</span>
                  <span className="text-sm font-semibold text-green-800">{t('upToDate')}</span>
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
