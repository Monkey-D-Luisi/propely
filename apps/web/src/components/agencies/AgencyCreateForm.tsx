// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useRouter } from '@/i18n/navigation';
import { Button } from '@/components/ui/button';
import { FormField, FormSubmitButton, FormError } from '@/components/ui/form';
import { useToast } from '@/components/ui/toast';
import { useCreateAgency } from '@/hooks/agencies';
import { isApiError } from '@/lib/api';
import { createAgencyFormSchema, type CreateAgencyFormData } from '@/lib/schemas';

function generateSlug(name: string): string {
  return name
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9\s-]/g, '')
    .replace(/\s+/g, '-')
    .replace(/-+/g, '-')
    .replace(/^-|-$/g, '');
}

export function AgencyCreateForm() {
  const t = useTranslations('agencies');
  const tCommon = useTranslations('common');
  const { toast } = useToast();
  const router = useRouter();
  const createAgency = useCreateAgency();
  const [slugManuallyEdited, setSlugManuallyEdited] = useState(false);

  const schema = useMemo(
    () =>
      createAgencyFormSchema({
        nameRequired: t('validation.nameRequired'),
        nameMaxLength: t('validation.nameMaxLength'),
        slugRequired: t('validation.slugRequired'),
        slugMinLength: t('validation.slugMinLength'),
        slugMaxLength: t('validation.slugMaxLength'),
        slugFormat: t('validation.slugFormat'),
      }),
    [t],
  );

  const methods = useForm<CreateAgencyFormData>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', slug: '' },
  });

  const onNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!slugManuallyEdited) {
      methods.setValue('slug', generateSlug(e.target.value));
    }
  };

  const onSlugChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSlugManuallyEdited(true);
    methods.setValue('slug', e.target.value.toLowerCase());
  };

  const onSubmit = async (data: CreateAgencyFormData) => {
    try {
      const result = await createAgency(data.name, data.slug);
      toast({
        title: t('create.successTitle'),
        description: t('create.successDescription'),
      });
      router.push(`/agencies/${result.id}`);
    } catch (err) {
      if (isApiError(err) && err.status === 409) {
        methods.setError('root', { message: t('create.slugAlreadyExists') });
      } else {
        methods.setError('root', { message: t('create.errorDescription') });
      }
    }
  };

  return (
    <div className="mx-auto w-full max-w-md px-4 py-12">
      <h1 className="mb-2 text-3xl font-bold tracking-tight text-slate-900">
        {t('create.title')}
      </h1>
      <p className="mb-8 text-slate-500">{t('create.description')}</p>

      <FormProvider {...methods}>
        <form
          className="space-y-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
          noValidate
          onSubmit={methods.handleSubmit(onSubmit)}
        >
          <div>
            <label htmlFor="name" className="mb-1.5 block text-sm font-medium text-slate-700">
              {t('create.nameLabel')}
            </label>
            <input
              id="name"
              type="text"
              {...methods.register('name', {
                onChange: onNameChange,
              })}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:ring-2 focus:ring-primary-600"
              placeholder={t('create.namePlaceholder')}
              autoFocus
            />
            {methods.formState.errors.name && (
              <p className="mt-1 text-sm text-red-600">{methods.formState.errors.name.message}</p>
            )}
          </div>

          <div>
            <label htmlFor="slug" className="mb-1.5 block text-sm font-medium text-slate-700">
              {t('create.slugLabel')}
            </label>
            <input
              id="slug"
              type="text"
              {...methods.register('slug', {
                onChange: onSlugChange,
              })}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:ring-2 focus:ring-primary-600"
              placeholder={t('create.slugPlaceholder')}
            />
            <p className="mt-1 text-xs text-slate-400">{t('create.slugHint')}</p>
            {methods.formState.errors.slug && (
              <p className="mt-1 text-sm text-red-600">{methods.formState.errors.slug.message}</p>
            )}
          </div>

          <FormError message={methods.formState.errors.root?.message} />

          <div className="flex gap-3">
            <FormSubmitButton loadingText={t('create.creatingButton')}>
              {t('create.createButton')}
            </FormSubmitButton>
            <Button type="button" onClick={() => router.push('/orgs/mine')} className="border border-slate-200 bg-white text-slate-700 shadow-none hover:bg-slate-50">
              {tCommon('cancel')}
            </Button>
          </div>
        </form>
      </FormProvider>
    </div>
  );
}
