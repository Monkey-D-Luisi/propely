// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useRouter } from '@/i18n/navigation';
import { Link } from '@/i18n/navigation';
import { Button } from '@/components/ui/button';
import { FormSubmitButton, FormError } from '@/components/ui/form';
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
      <div className="mx-auto w-full max-w-5xl px-4 py-12">
        <Skeleton className="mb-2 h-9 w-48" />
        <Skeleton className="mb-8 h-4 w-64" />
        <div className="space-y-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-32" />
        </div>
      </div>
    );
  }

  if (error || !agency) {
    return (
      <div className="mx-auto w-full max-w-5xl px-4 py-12">
        <ErrorMessage message={t('settings.loadError')} />
      </div>
    );
  }

  const isOwner = user?.id === agency.createdByUserId;

  const onSubmit = async (_data: UpdateAgencyFormData) => {
    // Note: Update agency endpoint not yet available in backend.
    // When the backend adds PATCH /api/agencies/{id}, this form will work.
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
    <div className="mx-auto w-full max-w-5xl px-4 py-12">
      <h1 className="mb-2 text-3xl font-bold tracking-tight text-slate-900">
        {t('settings.title')}
      </h1>
      <p className="mb-8 text-slate-500">{t('settings.description')}</p>

      {/* Settings Form */}
      <FormProvider {...methods}>
        <form
          className="mb-8 space-y-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
          noValidate
          onSubmit={methods.handleSubmit(onSubmit)}
        >
          <div>
            <label htmlFor="agency-name" className="mb-1.5 block text-sm font-medium text-slate-700">
              {t('settings.nameLabel')}
            </label>
            <input
              id="agency-name"
              type="text"
              {...methods.register('name')}
              disabled={!isOwner}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:ring-2 focus:ring-primary-600 disabled:bg-slate-50 disabled:text-slate-500"
            />
            {methods.formState.errors.name && (
              <p className="mt-1 text-sm text-red-600">{methods.formState.errors.name.message}</p>
            )}
          </div>

          <div>
            <label htmlFor="agency-slug" className="mb-1.5 block text-sm font-medium text-slate-700">
              {t('settings.slugLabel')}
            </label>
            <input
              id="agency-slug"
              type="text"
              value={agency.slug}
              disabled
              className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-500"
            />
            <p className="mt-1 text-xs text-slate-400">{t('settings.slugReadOnly')}</p>
          </div>

          <FormError message={methods.formState.errors.root?.message} />

          {isOwner && (
            <FormSubmitButton loadingText={t('settings.savingButton')}>
              {t('settings.saveButton')}
            </FormSubmitButton>
          )}
        </form>
      </FormProvider>

      {/* Danger Zone */}
      {isOwner && (
        <div className="rounded-xl border border-red-200 bg-white p-6 shadow-sm">
          <h2 className="mb-1 text-lg font-semibold text-red-600">{t('settings.dangerZone')}</h2>
          <p className="mb-4 text-sm text-slate-600">{t('settings.deleteDescription')}</p>

          <div className="mb-4">
            <label htmlFor="delete-confirm" className="mb-1.5 block text-sm font-medium text-slate-700">
              {t('settings.typeNameToConfirm', { name: agency.name })}
            </label>
            <input
              id="delete-confirm"
              type="text"
              value={deleteConfirm}
              onChange={(e) => setDeleteConfirm(e.target.value)}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:ring-2 focus:ring-red-500"
              placeholder={agency.name}
            />
          </div>

          <Button
            type="button"
            onClick={() => void handleDelete()}
            disabled={deleteConfirm !== agency.name || isDeleting}
            className="bg-red-600 text-white hover:bg-red-700 focus:ring-red-500 disabled:opacity-50"
          >
            {isDeleting ? t('settings.deletingButton') : t('settings.deleteButton')}
          </Button>
        </div>
      )}

      <div className="mt-8">
        <Link href={`/agencies/${agencyId}`} className="text-sm text-slate-500 hover:text-slate-700">
          ← {t('settings.backToDashboard')}
        </Link>
      </div>
    </div>
  );
}
