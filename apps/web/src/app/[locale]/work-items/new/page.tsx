// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { Link } from '@/i18n/navigation';
import { useCreateWorkItem } from '@/hooks/work-items';
import { useActiveOrg } from '@/hooks/use-active-org';
import { WorkItemForm } from '@/components/work-items/WorkItemForm';
import { SmartFill } from '@/components/work-items/SmartFill';
import { useToast } from '@/components/ui/toast';
import type { WorkItemFormData } from '@/lib/schemas';

export default function NewWorkItemPage() {
  const t = useTranslations('workItems');
  const router = useRouter();
  const { toast } = useToast();
  const createWorkItem = useCreateWorkItem();
  useActiveOrg();

  const [isSubmitting, setSubmitting] = useState(false);
  const [smartFillData, setSmartFillData] = useState<Partial<WorkItemFormData> | undefined>(undefined);
  const [formKey, setFormKey] = useState(0);

  const handleSmartFill = (data: Partial<WorkItemFormData>) => {
    setSmartFillData(data);
    setFormKey((k) => k + 1);
  };

  const handleSubmit = async (data: WorkItemFormData) => {
    setSubmitting(true);
    try {
      const created = await createWorkItem({
        title: data.title,
        description: data.description || undefined,
        priority: data.priority || undefined,
        type: data.type || undefined,
        dueDate: data.dueDate || undefined,
        estimatedEffort: data.estimatedEffort || undefined,
      });
      toast({ title: t('actions.create'), variant: 'success' });
      router.push(`/work-items/${created.id}`);
    } catch {
      toast({ title: t('loadError'), variant: 'destructive' });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
      {/* Header */}
      <div className="border-b border-slate-100 pb-6">
        <div className="mb-3 flex items-center gap-2 text-sm text-slate-500">
          <Link href="/work-items" className="hover:text-slate-700">
            {t('breadcrumbs.list')}
          </Link>
          <span aria-hidden="true">/</span>
          <span className="text-slate-900">{t('breadcrumbs.new')}</span>
        </div>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('newWorkItem')}</h1>
      </div>

      {/* Smart Fill */}
      <SmartFill onFill={handleSmartFill} />

      {/* Form */}
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <WorkItemForm
          key={formKey}
          defaultValues={smartFillData}
          onSubmit={handleSubmit}
          submitLabel={t('actions.create')}
          onCancel={() => router.push('/work-items')}
          isSubmitting={isSubmitting}
        />
      </div>
    </div>
  );
}
