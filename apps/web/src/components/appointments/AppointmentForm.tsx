// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import type { AppointmentType } from '@/hooks/useAppointments';

const appointmentTypes: AppointmentType[] = ['PropertyViewing', 'OwnerMeeting', 'Generic'];

export interface AppointmentFormData {
  title: string;
  type: AppointmentType;
  startDate: string;
  startTime: string;
  endDate: string;
  endTime: string;
  isAllDay: boolean;
  description?: string;
  location?: string;
  propertyId?: string;
  contactId?: string;
  notes?: string;
}

interface AppointmentFormProps {
  defaultValues?: Partial<AppointmentFormData>;
  onSubmit: (data: AppointmentFormData) => Promise<void>;
  submitLabel: string;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export function AppointmentForm({
  defaultValues,
  onSubmit,
  submitLabel,
  onCancel,
  isSubmitting,
}: AppointmentFormProps) {
  const t = useTranslations('appointments');

  const schema = useMemo(
    () =>
      z.object({
        title: z.string().min(1, t('form.validation.titleRequired')).max(200, t('form.validation.titleMaxLength')),
        type: z.enum(['PropertyViewing', 'OwnerMeeting', 'Generic'] as const),
        startDate: z.string().min(1, t('form.validation.startDateRequired')),
        startTime: z.string(),
        endDate: z.string().min(1, t('form.validation.endDateRequired')),
        endTime: z.string(),
        isAllDay: z.boolean(),
        description: z.string().max(2000).optional().or(z.literal('')),
        location: z.string().max(500).optional().or(z.literal('')),
        propertyId: z.string().optional().or(z.literal('')),
        contactId: z.string().optional().or(z.literal('')),
        notes: z.string().max(2000).optional().or(z.literal('')),
      }),
    [t],
  );

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<AppointmentFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      title: defaultValues?.title ?? '',
      type: defaultValues?.type ?? 'Generic',
      startDate: defaultValues?.startDate ?? '',
      startTime: defaultValues?.startTime ?? '09:00',
      endDate: defaultValues?.endDate ?? '',
      endTime: defaultValues?.endTime ?? '10:00',
      isAllDay: defaultValues?.isAllDay ?? false,
      description: defaultValues?.description ?? '',
      location: defaultValues?.location ?? '',
      propertyId: defaultValues?.propertyId ?? '',
      contactId: defaultValues?.contactId ?? '',
      notes: defaultValues?.notes ?? '',
    },
  });

  const isAllDay = watch('isAllDay');

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6" data-testid="appointment-form">
      {/* Title */}
      <div>
        <label htmlFor="title" className="mb-1.5 block text-sm font-medium text-slate-700">
          {t('form.titleLabel')} <span className="text-red-500">*</span>
        </label>
        <input
          id="title"
          type="text"
          {...register('title')}
          placeholder={t('form.titlePlaceholder')}
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          maxLength={200}
        />
        {errors.title && (
          <p className="mt-1 text-sm text-red-600">{errors.title.message}</p>
        )}
      </div>

      {/* Type */}
      <div>
        <label htmlFor="type" className="mb-1.5 block text-sm font-medium text-slate-700">
          {t('form.typeLabel')}
        </label>
        <select
          id="type"
          {...register('type')}
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
        >
          {appointmentTypes.map((tp) => (
            <option key={tp} value={tp}>
              {t(`type.${tp}`)}
            </option>
          ))}
        </select>
      </div>

      {/* All Day Toggle */}
      <div className="flex items-center gap-3">
        <input
          id="isAllDay"
          type="checkbox"
          {...register('isAllDay')}
          className="h-4 w-4 rounded border-slate-300 text-primary-600 focus:ring-primary-600"
        />
        <label htmlFor="isAllDay" className="text-sm font-medium text-slate-700">
          {t('form.allDayLabel')}
        </label>
      </div>

      {/* Date/Time */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="startDate" className="mb-1.5 block text-sm font-medium text-slate-700">
            {t('form.startDateLabel')} <span className="text-red-500">*</span>
          </label>
          <input
            id="startDate"
            type="date"
            {...register('startDate')}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
          {errors.startDate && (
            <p className="mt-1 text-sm text-red-600">{errors.startDate.message}</p>
          )}
        </div>
        {!isAllDay && (
          <div>
            <label htmlFor="startTime" className="mb-1.5 block text-sm font-medium text-slate-700">
              {t('form.startTimeLabel')}
            </label>
            <input
              id="startTime"
              type="time"
              {...register('startTime')}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
            />
          </div>
        )}
        <div>
          <label htmlFor="endDate" className="mb-1.5 block text-sm font-medium text-slate-700">
            {t('form.endDateLabel')} <span className="text-red-500">*</span>
          </label>
          <input
            id="endDate"
            type="date"
            {...register('endDate')}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
          {errors.endDate && (
            <p className="mt-1 text-sm text-red-600">{errors.endDate.message}</p>
          )}
        </div>
        {!isAllDay && (
          <div>
            <label htmlFor="endTime" className="mb-1.5 block text-sm font-medium text-slate-700">
              {t('form.endTimeLabel')}
            </label>
            <input
              id="endTime"
              type="time"
              {...register('endTime')}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
            />
          </div>
        )}
      </div>

      {/* Description */}
      <div>
        <label htmlFor="description" className="mb-1.5 block text-sm font-medium text-slate-700">
          {t('form.descriptionLabel')}
        </label>
        <textarea
          id="description"
          {...register('description')}
          placeholder={t('form.descriptionPlaceholder')}
          rows={3}
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          maxLength={2000}
        />
      </div>

      {/* Location */}
      <div>
        <label htmlFor="location" className="mb-1.5 block text-sm font-medium text-slate-700">
          {t('form.locationLabel')}
        </label>
        <input
          id="location"
          type="text"
          {...register('location')}
          placeholder={t('form.locationPlaceholder')}
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          maxLength={500}
        />
      </div>

      {/* Property ID (optional) */}
      <div>
        <label htmlFor="propertyId" className="mb-1.5 block text-sm font-medium text-slate-700">
          {t('form.propertyLabel')}
        </label>
        <input
          id="propertyId"
          type="text"
          {...register('propertyId')}
          placeholder={t('form.propertyPlaceholder')}
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
        />
      </div>

      {/* Contact ID (optional) */}
      <div>
        <label htmlFor="contactId" className="mb-1.5 block text-sm font-medium text-slate-700">
          {t('form.contactLabel')}
        </label>
        <input
          id="contactId"
          type="text"
          {...register('contactId')}
          placeholder={t('form.contactPlaceholder')}
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
        />
      </div>

      {/* Notes */}
      <div>
        <label htmlFor="notes" className="mb-1.5 block text-sm font-medium text-slate-700">
          {t('form.notesLabel')}
        </label>
        <textarea
          id="notes"
          {...register('notes')}
          placeholder={t('form.notesPlaceholder')}
          rows={2}
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          maxLength={2000}
        />
      </div>

      {/* Actions */}
      <div className="flex items-center justify-end gap-3 border-t border-slate-200 pt-6">
        <button
          type="button"
          onClick={onCancel}
          className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
        >
          {t('form.cancel')}
        </button>
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] disabled:opacity-50 focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
        >
          {isSubmitting ? '...' : submitLabel}
        </button>
      </div>
    </form>
  );
}
