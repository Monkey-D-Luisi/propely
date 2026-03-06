// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { DialogOverlay } from '@/components/ui/dialog-overlay';
import type { ContactRole } from '@/hooks/useContacts';

interface LeadConversionModalProps {
  leadId: string;
  onConvert: (id: string, role: ContactRole, notes?: string) => Promise<void>;
  onClose: () => void;
}

const ALL_ROLES: ContactRole[] = ['Buyer', 'Seller', 'Tenant', 'Landlord', 'Professional'];

export function LeadConversionModal({ leadId, onConvert, onClose }: LeadConversionModalProps) {
  const t = useTranslations('leads.conversion');
  const tRoles = useTranslations('contacts.role');
  const dialogRef = useRef<HTMLDivElement>(null);
  const [role, setRole] = useState<ContactRole>('Buyer');
  const [notes, setNotes] = useState('');
  const [isConverting, setConverting] = useState(false);

  const handleConvert = async () => {
    setConverting(true);
    try {
      await onConvert(leadId, role, notes.trim() || undefined);
    } finally {
      setConverting(false);
    }
  };

  return (
    <DialogOverlay dialogRef={dialogRef} isProcessing={isConverting} onClose={onClose}>
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="convert-dialog-title"
        aria-describedby="convert-dialog-desc"
        data-testid="lead-conversion-modal"
        className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-lg"
      >
        <h2 id="convert-dialog-title" className="text-lg font-semibold text-slate-900">{t('title')}</h2>
        <p id="convert-dialog-desc" className="mt-2 text-sm text-slate-500">{t('description')}</p>

        <div className="mt-4 space-y-4">
          {/* Role */}
          <div>
            <label htmlFor="convert-role" className="mb-1 block text-sm font-medium text-slate-700">{t('role')}</label>
            <select
              id="convert-role"
              value={role}
              onChange={(e) => setRole(e.target.value as ContactRole)}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
            >
              {ALL_ROLES.map((r) => (
                <option key={r} value={r}>{tRoles(r)}</option>
              ))}
            </select>
          </div>

          {/* Notes */}
          <div>
            <label htmlFor="convert-notes" className="mb-1 block text-sm font-medium text-slate-700">{t('notes')}</label>
            <textarea
              id="convert-notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder={t('notesPlaceholder')}
              rows={3}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
            />
          </div>
        </div>

        <div className="mt-6 flex justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            disabled={isConverting}
            className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50"
          >
            {t('cancel')}
          </button>
          <button
            type="button"
            onClick={() => void handleConvert()}
            disabled={isConverting}
            className="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] disabled:opacity-50"
          >
            {isConverting ? t('converting') : t('convert')}
          </button>
        </div>
      </div>
    </DialogOverlay>
  );
}
