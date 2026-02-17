// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useRef, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { Button } from '@/components/ui/button';
import { DialogOverlay } from '@/components/ui/dialog-overlay';
import { useToast } from '@/components/ui/toast';
import { useLeaveOrg } from '@/hooks/orgs';
import type { Member } from '@/lib/schemas';

interface LeaveOrgButtonProps {
  orgId: string;
  members: Member[];
  currentMember: Member | null;
  onAfterLeave?: () => void;
}

export function LeaveOrgButton({ orgId, members, currentMember, onAfterLeave }: LeaveOrgButtonProps) {
  const t = useTranslations('orgs');
  const tCommon = useTranslations('common');
  const router = useRouter();
  const leaveOrg = useLeaveOrg(orgId);
  const { toast } = useToast();
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);

  const triggerRef = useRef<HTMLButtonElement>(null);
  const dialogRef = useRef<HTMLDivElement>(null);

  if (!currentMember) {
    return null;
  }

  const closeDialog = () => {
    setIsDialogOpen(false);
    triggerRef.current?.focus();
  };

  const handleLeave = async () => {
    setIsProcessing(true);
    try {
      const result = await leaveOrg({ members, currentMember });
      if (!result.ok) {
        if (result.reason === 'LAST_OWNER') {
          toast({
            title: t('leave.lastOwnerTitle'),
            description: t('leave.lastOwnerDescription'),
            variant: 'destructive'
          });
        }
        return;
      }

      toast({
        title: t('leave.successTitle'),
        description: t('leave.successDescription'),
        variant: 'success'
      });
      onAfterLeave?.();
      router.push('/orgs/mine');
    } catch {
      toast({
        title: t('leave.errorTitle'),
        description: t('leave.errorDescription'),
        variant: 'destructive'
      });
    } finally {
      setIsProcessing(false);
      setIsDialogOpen(false);
    }
  };

  return (
    <div>
      <Button
        ref={triggerRef}
        type="button"
        onClick={() => setIsDialogOpen(true)}
        className="bg-red-600 hover:bg-red-500"
      >
        {t('leave.button')}
      </Button>

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
            aria-labelledby="leave-dialog-title"
            className="w-full max-w-md rounded-lg border border-slate-200 bg-white p-6 shadow-xl"
          >
            <h3 id="leave-dialog-title" className="text-lg font-semibold text-slate-900">
              {t('leave.dialogTitle')}
            </h3>
            <p className="mt-2 text-sm text-slate-600">
              {t('leave.dialogDescription')}
            </p>
            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                className="rounded-md border border-slate-200 px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50"
                onClick={() => {
                  if (!isProcessing) closeDialog();
                }}
              >
                {tCommon('cancel')}
              </button>
              <button
                type="button"
                className="rounded-md bg-red-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-red-500 disabled:cursor-not-allowed disabled:opacity-60"
                onClick={handleLeave}
                disabled={isProcessing}
              >
                {isProcessing ? t('leave.leaving') : t('leave.confirmButton')}
              </button>
            </div>
          </div>
        </DialogOverlay>
      ) : null}
    </div>
  );
}
