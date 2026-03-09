// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from '@/i18n/navigation';
import { FormField, FormSubmitButton, FormError } from '@/components/ui/form';
import { Pagination } from '@/components/ui/pagination';
import { Skeleton } from '@/components/ui/skeleton';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useMyOrgs, useCreateOrg } from '@/hooks/orgs';
import { isApiError } from '@/lib/api';
import { RoleBadge } from '@/components/orgs/RoleBadge';
import { createOrgFormSchema, type CreateOrgFormData } from '@/lib/schemas';
import { useRequireAuth } from '@/hooks/useRequireAuth';

export default function MyOrgsPage() {
  const { isLoading: authLoading } = useRequireAuth();
  const t = useTranslations('orgs');
  const tCommon = useTranslations('common');
  const [page, setPage] = useState(1);
  const { orgs, pagination, isLoading, error, refetch } = useMyOrgs(page);
  const createOrg = useCreateOrg();
  const [showForm, setShowForm] = useState(false);

  const schema = useMemo(
    () =>
      createOrgFormSchema({
        nameRequired: t('validation.orgNameRequired'),
      }),
    [t]
  );

  const methods = useForm<CreateOrgFormData>({
    resolver: zodResolver(schema),
    defaultValues: { name: '' },
  });

  const onSubmit = async (data: CreateOrgFormData) => {
    try {
      await createOrg(data.name);
      methods.reset();
      setShowForm(false);
      void refetch();
    } catch (err) {
      const message = isApiError(err) && err.status === 409
        ? t('mine.nameAlreadyExists')
        : t('mine.createError');
      methods.setError('root', { message });
    }
  };

  if (authLoading || isLoading) {
    return (
      <div className="w-full max-w-6xl mx-auto px-6 py-8 md:px-10 md:py-12">
        <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <Skeleton className="h-9 w-48" />
          <Skeleton className="h-10 w-52 rounded-lg" />
        </div>
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <div
              key={i}
              className="flex flex-col rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
            >
              <div className="mb-4 flex items-start justify-between">
                <Skeleton className="h-12 w-12 rounded-lg" />
                <Skeleton className="h-6 w-16 rounded-full" />
              </div>
              <Skeleton className="h-5 w-32 mb-1" />
              <Skeleton className="h-4 w-20" />
              <div className="mt-4 grid grid-cols-3 gap-4 border-t border-slate-100 pt-4">
                <Skeleton className="h-8 w-full" />
                <Skeleton className="h-8 w-full" />
                <Skeleton className="h-8 w-full" />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="w-full max-w-6xl mx-auto px-6 py-8 md:px-10 md:py-12">
      {/* Section Header */}
      <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-center sm:justify-between">
        <h1 className="text-slate-900 text-[32px] font-bold leading-tight">{t('mine.title')}</h1>
        <button
          type="button"
          onClick={() => setShowForm(v => !v)}
          className="flex cursor-pointer items-center justify-center rounded-lg h-10 px-5 bg-primary-600 hover:bg-primary-600/90 text-white text-sm font-semibold leading-normal shadow-sm transition-colors gap-2"
        >
          <span className="material-symbols-outlined text-[20px]" aria-hidden="true">add</span>
          {showForm ? tCommon('cancel') : t('mine.newOrg')}
        </button>
      </div>

      {showForm ? (
        <FormProvider {...methods}>
          <form
            className="mb-6 flex items-end gap-3 rounded-xl border border-slate-200 bg-white p-5 shadow-sm"
            noValidate
            onSubmit={methods.handleSubmit(onSubmit)}
          >
            <div className="flex-1">
              <FormField<CreateOrgFormData>
                name="name"
                label={t('mine.createOrgLabel')}
                type="text"
                autoFocus
              />
            </div>
            <FormSubmitButton loadingText={t('mine.creatingButton')}>
              {t('mine.createButton')}
            </FormSubmitButton>
            <FormError message={methods.formState.errors.root?.message} />
          </form>
        </FormProvider>
      ) : null}

      {error ? (
        <ErrorMessage message={t('mine.loadError')} />
      ) : null}

      {orgs.length === 0 ? (
        <button
          type="button"
          onClick={() => setShowForm(true)}
          className="group flex min-h-[180px] w-full flex-col items-center justify-center rounded-xl border-2 border-dashed border-slate-300 bg-white/50 p-6 transition-colors hover:bg-slate-50"
        >
          <div className="mb-2 flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 text-slate-400 transition-colors group-hover:bg-primary-600/10 group-hover:text-primary-600">
            <span className="material-symbols-outlined text-2xl" aria-hidden="true">add</span>
          </div>
          <span className="text-lg font-bold text-slate-900">{t('mine.emptyState')}</span>
          <span className="mt-1 text-sm font-medium text-slate-500">{t('mine.description')}</span>
        </button>
      ) : (
        <>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
            {orgs.map(org => (
              <Link
                key={org.id}
                href={`/orgs/${org.id}/members`}
                className="group flex flex-col rounded-xl border border-slate-200 bg-white p-6 shadow-sm transition-colors hover:border-primary-600/50 cursor-pointer"
              >
                <div className="mb-4 flex items-start justify-between">
                  <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary-600/10 text-primary-600">
                    <span className="material-symbols-outlined text-[28px]" aria-hidden="true">domain</span>
                  </div>
                  {org.role ? <RoleBadge role={org.role} /> : null}
                </div>
                <h3 className="text-lg font-bold leading-tight text-slate-900 mb-1">{org.name}</h3>
                <div className="mt-2 grid grid-cols-3 gap-4 border-t border-slate-100 pt-4">
                  <div>
                    <div className="flex items-center gap-1.5 text-slate-500">
                      <span className="material-symbols-outlined text-[16px]" aria-hidden="true">real_estate_agent</span>
                      <span className="text-xs font-medium uppercase tracking-wider">{t('mine.properties')}</span>
                    </div>
                    <p className="mt-1 text-slate-900 font-semibold">—</p>
                  </div>
                  <div>
                    <div className="flex items-center gap-1.5 text-slate-500">
                      <span className="material-symbols-outlined text-[16px]" aria-hidden="true">group</span>
                      <span className="text-xs font-medium uppercase tracking-wider">{t('mine.members')}</span>
                    </div>
                    <p className="mt-1 text-slate-900 font-semibold">—</p>
                  </div>
                  <div>
                    <div className="flex items-center gap-1.5 text-slate-500">
                      <span className="material-symbols-outlined text-[16px]" aria-hidden="true">contacts</span>
                      <span className="text-xs font-medium uppercase tracking-wider">{t('mine.contacts')}</span>
                    </div>
                    <p className="mt-1 text-slate-900 font-semibold">—</p>
                  </div>
                </div>
              </Link>
            ))}
          </div>
          <Pagination pagination={pagination} onPageChange={setPage} />
        </>
      )}
    </div>
  );
}
