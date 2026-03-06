// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useRouter } from '@/i18n/navigation';
import { Link } from '@/i18n/navigation';
import { FormError } from '@/components/ui/form';
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
    <main className="flex flex-1 justify-center py-8 px-4 sm:px-8">
      <div className="flex flex-col max-w-[800px] w-full gap-6">
        {/* Breadcrumb */}
        <nav className="flex flex-wrap items-center gap-2 text-sm" aria-label="Breadcrumb">
          <Link href="/orgs/mine" className="text-slate-500 hover:text-primary-600 transition-colors font-medium">
            {tCommon('backToOrgs')}
          </Link>
          <span className="material-symbols-outlined text-slate-400 text-[18px]" aria-hidden="true">chevron_right</span>
          <span className="text-slate-900 font-medium">{t('create.title')}</span>
        </nav>

        {/* Page Title */}
        <div>
          <h1 className="text-3xl font-bold leading-tight tracking-tight text-slate-900">
            {t('create.title')}
          </h1>
          <p className="mt-1 text-slate-500">{t('create.description')}</p>
        </div>

        {/* Form Card */}
        <FormProvider {...methods}>
          <form
            className="bg-white rounded-xl shadow-sm border border-slate-200 p-6 sm:p-8"
            noValidate
            onSubmit={methods.handleSubmit(onSubmit)}
          >
            <div className="flex flex-col gap-10">
              {/* Agency Details Section */}
              <section className="flex flex-col gap-6">
                <h2 className="text-lg font-bold text-slate-900 border-b border-slate-100 pb-3">
                  {t('create.sectionDetails')}
                </h2>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="flex flex-col gap-2">
                    <label htmlFor="name" className="text-sm font-semibold text-slate-700">
                      {t('create.nameLabel')} *
                    </label>
                    <input
                      id="name"
                      type="text"
                      {...methods.register('name', {
                        onChange: onNameChange,
                      })}
                      className="form-input w-full rounded-lg border-slate-300 bg-white text-slate-900 focus:border-primary-600 focus:ring-primary-600 h-11 placeholder:text-slate-400"
                      placeholder={t('create.namePlaceholder')}
                      autoFocus
                    />
                    {methods.formState.errors.name && (
                      <p className="text-sm text-red-600">{methods.formState.errors.name.message}</p>
                    )}
                  </div>

                  <div className="flex flex-col gap-2">
                    <label htmlFor="slug" className="text-sm font-semibold text-slate-700">
                      {t('create.slugLabel')} *
                    </label>
                    <div className="flex rounded-lg shadow-sm">
                      <span className="inline-flex items-center px-3 rounded-l-lg border border-r-0 border-slate-300 bg-slate-50 text-slate-500 text-sm">
                        propely.app/
                      </span>
                      <input
                        id="slug"
                        type="text"
                        {...methods.register('slug', {
                          onChange: onSlugChange,
                        })}
                        className="form-input flex-1 min-w-0 block w-full rounded-none rounded-r-lg border-slate-300 bg-white text-slate-900 focus:border-primary-600 focus:ring-primary-600 h-11 placeholder:text-slate-400"
                        placeholder={t('create.slugPlaceholder')}
                      />
                    </div>
                    <p className="text-xs text-slate-400">{t('create.slugHint')}</p>
                    {methods.formState.errors.slug && (
                      <p className="text-sm text-red-600">{methods.formState.errors.slug.message}</p>
                    )}
                  </div>
                </div>
              </section>
            </div>

            <FormError message={methods.formState.errors.root?.message} />

            {/* Actions */}
            <div className="flex items-center justify-end gap-4 pt-6 mt-10 border-t border-slate-200">
              <button
                type="button"
                onClick={() => router.push('/orgs/mine')}
                className="px-5 py-2.5 text-sm font-semibold text-slate-600 hover:text-slate-900 transition-colors"
              >
                {tCommon('cancel')}
              </button>
              <button
                type="submit"
                disabled={methods.formState.isSubmitting}
                className="px-6 py-2.5 rounded-xl bg-primary-600 hover:bg-primary-600/90 text-white text-sm font-bold shadow-sm transition-colors disabled:opacity-50"
              >
                {methods.formState.isSubmitting ? t('create.creatingButton') : t('create.createButton')}
              </button>
            </div>
          </form>
        </FormProvider>
      </div>
    </main>
  );
}
