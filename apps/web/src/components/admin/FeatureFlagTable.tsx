// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback, useState } from 'react';
import { useTranslations } from 'next-intl';
import { useFeatureFlags, useToggleFeatureFlag } from '@/hooks/feature-flags';
import { useToast } from '@/components/ui/toast';
import { Button } from '@/components/ui/button';
import { FlagIcon } from '@/components/ui/icons';
import type { FeatureFlag } from '@/lib/schemas';

export function FeatureFlagTable() {
  const t = useTranslations('featureFlags');
  const tCommon = useTranslations('common');
  const { flags, isLoading, error, refetch } = useFeatureFlags();
  const toggleFlag = useToggleFeatureFlag();
  const { toast } = useToast();
  const [pendingFlags, setPendingFlags] = useState<Set<string>>(new Set());

  const handleToggle = useCallback(
    async (flag: FeatureFlag) => {
      const newState = !flag.isEnabled;
      setPendingFlags((prev) => new Set(prev).add(flag.name));
      try {
        await toggleFlag(flag.name, newState);
        toast({
          title: t('toggleSuccess'),
          variant: 'success',
        });
        await refetch();
      } catch {
        toast({
          title: t('toggleError'),
          variant: 'destructive',
        });
      } finally {
        setPendingFlags((prev) => {
          const next = new Set(prev);
          next.delete(flag.name);
          return next;
        });
      }
    },
    [toggleFlag, refetch, toast, t],
  );

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[1, 2, 3].map((i) => (
          <div
            key={i}
            className="h-14 animate-pulse rounded-lg bg-slate-100"
          />
        ))}
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-xl border border-red-200 bg-red-50 p-5">
        <p className="text-sm text-red-700">{t('loadError')}</p>
        <Button
          type="button"
          onClick={() => void refetch()}
          className="mt-3 bg-red-600 hover:bg-red-500"
        >
          {tCommon('tryAgain')}
        </Button>
      </div>
    );
  }

  if (flags.length === 0) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-10 text-center">
        <FlagIcon className="mx-auto h-10 w-10 text-slate-300" />
        <p className="mt-3 text-sm text-slate-500">{t('emptyState')}</p>
      </div>
    );
  }

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <table className="w-full text-left text-sm">
        <thead className="border-b border-slate-200 bg-slate-50/50">
          <tr>
            <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
              {t('name')}
            </th>
            <th className="hidden px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 sm:table-cell">
              {t('description')}
            </th>
            <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
              {t('status')}
            </th>
            <th className="hidden px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 md:table-cell">
              {t('source')}
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {flags.map((flag) => {
            const isPending = pendingFlags.has(flag.name);
            return (
              <tr key={flag.name} className="group hover:bg-slate-50 transition-colors">
                <td className="px-6 py-5">
                  <span className="font-semibold text-slate-900">
                    {flag.name}
                  </span>
                  <span className="mt-0.5 block text-xs text-slate-500 sm:hidden">
                    {flag.description || t('noDescription')}
                  </span>
                </td>
                <td className="hidden px-6 py-5 text-slate-500 sm:table-cell">
                  {flag.description || (
                    <span className="text-slate-400">{t('noDescription')}</span>
                  )}
                </td>
                <td className="px-6 py-5">
                  <button
                    type="button"
                    role="switch"
                    aria-checked={flag.isEnabled}
                    aria-label={`${flag.name}: ${flag.isEnabled ? t('enabled') : t('disabled')}`}
                    disabled={isPending}
                    onClick={() => void handleToggle(flag)}
                    className={`relative inline-flex h-6 w-11 shrink-0 cursor-pointer items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${
                      flag.isEnabled ? 'bg-primary-600' : 'bg-slate-300'
                    }`}
                  >
                    <span
                      className={`inline-block h-4 w-4 rounded-full bg-white shadow transition-transform ${
                        flag.isEnabled ? 'translate-x-6' : 'translate-x-1'
                      }`}
                    />
                  </button>
                </td>
                <td className="hidden px-6 py-5 md:table-cell">
                  <span
                    className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                      flag.source === 'Database'
                        ? 'bg-primary-50 text-primary-700'
                        : 'bg-slate-100 text-slate-600'
                    }`}
                  >
                    {flag.source === 'Database'
                      ? t('sourceDb')
                      : t('sourceConfig')}
                  </span>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
