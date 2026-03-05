// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useRef } from 'react';
import { useTranslations } from 'next-intl';
import { DialogOverlay } from '@/components/ui/dialog-overlay';

interface OverrideConfirmDialogProps {
  permissionName: string;
  userName: string;
  isProcessing: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function OverrideConfirmDialog({
  permissionName,
  userName,
  isProcessing,
  onConfirm,
  onCancel,
}: OverrideConfirmDialogProps) {
  const t = useTranslations('permissions');
  const dialogRef = useRef<HTMLDivElement>(null);

  return (
    <DialogOverlay
      dialogRef={dialogRef}
      isProcessing={isProcessing}
      onClose={onCancel}
    >
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="override-confirm-title"
        className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-xl"
      >
        <h3
          id="override-confirm-title"
          className="text-lg font-semibold text-slate-900"
        >
          {t('confirmDeny.title')}
        </h3>
        <p className="mt-2 text-sm text-slate-600">
          {t('confirmDeny.description', {
            permission: permissionName,
            user: userName,
          })}
        </p>
        <div className="mt-6 flex justify-end gap-3">
          <button
            type="button"
            className="rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50"
            onClick={() => {
              if (!isProcessing) onCancel();
            }}
          >
            {t('confirmDeny.cancel')}
          </button>
          <button
            type="button"
            className="rounded-lg bg-red-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-red-500 disabled:cursor-not-allowed disabled:opacity-60"
            onClick={onConfirm}
            disabled={isProcessing}
          >
            {t('confirmDeny.confirm')}
          </button>
        </div>
      </div>
    </DialogOverlay>
  );
}
