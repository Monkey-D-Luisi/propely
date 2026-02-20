// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMemo } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormField, FormTextarea, FormSubmitButton, FormError } from '@/components/ui/form';
import { Button } from '@/components/ui/button';
import { RoleBadge } from '@/components/orgs/RoleBadge';
import { DeleteOrgSection } from '@/components/orgs/DeleteOrgSection';
import { Skeleton } from '@/components/ui/skeleton';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';
import { useOrg, useUpdateOrg } from '@/hooks/orgs';
import { createUpdateOrgFormSchema, type UpdateOrgFormData } from '@/lib/schemas';
import { isApiError } from '@/lib/api';
import { isManager } from '@/lib/roles';
import { Link } from '@/i18n/navigation';

interface OrgSettingsFormProps {
  orgId: string;
}

export function OrgSettingsForm({ orgId }: OrgSettingsFormProps) {
  const t = useTranslations('orgs');
  const tErrors = useTranslations('errors');
  const tCommon = useTranslations('common');
  const { org, isLoading, error } = useOrg(orgId);
  const updateOrg = useUpdateOrg(orgId);
  const { toast } = useToast();
  const router = useRouter();

  const schema = useMemo(
    () =>
      createUpdateOrgFormSchema({
        nameRequired: t('validation.orgNameRequired'),
      }),
    [t]
  );

  const methods = useForm<UpdateOrgFormData>({
    resolver: zodResolver(schema),
    values: org
      ? { name: org.name, description: org.description ?? '' }
      : undefined,
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
          <div className="p-6 md:p-8">
            <div className="mb-8 border-b border-slate-100 pb-6">
              <Skeleton className="h-7 w-48" />
              <Skeleton className="mt-1 h-4 w-64" />
            </div>
            <div className="flex flex-col gap-4">
              <div>
                <Skeleton className="mb-1.5 h-4 w-32" />
                <Skeleton className="h-10 w-full" />
              </div>
              <div>
                <Skeleton className="mb-1.5 h-4 w-40" />
                <Skeleton className="h-20 w-full" />
              </div>
              <div className="flex items-center justify-end border-t border-slate-100 pt-4">
                <Skeleton className="h-10 w-32" />
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    if (isApiError(error) && error.status === 403) {
      return (
        <div className="mx-auto mt-10 max-w-md space-y-4 text-center">
          <h1 className="text-xl font-semibold text-slate-900">{tErrors('insufficientPermissions.title')}</h1>
          <p className="text-sm text-slate-600">{t('settings.forbidden')}</p>
          <Button type="button" onClick={() => router.push('/orgs/mine')}>
            {tCommon('backToOrgs')}
          </Button>
        </div>
      );
    }

    return <ErrorMessage message={t('settings.loadError')} />;
  }

  if (!org) return null;

  const canEdit = isManager(org.role);

  const onSubmit = async (data: UpdateOrgFormData) => {
    methods.clearErrors('root');
    try {
      await updateOrg({
        name: data.name,
        description: data.description || undefined,
      });
      toast({
        title: t('settings.successTitle'),
        description: t('settings.successDescription'),
        variant: 'success',
      });
    } catch (err) {
      const is409 = isApiError(err) && err.status === 409;
      const message = is409 ? t('settings.nameAlreadyExists') : (err instanceof Error ? err.message : t('settings.errorDescription'));
      methods.setError('root', { message });
      toast({
        title: t('settings.errorTitle'),
        description: message,
        variant: 'destructive',
      });
    }
  };

  return (
    <div className="space-y-6">
      <section className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="p-6 md:p-8">
          <div className="mb-8 border-b border-slate-100 pb-6">
            <div className="flex items-center gap-3">
              <h1 className="text-lg font-semibold text-slate-900">{t('settings.title')}</h1>
              {org.role ? <RoleBadge role={org.role} /> : null}
            </div>
            <p className="mt-1 text-sm text-slate-500">{t('settings.description')}</p>
          </div>

          <FormProvider {...methods}>
            <form className="flex flex-col gap-4" noValidate onSubmit={methods.handleSubmit(onSubmit)}>
              <FormField<UpdateOrgFormData>
                name="name"
                label={t('settings.nameLabel')}
                disabled={!canEdit}
              />

              <FormTextarea<UpdateOrgFormData>
                name="description"
                label={t('settings.descriptionLabel')}
                placeholder={t('settings.descriptionPlaceholder')}
                rows={3}
                disabled={!canEdit}
              />

              <FormError message={methods.formState.errors.root?.message} />

              {canEdit ? (
                <div className="flex items-center justify-end border-t border-slate-100 pt-4">
                  <FormSubmitButton loadingText={t('settings.saving')}>
                    {t('settings.save')}
                  </FormSubmitButton>
                </div>
              ) : null}
            </form>
          </FormProvider>
        </div>
      </section>

      {org.role === 'owner' ? (
        <DeleteOrgSection orgId={orgId} orgName={org.name} />
      ) : null}

      <div className="flex justify-end">
        <Link
          href={`/orgs/${orgId}/members`}
          className="text-sm font-medium text-slate-500 transition hover:text-slate-700"
        >
          {t('settings.backToMembers')}
        </Link>
      </div>
    </div>
  );
}
