// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createWorkItemFormSchema, type WorkItemFormData, type WorkItemStatusType, type WorkItemPriorityType, type WorkItemTypeType, type WorkItemEffortType } from '@/lib/schemas';

interface WorkItemFormProps {
  defaultValues?: Partial<WorkItemFormData>;
  onSubmit: (data: WorkItemFormData) => Promise<void>;
  submitLabel: string;
  onCancel: () => void;
  isSubmitting?: boolean;
}

const statusOptions: WorkItemStatusType[] = ['Pending', 'Active', 'Deactivated', 'Expired'];
const priorityOptions: WorkItemPriorityType[] = ['Low', 'Medium', 'High', 'Critical'];
const typeOptions: WorkItemTypeType[] = ['Task', 'Bug', 'Feature', 'Improvement'];
const effortOptions: WorkItemEffortType[] = ['XS', 'S', 'M', 'L', 'XL'];

export function WorkItemForm({ defaultValues, onSubmit, submitLabel, onCancel, isSubmitting }: WorkItemFormProps) {
  const t = useTranslations('workItems');

  const schema = useMemo(
    () =>
      createWorkItemFormSchema({
        titleRequired: t('form.validation.titleRequired'),
        titleMaxLength: t('form.validation.titleMaxLength'),
      }),
    [t]
  );

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<WorkItemFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      title: defaultValues?.title ?? '',
      description: defaultValues?.description ?? '',
      status: defaultValues?.status ?? 'Pending',
      priority: defaultValues?.priority ?? 'Medium',
      type: defaultValues?.type ?? 'Task',
      dueDate: defaultValues?.dueDate ?? '',
      estimatedEffort: defaultValues?.estimatedEffort ?? 'M',
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
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

      <div>
        <label htmlFor="description" className="mb-1.5 block text-sm font-medium text-slate-700">
          {t('form.descriptionLabel')}
        </label>
        <textarea
          id="description"
          {...register('description')}
          placeholder={t('form.descriptionPlaceholder')}
          rows={4}
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          maxLength={2000}
        />
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="priority" className="mb-1.5 block text-sm font-medium text-slate-700">
            {t('form.priorityLabel')}
          </label>
          <select
            id="priority"
            {...register('priority')}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          >
            {priorityOptions.map((p) => (
              <option key={p} value={p}>
                {t(`priority.${p}`)}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="type" className="mb-1.5 block text-sm font-medium text-slate-700">
            {t('form.typeLabel')}
          </label>
          <select
            id="type"
            {...register('type')}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          >
            {typeOptions.map((tp) => (
              <option key={tp} value={tp}>
                {t(`type.${tp}`)}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="dueDate" className="mb-1.5 block text-sm font-medium text-slate-700">
            {t('form.dueDateLabel')}
          </label>
          <input
            id="dueDate"
            type="date"
            {...register('dueDate')}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>

        <div>
          <label htmlFor="estimatedEffort" className="mb-1.5 block text-sm font-medium text-slate-700">
            {t('form.effortLabel')}
          </label>
          <select
            id="estimatedEffort"
            {...register('estimatedEffort')}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          >
            {effortOptions.map((e) => (
              <option key={e} value={e}>
                {t(`effort.${e}`)}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div>
        <label htmlFor="status" className="mb-1.5 block text-sm font-medium text-slate-700">
          {t('form.statusLabel')}
        </label>
        <select
          id="status"
          {...register('status')}
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
        >
          {statusOptions.map((s) => (
            <option key={s} value={s}>
              {t(`status.${s}`)}
            </option>
          ))}
        </select>
      </div>

      <div className="flex items-center justify-end gap-3 border-t border-slate-200 pt-6">
        <button
          type="button"
          onClick={onCancel}
          className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
        >
          {t('actions.cancel')}
        </button>
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] disabled:opacity-50"
        >
          {isSubmitting ? '...' : submitLabel}
        </button>
      </div>
    </form>
  );
}

export type { WorkItemFormData };
