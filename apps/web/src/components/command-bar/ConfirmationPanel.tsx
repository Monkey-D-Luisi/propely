// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { ActionResult } from '@/hooks/use-execute-action';

export interface ConfirmationPanelProps {
  result: ActionResult;
  onConfirm: () => void;
  onCancel: () => void;
  isLoading: boolean;
}

export function ConfirmationPanel({
  result,
  onConfirm,
  onCancel,
  isLoading,
}: ConfirmationPanelProps) {
  const t = useTranslations('commandBar');

  return (
    <div className="px-4 py-3" role="alertdialog" aria-label={t('needsConfirmation')}>
      <p className="text-sm font-medium text-slate-900">{t('needsConfirmation')}</p>
      <p className="mt-1 text-xs text-slate-600">{result.message}</p>

      {result.extractedParams && Object.keys(result.extractedParams).length > 0 && (
        <div className="mt-3 rounded-lg border border-slate-200 bg-slate-50 p-3">
          <dl className="space-y-1">
            {Object.entries(result.extractedParams).map(([key, value]) => (
              <div key={key} className="flex items-baseline gap-2 text-xs">
                <dt className="font-medium text-slate-600">{key}:</dt>
                <dd className="text-slate-900">{String(value)}</dd>
              </div>
            ))}
          </dl>
        </div>
      )}

      <div className="mt-3 flex justify-end gap-2">
        <button
          type="button"
          onClick={onCancel}
          disabled={isLoading}
          className="rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-xs font-medium text-slate-700 shadow-sm transition-all hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {t('cancel')}
        </button>
        <button
          type="button"
          onClick={onConfirm}
          disabled={isLoading}
          className="rounded-lg bg-primary-600 px-3 py-1.5 text-xs font-medium text-white shadow-sm transition-all hover:bg-primary-600/90 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isLoading ? t('loading') : t('confirm')}
        </button>
      </div>
    </div>
  );
}
