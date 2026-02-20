// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useEffect, useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormField, FormSubmitButton, FormError } from '@/components/ui/form';
import { useToast } from '@/components/ui/toast';
import { useChangePassword } from '@/hooks/orgs';
import { createChangePasswordFormSchema, type ChangePasswordFormData } from '@/lib/schemas';
import { ensureCsrfToken } from '@/lib/csrf';

export function ChangePasswordForm() {
  const t = useTranslations('profile');
  const changePassword = useChangePassword();
  const { toast } = useToast();
  const [csrfToken, setCsrfToken] = useState<string | null>(null);

  useEffect(() => {
    void ensureCsrfToken().then(setCsrfToken);
  }, []);

  const schema = useMemo(
    () =>
      createChangePasswordFormSchema({
        currentPasswordRequired: t('validation.currentPasswordRequired'),
        newPasswordRequired: t('validation.newPasswordRequired'),
        newPasswordMinLength: t('validation.newPasswordMinLength'),
      }),
    [t]
  );

  const methods = useForm<ChangePasswordFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      currentPassword: '',
      newPassword: '',
    },
  });

  const onSubmit = async (data: ChangePasswordFormData) => {
    methods.clearErrors('root');
    if (!csrfToken) return;

    try {
      await changePassword(
        {
          currentPassword: data.currentPassword,
          newPassword: data.newPassword,
        },
        csrfToken,
      );
      methods.reset();
      toast({
        title: t('passwordSuccessTitle'),
        description: t('passwordSuccessDescription'),
        variant: 'success',
      });
    } catch {
      methods.setError('root', { message: t('passwordErrorDescription') });
      toast({
        title: t('passwordErrorTitle'),
        description: t('passwordErrorDescription'),
        variant: 'destructive',
      });
    }
  };

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <div className="p-6 md:p-8">
        <div className="mb-8 border-b border-slate-100 pb-6">
          <h2 className="text-lg font-semibold text-slate-900">{t('passwordTitle')}</h2>
          <p className="mt-1 text-sm text-slate-500">{t('passwordDescription')}</p>
        </div>

        <FormProvider {...methods}>
          <form className="max-w-2xl space-y-6" noValidate onSubmit={methods.handleSubmit(onSubmit)}>
            <FormField<ChangePasswordFormData>
              name="currentPassword"
              label={t('currentPasswordLabel')}
              type="password"
              autoComplete="current-password"
            />

            <div className="grid grid-cols-1 gap-x-4 gap-y-6 sm:grid-cols-2">
              <FormField<ChangePasswordFormData>
                name="newPassword"
                label={t('newPasswordLabel')}
                type="password"
                autoComplete="new-password"
              />
            </div>

            <FormError message={methods.formState.errors.root?.message} />

            <div className="flex items-center justify-end border-t border-slate-100 pt-4">
              <FormSubmitButton loadingText={t('passwordSaving')} disabled={!csrfToken}>
                {t('passwordSave')}
              </FormSubmitButton>
            </div>
          </form>
        </FormProvider>
      </div>
    </div>
  );
}
