// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMemo } from 'react';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormField, FormSelect, FormSubmitButton, FormError } from '@/components/ui/form';
import { useInviteMember } from '@/hooks/orgs';
import { createInviteFormSchema, type InviteFormData, type InviteRole } from '@/lib/schemas';
import { useToast } from '@/components/ui/toast';

interface InviteFormProps {
  orgId: string;
  canInvite: boolean;
  onInvited?: () => void | Promise<void>;
}

export function InviteForm({ orgId, canInvite, onInvited }: InviteFormProps) {
  const t = useTranslations('orgs');
  const inviteMember = useInviteMember(orgId);
  const { toast } = useToast();

  const schema = useMemo(
    () =>
      createInviteFormSchema({
        emailRequired: t('validation.emailRequired'),
        emailInvalid: t('validation.emailInvalid'),
      }),
    [t]
  );

  const methods = useForm<InviteFormData>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', role: 'member' },
  });

  const roleOptions = useMemo<{ label: string; value: InviteRole }[]>(() => [
    { label: t('roles.admin'), value: 'admin' },
    { label: t('roles.member'), value: 'member' },
    { label: t('roles.viewer'), value: 'viewer' },
  ], [t]);

  if (!canInvite) {
    return null;
  }

  const onSubmit = async (data: InviteFormData) => {
    try {
      await inviteMember(data);
      toast({
        title: t('invite.successTitle'),
        description: t('invite.successDescription'),
        variant: 'success',
      });
      methods.reset();
      await onInvited?.();
    } catch (err) {
      const message = err instanceof Error ? err.message : t('invite.sendError');
      methods.setError('root', { message });
      toast({
        title: t('invite.failedTitle'),
        description: t('invite.failedDescription'),
        variant: 'destructive',
      });
    }
  };

  return (
    <section className="overflow-hidden rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <h2 className="text-lg font-semibold text-slate-900">{t('invite.title')}</h2>
      <FormProvider {...methods}>
        <form className="mt-3 flex flex-col gap-3" noValidate onSubmit={methods.handleSubmit(onSubmit)}>
          <FormField<InviteFormData>
            name="email"
            label={t('invite.emailLabel')}
            type="email"
            placeholder={t('invite.emailPlaceholder')}
            autoComplete="email"
          />

          <FormSelect<InviteFormData>
            name="role"
            label={t('invite.roleLabel')}
            options={roleOptions}
          />

          <FormError message={methods.formState.errors.root?.message} />

          <FormSubmitButton
            loadingText={t('invite.submitting')}
            className="w-full"
          >
            {t('invite.submit')}
          </FormSubmitButton>
        </form>
      </FormProvider>
    </section>
  );
}
