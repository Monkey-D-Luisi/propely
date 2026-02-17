// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useRef, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { Button } from '@/components/ui/button';
import { DialogOverlay } from '@/components/ui/dialog-overlay';
import { useToast } from '@/components/ui/toast';
import { useDeleteOrg } from '@/hooks/orgs';
import { isApiError } from '@/lib/api';

interface DeleteOrgSectionProps {
  orgId: string;
  orgName: string;
}

export function DeleteOrgSection({ orgId, orgName }: DeleteOrgSectionProps) {
  const t = useTranslations('orgs');
  const tCommon = useTranslations('common');
  const router = useRouter();
  const deleteOrg = useDeleteOrg(orgId);
  const { toast } = useToast();
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [confirmText, setConfirmText] = useState('');

  const triggerRef = useRef<HTMLButtonElement>(null);
  const dialogRef = useRef<HTMLDivElement>(null);

  const isConfirmed = confirmText === orgName;

  const closeDialog = () => {
    setIsDialogOpen(false);
    setConfirmText('');
    triggerRef.current?.focus();
  };

  const handleDelete = async () => {
    if (!isConfirmed) return;

    setIsProcessing(true);
    try {
      await deleteOrg();

      toast({
        title: t('deleteOrg.successTitle'),
        description: t('deleteOrg.successDescription'),
        variant: 'success',
      });
      router.push('/orgs/mine');
    } catch (err) {
      const description = isApiError(err) && err.status === 403
        ? t('deleteOrg.forbidden')
        : t('deleteOrg.errorDescription');
      toast({
        title: t('deleteOrg.errorTitle'),
        description,
        variant: 'destructive',
      });
    } finally {
      setIsProcessing(false);
      setIsDialogOpen(false);
      setConfirmText('');
    }
  };

  return (
    <section className="overflow-hidden rounded-xl border border-red-200 bg-red-50/50 shadow-sm">
      <div className="p-6 md:p-8">
        <div className="mb-8 border-b border-slate-100 pb-6">
          <h2 className="text-lg font-semibold text-red-900">
            {t('deleteOrg.dangerZone')}
          </h2>
        </div>
        <div className="flex items-start justify-between gap-4">
          <div>
            <h3 className="text-sm font-medium text-red-800">
              {t('deleteOrg.title')}
            </h3>
            <p className="mt-1 text-sm text-red-700">
              {t('deleteOrg.description')}
            </p>
          </div>
          <Button
            ref={triggerRef}
            type="button"
            onClick={() => setIsDialogOpen(true)}
            className="shrink-0 bg-red-600 hover:bg-red-500"
          >
            {t('deleteOrg.button')}
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
            aria-labelledby="delete-org-dialog-title"
            className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-xl"
          >
            <h3
              id="delete-org-dialog-title"
              className="text-lg font-semibold text-slate-900"
            >
              {t('deleteOrg.dialogTitle')}
            </h3>
            <p className="mt-2 text-sm text-slate-600">
              {t('deleteOrg.dialogDescription')}
            </p>
            <label
              htmlFor="delete-org-confirm-input"
              className="mt-4 block text-sm font-medium text-slate-700"
            >
              {t('deleteOrg.typeName', { name: orgName })}
            </label>
            <input
              id="delete-org-confirm-input"
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
                  ? t('deleteOrg.deleting')
                  : t('deleteOrg.confirmButton')}
              </button>
            </div>
          </div>
        </DialogOverlay>
      ) : null}
    </section>
  );
}
