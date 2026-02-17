// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useRef, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { Button } from '@/components/ui/button';
import { DialogOverlay } from '@/components/ui/dialog-overlay';
import { useToast } from '@/components/ui/toast';
import { useDeleteAccount } from '@/hooks/orgs';
import { ensureCsrfToken } from '@/lib/csrf';
import { isApiError } from '@/lib/api';

export function DeleteAccountSection() {
  const t = useTranslations('profile');
  const tCommon = useTranslations('common');
  const router = useRouter();
  const deleteAccount = useDeleteAccount();
  const { toast } = useToast();
  const [csrfToken, setCsrfToken] = useState<string | null>(null);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [password, setPassword] = useState('');
  const [confirmText, setConfirmText] = useState('');

  useEffect(() => {
    void ensureCsrfToken().then(setCsrfToken);
  }, []);

  const triggerRef = useRef<HTMLButtonElement>(null);
  const dialogRef = useRef<HTMLDivElement>(null);

  const isConfirmed = confirmText === 'DELETE' && password.length > 0;

  const closeDialog = () => {
    setIsDialogOpen(false);
    setPassword('');
    setConfirmText('');
    triggerRef.current?.focus();
  };

  const handleDelete = async () => {
    if (!isConfirmed || !csrfToken) return;

    setIsProcessing(true);
    try {
      await deleteAccount({ password }, csrfToken);

      toast({
        title: t('deleteAccount.successTitle'),
        description: t('deleteAccount.successDescription'),
        variant: 'success',
      });
      router.push('/');
    } catch (err) {
      const description = isApiError(err) && err.status === 401
        ? t('deleteAccount.invalidPassword')
        : t('deleteAccount.errorDescription');
      toast({
        title: t('deleteAccount.errorTitle'),
        description,
        variant: 'destructive',
      });
    } finally {
      setIsProcessing(false);
      setIsDialogOpen(false);
      setPassword('');
      setConfirmText('');
    }
  };

  return (
    <section className="overflow-hidden rounded-xl border border-red-200 bg-red-50/50 shadow-sm">
      <div className="p-6 md:p-8">
        <div className="mb-8 border-b border-slate-100 pb-6">
          <h2 className="text-lg font-semibold text-red-900">
            {t('deleteAccount.dangerZone')}
          </h2>
        </div>
        <div className="flex items-start justify-between gap-4">
          <div>
            <h3 className="text-sm font-medium text-red-800">
              {t('deleteAccount.title')}
            </h3>
            <p className="mt-1 text-sm text-red-700">
              {t('deleteAccount.description')}
            </p>
          </div>
          <Button
            ref={triggerRef}
            type="button"
            onClick={() => setIsDialogOpen(true)}
            className="shrink-0 bg-red-600 hover:bg-red-500"
          >
            {t('deleteAccount.button')}
          </Button>
        </div>
      </div>

      {isDialogOpen ? (
        <DialogOverlay
          dialogRef={dialogRef}
          isProcessing={isProcessing}
          onClose={closeDialog}
        >
          <div
            ref={dialogRef}
            role="dialog"
            aria-modal="true"
            aria-labelledby="delete-account-dialog-title"
            className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-xl"
          >
            <h3
              id="delete-account-dialog-title"
              className="text-lg font-semibold text-slate-900"
            >
              {t('deleteAccount.dialogTitle')}
            </h3>
            <p className="mt-2 text-sm text-slate-600">
              {t('deleteAccount.dialogDescription')}
            </p>
            <label
              htmlFor="delete-account-password-input"
              className="mt-4 block text-sm font-medium text-slate-700"
            >
              {t('deleteAccount.passwordLabel')}
            </label>
            <input
              id="delete-account-password-input"
              type="password"
              className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-red-500"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
            />
            <label
              htmlFor="delete-account-confirm-input"
              className="mt-4 block text-sm font-medium text-slate-700"
            >
              {t('deleteAccount.typeDelete')}
            </label>
            <input
              id="delete-account-confirm-input"
              type="text"
              className="mt-1.5 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-red-500"
              value={confirmText}
              onChange={(e) => setConfirmText(e.target.value)}
              autoComplete="off"
            />
            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                className="rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50"
                onClick={() => {
                  if (!isProcessing) closeDialog();
                }}
              >
                {tCommon('cancel')}
              </button>
              <button
                type="button"
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-red-500 disabled:cursor-not-allowed disabled:opacity-60"
                onClick={handleDelete}
                disabled={!isConfirmed || isProcessing}
              >
                {isProcessing
                  ? t('deleteAccount.deleting')
                  : t('deleteAccount.confirmButton')}
              </button>
            </div>
          </div>
        </DialogOverlay>
      ) : null}
    </section>
  );
}
