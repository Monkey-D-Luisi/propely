// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useRef } from 'react';
import { useTranslations, useLocale } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useAppointment } from '@/hooks/useAppointment';
import { AppointmentStatusBadge } from './AppointmentStatusBadge';
import { AppointmentTypeBadge } from './AppointmentTypeBadge';
import type { AppointmentStatus } from '@/hooks/useAppointments';

interface AppointmentDetailPanelProps {
  appointmentId: string;
  onClose: () => void;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
  onStatusChange: (id: string, status: AppointmentStatus) => void;
}

function formatDateTime(dateStr: string | null | undefined, locale: string): string {
  if (!dateStr) return '\u2014';
  return new Intl.DateTimeFormat(locale, {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(dateStr));
}

function formatDate(dateStr: string | null | undefined, locale: string): string {
  if (!dateStr) return '\u2014';
  return new Intl.DateTimeFormat(locale, {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  }).format(new Date(dateStr));
}

/** Returns the valid next status transitions for a given status. */
function getAvailableTransitions(status: AppointmentStatus): { status: AppointmentStatus; labelKey: string; className: string }[] {
  switch (status) {
    case 'Scheduled':
      return [
        { status: 'Confirmed', labelKey: 'actions.confirm', className: 'bg-green-600 text-white hover:bg-green-700' },
        { status: 'Cancelled', labelKey: 'actions.cancel', className: 'bg-red-600 text-white hover:bg-red-700' },
      ];
    case 'Confirmed':
      return [
        { status: 'Completed', labelKey: 'actions.complete', className: 'bg-slate-600 text-white hover:bg-slate-700' },
        { status: 'NoShow', labelKey: 'actions.noShow', className: 'bg-amber-600 text-white hover:bg-amber-700' },
        { status: 'Cancelled', labelKey: 'actions.cancel', className: 'bg-red-600 text-white hover:bg-red-700' },
      ];
    default:
      return [];
  }
}

export function AppointmentDetailPanel({
  appointmentId,
  onClose,
  onEdit,
  onDelete,
  onStatusChange,
}: AppointmentDetailPanelProps) {
  const t = useTranslations('appointments.detail');
  const tAppt = useTranslations('appointments');
  const locale = useLocale();
  const panelRef = useRef<HTMLDivElement>(null);

  const { appointment, isLoading } = useAppointment(appointmentId);

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [onClose]);

  const transitions = appointment ? getAvailableTransitions(appointment.status) : [];

  return (
    <div
      ref={panelRef}
      role="dialog"
      aria-modal="true"
      aria-label={t('title')}
      data-testid="appointment-detail-panel"
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
          <div className="animate-pulse space-y-4" data-testid="appointment-detail-loading">
            <div className="h-6 w-48 rounded-lg bg-slate-200" />
            <div className="h-4 w-64 rounded-lg bg-slate-100" />
            <div className="h-32 rounded-xl bg-slate-100" />
          </div>
        ) : appointment ? (
          <div className="space-y-6">
            {/* Title & Badges */}
            <div>
              <h3 className="text-xl font-bold text-slate-900" data-testid="appointment-detail-title">
                {appointment.title}
              </h3>
              <div className="mt-2 flex flex-wrap gap-2">
                <AppointmentStatusBadge
                  status={appointment.status}
                  label={tAppt(`status.${appointment.status}`)}
                />
                <AppointmentTypeBadge
                  type={appointment.type}
                  label={tAppt(`type.${appointment.type}`)}
                />
              </div>
            </div>

            {/* Details */}
            <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <dl className="space-y-3">
                <div>
                  <dt className="text-xs font-medium text-slate-500">{t('dateTime')}</dt>
                  <dd className="mt-0.5 text-sm text-slate-900">
                    {appointment.isAllDay
                      ? formatDate(appointment.startTimeUtc, locale)
                      : `${formatDateTime(appointment.startTimeUtc, locale)} - ${formatDateTime(appointment.endTimeUtc, locale)}`}
                  </dd>
                </div>
                {appointment.location && (
                  <div>
                    <dt className="text-xs font-medium text-slate-500">{t('location')}</dt>
                    <dd className="mt-0.5 text-sm text-slate-900">{appointment.location}</dd>
                  </div>
                )}
                {appointment.propertyId && (
                  <div>
                    <dt className="text-xs font-medium text-slate-500">{t('property')}</dt>
                    <dd className="mt-0.5 text-sm">
                      <Link
                        href={`/properties/${appointment.propertyId}`}
                        className="text-primary-600 hover:text-primary-700"
                      >
                        {appointment.propertyId.slice(0, 8)}...
                      </Link>
                    </dd>
                  </div>
                )}
                {appointment.contactId && (
                  <div>
                    <dt className="text-xs font-medium text-slate-500">{t('contact')}</dt>
                    <dd className="mt-0.5 text-sm">
                      <Link
                        href={`/contacts/${appointment.contactId}`}
                        className="text-primary-600 hover:text-primary-700"
                      >
                        {appointment.contactId.slice(0, 8)}...
                      </Link>
                    </dd>
                  </div>
                )}
                <div>
                  <dt className="text-xs font-medium text-slate-500">{t('agent')}</dt>
                  <dd className="mt-0.5 text-sm text-slate-900">{appointment.agentId.slice(0, 8)}...</dd>
                </div>
                <div>
                  <dt className="text-xs font-medium text-slate-500">{t('created')}</dt>
                  <dd className="mt-0.5 text-sm text-slate-900">{formatDate(appointment.createdAtUtc, locale)}</dd>
                </div>
                {appointment.updatedAtUtc && (
                  <div>
                    <dt className="text-xs font-medium text-slate-500">{t('updated')}</dt>
                    <dd className="mt-0.5 text-sm text-slate-900">{formatDate(appointment.updatedAtUtc, locale)}</dd>
                  </div>
                )}
              </dl>
            </div>

            {/* Description */}
            {appointment.description && (
              <div className="rounded-xl border border-slate-200 bg-white p-4">
                <h4 className="mb-2 text-sm font-medium text-slate-500">{t('description')}</h4>
                <p className="text-sm text-slate-900 whitespace-pre-wrap">{appointment.description}</p>
              </div>
            )}

            {/* Notes */}
            {appointment.notes && (
              <div className="rounded-xl border border-slate-200 bg-white p-4">
                <h4 className="mb-2 text-sm font-medium text-slate-500">{t('notes')}</h4>
                <p className="text-sm text-slate-900 whitespace-pre-wrap">{appointment.notes}</p>
              </div>
            )}

            {/* Cancellation Reason */}
            {appointment.cancellationReason && (
              <div className="rounded-xl border border-red-200 bg-red-50 p-4">
                <h4 className="mb-2 text-sm font-medium text-red-600">{t('cancellationReason')}</h4>
                <p className="text-sm text-red-900 whitespace-pre-wrap">{appointment.cancellationReason}</p>
              </div>
            )}

            {/* Status Transitions */}
            {transitions.length > 0 && (
              <div className="space-y-2">
                {transitions.map((tr) => (
                  <button
                    key={tr.status}
                    type="button"
                    onClick={() => onStatusChange(appointment.id, tr.status)}
                    className={`w-full rounded-lg px-4 py-2 text-sm font-medium shadow-sm transition active:scale-[0.98] ${tr.className}`}
                  >
                    {tAppt(tr.labelKey)}
                  </button>
                ))}
              </div>
            )}

            {/* Edit & Delete */}
            <div className="flex items-center gap-3 border-t border-slate-200 pt-4">
              <button
                type="button"
                onClick={() => onEdit(appointment.id)}
                className="flex-1 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
              >
                {tAppt('actions.edit')}
              </button>
              <button
                type="button"
                onClick={() => onDelete(appointment.id)}
                className="flex-1 rounded-lg border border-red-200 bg-white px-4 py-2 text-sm font-medium text-red-600 transition hover:bg-red-50"
              >
                {tAppt('actions.delete')}
              </button>
            </div>
          </div>
        ) : (
          <p className="text-sm text-slate-500">{tAppt('loadError')}</p>
        )}
      </div>
    </div>
  );
}
