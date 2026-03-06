// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useCallback, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { useActiveOrg } from '@/hooks/use-active-org';
import { useRequireAuth } from '@/hooks/useRequireAuth';
import { useAppointments, type AppointmentFilters, type AppointmentStatus, type AppointmentType, type AppointmentListItem } from '@/hooks/useAppointments';
import { useCreateAppointment, type CreateAppointmentRequest } from '@/hooks/useCreateAppointment';
import { useDeleteAppointment } from '@/hooks/useDeleteAppointment';
import { useUpdateAppointmentStatus } from '@/hooks/useUpdateAppointmentStatus';
import { CalendarView } from '@/components/appointments/CalendarView';
import { AppointmentDetailPanel } from '@/components/appointments/AppointmentDetailPanel';
import { AppointmentForm, type AppointmentFormData } from '@/components/appointments/AppointmentForm';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';

const statusOptions: AppointmentStatus[] = ['Scheduled', 'Confirmed', 'Completed', 'Cancelled', 'NoShow'];
const typeOptions: AppointmentType[] = ['PropertyViewing', 'OwnerMeeting', 'Generic'];

export default function AppointmentsPage() {
  const t = useTranslations('appointments');
  const tCommon = useTranslations('common');
  const { toast } = useToast();
  const { isLoading: userLoading } = useRequireAuth();
  const { hasOrgs, isLoading: orgsLoading } = useActiveOrg();

  const [statusFilter, setStatusFilter] = useState<AppointmentStatus | ''>('');
  const [typeFilter, setTypeFilter] = useState<AppointmentType | ''>('');
  const [dateRange, setDateRange] = useState<{ from?: string; to?: string }>({});

  const [selectedAppointmentId, setSelectedAppointmentId] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [createFormDefaults, setCreateFormDefaults] = useState<Partial<AppointmentFormData>>({});
  const [isSubmitting, setSubmitting] = useState(false);

  const createAppointment = useCreateAppointment();
  const deleteAppointment = useDeleteAppointment();
  const updateStatus = useUpdateAppointmentStatus();

  const filters: AppointmentFilters = useMemo(() => ({
    status: statusFilter || undefined,
    type: typeFilter || undefined,
    fromUtc: dateRange.from,
    toUtc: dateRange.to,
  }), [statusFilter, typeFilter, dateRange.from, dateRange.to]);

  const { items, isLoading, error, refetch } = useAppointments(filters);

  const handleDatesChange = useCallback((start: Date, end: Date) => {
    setDateRange({
      from: start.toISOString(),
      to: end.toISOString(),
    });
  }, []);

  const handleEventClick = useCallback((appointment: AppointmentListItem) => {
    setSelectedAppointmentId(appointment.id);
    setShowCreateForm(false);
  }, []);

  const handleDateClick = useCallback((date: Date) => {
    const dateStr = date.toISOString().split('T')[0];
    setCreateFormDefaults({
      startDate: dateStr,
      endDate: dateStr,
    });
    setShowCreateForm(true);
    setSelectedAppointmentId(null);
  }, []);

  const handleCreate = useCallback(async (data: AppointmentFormData) => {
    setSubmitting(true);
    try {
      const startTimeUtc = data.isAllDay
        ? `${data.startDate}T00:00:00Z`
        : `${data.startDate}T${data.startTime}:00Z`;
      const endTimeUtc = data.isAllDay
        ? `${data.endDate}T23:59:59Z`
        : `${data.endDate}T${data.endTime}:00Z`;

      const req: CreateAppointmentRequest = {
        title: data.title,
        type: data.type,
        startTimeUtc,
        endTimeUtc,
        isAllDay: data.isAllDay,
        description: data.description || null,
        location: data.location || null,
        propertyId: data.propertyId || null,
        contactId: data.contactId || null,
        notes: data.notes || null,
      };

      await createAppointment(req);
      toast({ title: t('createSuccess'), variant: 'success' });
      setShowCreateForm(false);
      void refetch();
    } catch {
      toast({ title: t('createError'), variant: 'destructive' });
    } finally {
      setSubmitting(false);
    }
  }, [createAppointment, refetch, toast, t]);

  const handleDelete = useCallback(async (id: string) => {
    try {
      await deleteAppointment(id);
      toast({ title: t('deleteSuccess'), variant: 'success' });
      setSelectedAppointmentId(null);
      void refetch();
    } catch {
      toast({ title: t('deleteError'), variant: 'destructive' });
    }
  }, [deleteAppointment, refetch, toast, t]);

  const handleStatusChange = useCallback(async (id: string, status: AppointmentStatus) => {
    try {
      await updateStatus(id, status);
      toast({ title: t('statusChangeSuccess'), variant: 'success' });
      setSelectedAppointmentId(null);
      void refetch();
    } catch {
      toast({ title: t('statusChangeError'), variant: 'destructive' });
    }
  }, [updateStatus, refetch, toast, t]);

  const handleEdit = useCallback((_id: string) => {
    // For now, close detail and open create form with pre-filled data
    // Full edit support would require a separate edit endpoint
    setSelectedAppointmentId(null);
  }, []);

  if (userLoading || orgsLoading) {
    return (
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6 p-6 md:p-10">
        <div className="animate-pulse space-y-4">
          <div className="h-8 w-48 rounded-lg bg-slate-200" />
          <div className="h-4 w-72 rounded-lg bg-slate-100" />
        </div>
      </div>
    );
  }

  if (!hasOrgs) {
    return (
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6 p-6 md:p-10">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-8 text-center shadow-sm">
          <span className="material-symbols-outlined text-4xl text-slate-300" aria-hidden="true">calendar_month</span>
          <p className="mt-3 text-sm text-slate-600">{t('noOrg')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto flex w-full max-w-7xl flex-col gap-6 p-6 md:p-10">
      {/* Header */}
      <div className="flex flex-col gap-4 border-b border-slate-100 pb-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <button
          type="button"
          onClick={() => {
            setCreateFormDefaults({});
            setShowCreateForm(true);
            setSelectedAppointmentId(null);
          }}
          className="inline-flex items-center gap-2 rounded-lg bg-primary-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition-all hover:bg-primary-600/90 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 active:scale-[0.98]"
        >
          <span className="material-symbols-outlined text-lg" aria-hidden="true">add</span>
          {t('newAppointment')}
        </button>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as AppointmentStatus | '')}
          className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          aria-label={t('filters.statusLabel')}
        >
          <option value="">{t('filters.allStatuses')}</option>
          {statusOptions.map((s) => (
            <option key={s} value={s}>{t(`status.${s}`)}</option>
          ))}
        </select>
        <select
          value={typeFilter}
          onChange={(e) => setTypeFilter(e.target.value as AppointmentType | '')}
          className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          aria-label={t('filters.typeLabel')}
        >
          <option value="">{t('filters.allTypes')}</option>
          {typeOptions.map((tp) => (
            <option key={tp} value={tp}>{t(`type.${tp}`)}</option>
          ))}
        </select>
      </div>

      {/* Content */}
      {error ? (
        <ErrorMessage message={t('loadError')} />
      ) : (
        <CalendarView
          appointments={items}
          isLoading={isLoading}
          onEventClick={handleEventClick}
          onDateClick={handleDateClick}
          onDatesChange={handleDatesChange}
        />
      )}

      {/* Detail Panel */}
      {selectedAppointmentId && (
        <AppointmentDetailPanel
          appointmentId={selectedAppointmentId}
          onClose={() => setSelectedAppointmentId(null)}
          onEdit={handleEdit}
          onDelete={(id) => void handleDelete(id)}
          onStatusChange={(id, status) => void handleStatusChange(id, status)}
        />
      )}

      {/* Create Form Modal */}
      {showCreateForm && (
        <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/40 pt-20">
          <div className="relative mx-4 w-full max-w-lg rounded-xl border border-slate-200 bg-white p-6 shadow-xl">
            <div className="mb-6 flex items-center justify-between">
              <h2 className="text-lg font-semibold text-slate-900">{t('newAppointment')}</h2>
              <button
                type="button"
                onClick={() => setShowCreateForm(false)}
                className="rounded-lg p-1.5 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600"
                aria-label={tCommon('close')}
              >
                <span className="material-symbols-outlined text-lg" aria-hidden="true">close</span>
              </button>
            </div>
            <AppointmentForm
              defaultValues={createFormDefaults}
              onSubmit={handleCreate}
              submitLabel={t('form.create')}
              onCancel={() => setShowCreateForm(false)}
              isSubmitting={isSubmitting}
            />
          </div>
        </div>
      )}
    </div>
  );
}
