// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { Link } from '@/i18n/navigation';
import { useWorkItem, useUpdateWorkItem } from '@/hooks/work-items';
import { useActiveOrg } from '@/hooks/use-active-org';
import { WorkItemForm } from './WorkItemForm';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';
import { WorkItemStatusEnum, WorkItemPriorityEnum, WorkItemTypeEnum, WorkItemEffortEnum, type WorkItemFormData } from '@/lib/schemas';

interface WorkItemEditProps {
  id: string;
}

export function WorkItemEdit({ id }: WorkItemEditProps) {
  const t = useTranslations('workItems');
  const router = useRouter();
  const { toast } = useToast();
  useActiveOrg();
  const { item, isLoading, error } = useWorkItem(id);
  const updateWorkItem = useUpdateWorkItem();

  const [isSubmitting, setSubmitting] = useState(false);

  const handleSubmit = async (data: WorkItemFormData) => {
    setSubmitting(true);
    try {
      await updateWorkItem(id, {
        title: data.title,
        description: data.description || undefined,
        status: data.status,
        priority: data.priority || undefined,
        type: data.type || undefined,
        dueDate: data.dueDate || undefined,
        estimatedEffort: data.estimatedEffort || undefined,
      });
      toast({ title: t('actions.save'), variant: 'success' });
      router.push(`/work-items/${id}`);
    } catch {
      toast({ title: t('loadError'), variant: 'destructive' });
    } finally {
      setSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-6">
        <div className="h-8 w-64 rounded-lg bg-slate-200" />
        <div className="h-4 w-40 rounded-lg bg-slate-100" />
        <div className="h-64 rounded-xl bg-slate-100" />
      </div>
    );
  }

  if (error || !item) {
    return <ErrorMessage message={t('loadError')} />;
  }

  return (
    <>
      {/* Breadcrumbs */}
      <div className="flex items-center gap-2 text-sm text-slate-500">
        <Link href="/work-items" className="hover:text-slate-700">
          {t('breadcrumbs.list')}
        </Link>
        <span aria-hidden="true">/</span>
        <Link href={`/work-items/${id}`} className="hover:text-slate-700">
          {item.title}
        </Link>
        <span aria-hidden="true">/</span>
        <span className="text-slate-900">{t('breadcrumbs.edit')}</span>
      </div>

      {/* Header */}
      <div className="border-b border-slate-100 pb-6">
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('editWorkItem')}</h1>
      </div>

      {/* Form */}
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <WorkItemForm
          defaultValues={{
            title: item.title,
            description: item.description ?? '',
            status: WorkItemStatusEnum.catch('Pending').parse(item.status),
            priority: item.priority ? WorkItemPriorityEnum.catch('Medium').parse(item.priority) : 'Medium',
            type: item.type ? WorkItemTypeEnum.catch('Task').parse(item.type) : 'Task',
            dueDate: item.dueDateUtc ? item.dueDateUtc.split('T')[0] : '',
            estimatedEffort: item.estimatedEffort ? WorkItemEffortEnum.catch('M').parse(item.estimatedEffort) : 'M',
          }}
          onSubmit={handleSubmit}
          submitLabel={t('actions.save')}
          onCancel={() => router.push(`/work-items/${id}`)}
          isSubmitting={isSubmitting}
        />
      </div>
    </>
  );
}
