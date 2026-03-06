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
import { Skeleton } from '@/components/ui/skeleton';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';
import { useAgency, useDeleteAgency } from '@/hooks/agencies';
import { useCurrentUser } from '@/hooks/orgs';
import { isApiError } from '@/lib/api';
import { createUpdateAgencyFormSchema, type UpdateAgencyFormData } from '@/lib/schemas';

interface AgencySettingsFormProps {
  agencyId: string;
}

export function AgencySettingsForm({ agencyId }: AgencySettingsFormProps) {
  const t = useTranslations('agencies');
  const tCommon = useTranslations('common');
  const { toast } = useToast();
  const router = useRouter();
  const { agency, isLoading, error } = useAgency(agencyId);
  const { user } = useCurrentUser();
  const deleteAgency = useDeleteAgency(agencyId);
  const [deleteConfirm, setDeleteConfirm] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);

  const schema = useMemo(
    () =>
      createUpdateAgencyFormSchema({
        nameRequired: t('validation.nameRequired'),
        nameMaxLength: t('validation.nameMaxLength'),
      }),
    [t],
  );

  const methods = useForm<UpdateAgencyFormData>({
    resolver: zodResolver(schema),
    values: agency ? { name: agency.name } : undefined,
  });

  if (isLoading) {
    return (
      <main className="flex-1 w-full max-w-[1024px] mx-auto px-6 lg:px-8 py-8">
        <Skeleton className="mb-2 h-9 w-48" />
        <Skeleton className="mb-8 h-4 w-64" />
        <div className="space-y-6 rounded-2xl border border-slate-200 bg-white p-6 sm:p-8 shadow-sm">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-32" />
        </div>
      </main>
    );
  }

  if (error || !agency) {
    return (
      <main className="flex-1 w-full max-w-[1024px] mx-auto px-6 lg:px-8 py-8">
        <ErrorMessage message={t('settings.loadError')} />
      </main>
    );
  }

  const isOwner = user?.id === agency.createdByUserId;

  const onSubmit = async (_data: UpdateAgencyFormData) => {
    toast({
      title: t('settings.updateNotAvailable'),
    });
  };

  const handleDelete = async () => {
    if (deleteConfirm !== agency.name) return;

    setIsDeleting(true);
    try {
      await deleteAgency();
      toast({
        title: t('settings.deleteSuccessTitle'),
        description: t('settings.deleteSuccessDescription'),
      });
      router.push('/orgs/mine');
    } catch (err) {
      if (isApiError(err) && err.status === 403) {
        toast({
          title: t('settings.deleteErrorTitle'),
          description: t('settings.deleteForbidden'),
          variant: 'destructive',
        });
      } else {
        toast({
          title: t('settings.deleteErrorTitle'),
          description: t('settings.deleteErrorDescription'),
          variant: 'destructive',
        });
      }
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <main className="flex-1 w-full max-w-[1024px] mx-auto px-6 lg:px-8 py-8">
      {/* Breadcrumb */}
      <nav className="flex flex-wrap items-center gap-2 mb-8 text-sm" aria-label="Breadcrumb">
        <Link href="/orgs/mine" className="text-slate-500 hover:text-primary-600 transition-colors">
          {tCommon('backToOrgs')}
        </Link>
        <span className="material-symbols-outlined text-slate-400 text-[16px]" aria-hidden="true">chevron_right</span>
        <Link href={`/agencies/${agencyId}`} className="text-slate-500 hover:text-primary-600 transition-colors">
          {agency.name}
        </Link>
        <span className="material-symbols-outlined text-slate-400 text-[16px]" aria-hidden="true">chevron_right</span>
        <span className="text-slate-900 font-medium">{t('settings.title')}</span>
      </nav>

      {/* Page Title */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-slate-900 mb-2">
          {t('settings.title')}
        </h1>
        <p className="text-slate-600">{t('settings.description')}</p>
      </div>

      <div className="space-y-8">
        {/* Basic Info Card */}
        <section className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
          <FormProvider {...methods}>
            <form noValidate onSubmit={methods.handleSubmit(onSubmit)}>
              <div className="p-6 md:p-8">
                <h2 className="text-xl font-bold text-slate-900 mb-6">
                  {t('settings.sectionGeneral')}
                </h2>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-2">
                    <label htmlFor="agency-name" className="block text-sm font-medium text-slate-700">
                      {t('settings.nameLabel')}
                    </label>
                    <input
                      id="agency-name"
                      type="text"
                      {...methods.register('name')}
                      disabled={!isOwner}
                      className="w-full rounded-xl border-slate-300 bg-white text-slate-900 focus:ring-primary-600 focus:border-primary-600 shadow-sm h-11 px-4 disabled:bg-slate-50 disabled:text-slate-500"
                    />
                    {methods.formState.errors.name && (
                      <p className="text-sm text-red-600">{methods.formState.errors.name.message}</p>
                    )}
                  </div>

                  <div className="space-y-2">
                    <label htmlFor="agency-slug" className="block text-sm font-medium text-slate-700">
                      {t('settings.slugLabel')}
                    </label>
                    <input
                      id="agency-slug"
                      type="text"
                      value={agency.slug}
                      disabled
                      className="w-full rounded-xl border-slate-300 bg-slate-50 text-slate-500 shadow-sm h-11 px-4"
                    />
                    <p className="text-xs text-slate-400">{t('settings.slugReadOnly')}</p>
                  </div>
                </div>

                <FormError message={methods.formState.errors.root?.message} />
              </div>

              {isOwner && (
                <div className="bg-slate-50 px-6 py-4 border-t border-slate-200 flex justify-end gap-3">
                  <button
                    type="button"
                    onClick={() => methods.reset()}
                    className="px-5 py-2.5 rounded-xl text-sm font-medium text-slate-700 hover:bg-slate-200 transition-colors"
                  >
                    {tCommon('discard')}
                  </button>
                  <button
                    type="submit"
                    disabled={methods.formState.isSubmitting}
                    className="px-5 py-2.5 rounded-xl text-sm font-medium text-white bg-primary-600 hover:bg-primary-600/90 transition-colors shadow-sm flex items-center gap-2 disabled:opacity-50"
                  >
                    <span className="material-symbols-outlined text-[18px]" aria-hidden="true">save</span>
                    {methods.formState.isSubmitting ? t('settings.savingButton') : t('settings.saveButton')}
                  </button>
                </div>
              )}
            </form>
          </FormProvider>
        </section>

        {/* Danger Zone */}
        {isOwner && (
          <section className="bg-red-50 rounded-2xl border border-red-200 p-6 md:p-8">
            <div className="flex items-start gap-4 mb-6">
              <div className="bg-red-100 p-2 rounded-lg text-red-600">
                <span className="material-symbols-outlined text-[24px]" aria-hidden="true">warning</span>
              </div>
              <div>
                <h2 className="text-xl font-bold text-red-600">{t('settings.dangerZone')}</h2>
                <p className="text-slate-700 text-sm mt-1 max-w-2xl">{t('settings.deleteDescription')}</p>
              </div>
            </div>

            <div className="space-y-4">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-4 bg-white rounded-xl border border-red-100">
                <div>
                  <h3 className="font-semibold text-slate-900">{t('settings.deleteAgencyTitle')}</h3>
                  <p className="text-sm text-slate-500 mt-1">
                    {t('settings.typeNameToConfirm', { name: agency.name })}
                  </p>
                  <input
                    id="delete-confirm"
                    type="text"
                    value={deleteConfirm}
                    onChange={(e) => setDeleteConfirm(e.target.value)}
                    className="mt-2 w-full sm:w-64 rounded-xl border-slate-300 bg-white text-slate-900 focus:ring-red-500 focus:border-red-500 shadow-sm h-11 px-4 placeholder:text-slate-400"
                    placeholder={agency.name}
                  />
                </div>
                <button
                  type="button"
                  onClick={() => void handleDelete()}
                  disabled={deleteConfirm !== agency.name || isDeleting}
                  className="shrink-0 px-4 py-2.5 rounded-xl text-sm font-medium text-white bg-red-600 hover:bg-red-700 transition-colors shadow-sm disabled:opacity-50"
                >
                  {isDeleting ? t('settings.deletingButton') : t('settings.deleteButton')}
                </button>
              </div>
            </div>
          </section>
        )}
      </div>
    </main>
  );
}
