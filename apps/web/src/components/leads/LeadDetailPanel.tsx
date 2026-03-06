// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useLead } from '@/hooks/useLead';
import { LeadStatusBadge } from './LeadStatusBadge';

interface LeadDetailPanelProps {
  leadId: string;
  onClose: () => void;
  onConvert: (id: string) => void;
}

function formatDate(dateStr: string | null | undefined): string {
  if (!dateStr) return '\u2014';
  return new Intl.DateTimeFormat('es-ES', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(dateStr));
}

export function LeadDetailPanel({ leadId, onClose, onConvert }: LeadDetailPanelProps) {
  const t = useTranslations('leads.detail');
  const tLeads = useTranslations('leads');
  const panelRef = useRef<HTMLDivElement>(null);

  const { lead, isLoading } = useLead(leadId);

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [onClose]);

  return (
    <div
      ref={panelRef}
      role="dialog"
      aria-modal="true"
      aria-label={t('title')}
      data-testid="lead-detail-panel"
      className="fixed inset-y-0 right-0 z-40 flex w-full max-w-md flex-col border-l border-slate-200 bg-white shadow-xl"
    >
      {/* Header */}
      <div className="flex items-center justify-between border-b border-slate-200 px-6 py-4">
        <h2 className="text-lg font-semibold text-slate-900">{t('title')}</h2>
        <button
          type="button"
          onClick={onClose}
          className="rounded-lg p-1.5 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600"
          aria-label={t('close')}
        >
          <span className="material-symbols-outlined text-lg" aria-hidden="true">close</span>
        </button>
      </div>

      {/* Body */}
      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="animate-pulse space-y-4" data-testid="lead-detail-loading">
            <div className="h-6 w-48 rounded-lg bg-slate-200" />
            <div className="h-4 w-64 rounded-lg bg-slate-100" />
            <div className="h-32 rounded-xl bg-slate-100" />
          </div>
        ) : lead ? (
          <div className="space-y-6">
            {/* Lead Name & Status */}
            <div>
              <h3 className="text-xl font-bold text-slate-900" data-testid="lead-detail-name">{lead.name}</h3>
              <div className="mt-2">
                <LeadStatusBadge status={lead.status} label={tLeads(`status.${lead.status}`)} />
              </div>
            </div>

            {/* Details */}
            <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <dl className="space-y-3">
                <div>
                  <dt className="text-xs font-medium text-slate-500">{t('email')}</dt>
                  <dd className="mt-0.5 text-sm text-slate-900">{lead.email}</dd>
                </div>
                {lead.phone && (
                  <div>
                    <dt className="text-xs font-medium text-slate-500">{t('phone')}</dt>
                    <dd className="mt-0.5 text-sm text-slate-900">{lead.phone}</dd>
                  </div>
                )}
                {lead.source && (
                  <div>
                    <dt className="text-xs font-medium text-slate-500">{t('source')}</dt>
                    <dd className="mt-0.5 text-sm text-slate-900">{lead.source}</dd>
                  </div>
                )}
                <div>
                  <dt className="text-xs font-medium text-slate-500">{t('property')}</dt>
                  <dd className="mt-0.5 text-sm">
                    <Link
                      href={`/properties/${lead.propertyId}`}
                      className="text-primary-600 hover:text-primary-700"
                    >
                      {lead.propertyId.slice(0, 8)}...
                    </Link>
                  </dd>
                </div>
                {lead.assignedAgentId && (
                  <div>
                    <dt className="text-xs font-medium text-slate-500">{t('agent')}</dt>
                    <dd className="mt-0.5 text-sm text-slate-900">{lead.assignedAgentId.slice(0, 8)}...</dd>
                  </div>
                )}
                <div>
                  <dt className="text-xs font-medium text-slate-500">{t('created')}</dt>
                  <dd className="mt-0.5 text-sm text-slate-900">{formatDate(lead.createdAtUtc)}</dd>
                </div>
                {lead.updatedAtUtc && (
                  <div>
                    <dt className="text-xs font-medium text-slate-500">{t('updated')}</dt>
                    <dd className="mt-0.5 text-sm text-slate-900">{formatDate(lead.updatedAtUtc)}</dd>
                  </div>
                )}
              </dl>
            </div>

            {/* Message */}
            {lead.message && (
              <div className="rounded-xl border border-slate-200 bg-white p-4">
                <h4 className="mb-2 text-sm font-medium text-slate-500">{t('message')}</h4>
                <p className="text-sm text-slate-900 whitespace-pre-wrap">{lead.message}</p>
              </div>
            )}

            {/* Actions */}
            {lead.status !== 'Converted' && lead.status !== 'Lost' && (
              <button
                type="button"
                onClick={() => onConvert(lead.id)}
                className="w-full rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98]"
              >
                {tLeads('actions.convert')}
              </button>
            )}
          </div>
        ) : (
          <p className="text-sm text-slate-500">{tLeads('loadError')}</p>
        )}
      </div>
    </div>
  );
}
