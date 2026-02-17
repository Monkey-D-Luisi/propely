// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormField, FormSubmitButton, FormError } from '@/components/ui/form';
import { Skeleton } from '@/components/ui/skeleton';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';
import { useCurrentUser, useUpdateProfile } from '@/hooks/orgs';
import { createUpdateProfileFormSchema, type UpdateProfileFormData } from '@/lib/schemas';
import { ensureCsrfToken } from '@/lib/csrf';

export function ProfileForm() {
  const t = useTranslations('profile');
  const { user, isLoading, error } = useCurrentUser();
  const updateProfile = useUpdateProfile();
  const { toast } = useToast();
  const [csrfToken, setCsrfToken] = useState<string | null>(null);

  useEffect(() => {
    void ensureCsrfToken().then(setCsrfToken);
  }, []);

  const schema = useMemo(
    () =>
      createUpdateProfileFormSchema({
        nameMaxLength: t('validation.nameMaxLength'),
      }),
    [t]
  );

  const methods = useForm<UpdateProfileFormData>({
    resolver: zodResolver(schema),
    values: user
      ? { name: user.name ?? '' }
      : undefined,
  });

  if (isLoading) {
    return (
      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="p-6 md:p-8">
          <div className="mb-8 border-b border-slate-100 pb-6">
            <Skeleton className="h-6 w-40" />
            <Skeleton className="mt-1 h-4 w-56" />
          </div>
          <div className="space-y-6">
            <div>
              <Skeleton className="mb-1.5 h-4 w-24" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div>
              <Skeleton className="mb-1.5 h-4 w-20" />
              <Skeleton className="h-10 w-full" />
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !user) {
    return <ErrorMessage message={t('loadError')} />;
  }

  const onSubmit = async (data: UpdateProfileFormData) => {
    methods.clearErrors('root');
    if (!csrfToken) return;

    try {
      await updateProfile(
        { name: data.name || undefined },
        csrfToken,
      );
      toast({
        title: t('successTitle'),
        description: t('successDescription'),
        variant: 'success',
      });
    } catch {
      methods.setError('root', { message: t('errorDescription') });
      toast({
        title: t('errorTitle'),
        description: t('errorDescription'),
        variant: 'destructive',
      });
    }
  };

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <div className="p-6 md:p-8">
        <div className="mb-8 border-b border-slate-100 pb-6">
          <h2 className="text-lg font-semibold text-slate-900">{t('title')}</h2>
          <p className="mt-1 text-sm text-slate-500">{t('description')}</p>
        </div>

        <FormProvider {...methods}>
          <form className="space-y-6" noValidate onSubmit={methods.handleSubmit(onSubmit)}>
            <div className="grid grid-cols-1 gap-x-8 gap-y-6 sm:grid-cols-2">
              <div className="sm:col-span-2">
                <label className="mb-1.5 block text-sm font-medium text-slate-700">
                  {t('emailLabel')}
                </label>
                <div className="relative">
                  <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                    <span className="material-symbols-outlined text-sm text-slate-400" aria-hidden="true">lock</span>
                  </div>
                  <input
                    type="email"
                    value={user.email}
                    disabled
                    className="block w-full cursor-not-allowed rounded-lg border border-slate-200 bg-slate-100 py-2.5 pl-10 pr-3 text-sm text-slate-500"
                  />
                </div>
              </div>

              <div className="sm:col-span-1">
                <FormField<UpdateProfileFormData>
                  name="name"
                  label={t('nameLabel')}
                  autoComplete="name"
                />
              </div>
            </div>

            <FormError message={methods.formState.errors.root?.message} />

            <div className="flex items-center justify-end border-t border-slate-100 pt-4">
              <FormSubmitButton loadingText={t('saving')} disabled={!csrfToken}>
                {t('save')}
              </FormSubmitButton>
            </div>
          </form>
        </FormProvider>
      </div>
    </div>
  );
}
