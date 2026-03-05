// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useRef } from 'react';
import { useTranslations } from 'next-intl';
import { DialogOverlay } from '@/components/ui/dialog-overlay';

interface StatusChangeDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  fromStatus: string;
  toStatus: string;
  isLoading: boolean;
}

export function StatusChangeDialog({
  isOpen,
  onClose,
  onConfirm,
  fromStatus,
  toStatus,
  isLoading,
}: StatusChangeDialogProps) {
  const t = useTranslations('properties.statusChange');
  const tStatus = useTranslations('properties.status');
  const tCommon = useTranslations('common');
  const dialogRef = useRef<HTMLDivElement>(null);

  if (!isOpen) return null;

  return (
    <DialogOverlay dialogRef={dialogRef} isProcessing={isLoading} onClose={onClose}>
      <div
        ref={dialogRef}
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="status-change-dialog-title"
        aria-describedby="status-change-dialog-desc"
        className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-6 shadow-lg"
        data-testid="status-change-dialog"
      >
        <h2
          id="status-change-dialog-title"
          className="text-lg font-semibold text-slate-900"
        >
          {t('dialogTitle')}
        </h2>
        <p
          id="status-change-dialog-desc"
          className="mt-2 text-sm text-slate-500"
        >
          {t('dialogMessage', {
            from: tStatus(fromStatus as never),
            to: tStatus(toStatus as never),
          })}
        </p>
        <div className="mt-6 flex justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            disabled={isLoading}
            className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50"
            data-testid="status-change-cancel"
          >
            {t('cancel')}
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={isLoading}
            className="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] disabled:opacity-50"
            data-testid="status-change-confirm"
          >
            {isLoading ? tCommon('loading') : t('confirm')}
          </button>
        </div>
      </div>
    </DialogOverlay>
  );
}
