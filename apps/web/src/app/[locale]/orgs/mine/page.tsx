// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMemo, useState } from 'react';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from '@/i18n/navigation';
import { Button } from '@/components/ui/button';
import { FormField, FormSubmitButton, FormError } from '@/components/ui/form';
import { Pagination } from '@/components/ui/pagination';
import { Skeleton } from '@/components/ui/skeleton';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useMyOrgs, useCreateOrg } from '@/hooks/orgs';
import { isApiError } from '@/lib/api';
import { RoleBadge } from '@/components/orgs/RoleBadge';
import { createOrgFormSchema, type CreateOrgFormData } from '@/lib/schemas';

export default function MyOrgsPage() {
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

  if (isLoading) {
    return (
      <div className="mx-auto w-full max-w-4xl px-4 py-12">
        <div className="mb-10 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <Skeleton className="h-9 w-48" />
            <Skeleton className="mt-1 h-4 w-64" />
          </div>
          <Skeleton className="h-10 w-44 rounded-lg" />
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
              <Skeleton className="h-5 w-32" />
              <Skeleton className="mt-2 h-4 w-full" />
              <div className="mt-6 border-t border-slate-50 pt-6">
                <Skeleton className="h-4 w-24" />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto w-full max-w-4xl px-4 py-12">
      {/* Section Header */}
      <div className="mb-10 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('mine.title')}</h1>
          <p className="mt-1 text-slate-500">{t('mine.description')}</p>
        </div>
        <Button type="button" onClick={() => setShowForm(v => !v)}>
          <span className="material-symbols-outlined mr-2 text-sm" aria-hidden="true">add</span>
          {showForm ? tCommon('cancel') : t('mine.newOrg')}
        </Button>
      </div>

      {showForm ? (
        <FormProvider {...methods}>
          <form
            className="flex items-end gap-3 rounded-xl border border-slate-100 bg-white p-5 shadow-md"
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
          className="group flex min-h-[220px] flex-col items-center justify-center rounded-xl border-2 border-dashed border-slate-300 bg-transparent p-6 transition-colors hover:border-primary-600"
        >
          <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 text-slate-400 transition-colors group-hover:bg-primary-600/10 group-hover:text-primary-600">
            <span className="material-symbols-outlined text-2xl" aria-hidden="true">add</span>
          </div>
          <span className="text-sm font-semibold text-slate-600 group-hover:text-primary-600">{t('mine.emptyState')}</span>
        </button>
      ) : (
        <>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
            {orgs.map(org => (
              <Link
                key={org.id}
                href={`/orgs/${org.id}/members`}
                className="group flex flex-col rounded-xl border border-slate-200 bg-white p-6 shadow-sm transition-shadow hover:shadow-md"
              >
                <div className="mb-4 flex items-start justify-between">
                  <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary-600/10 text-primary-600">
                    <span className="material-symbols-outlined" aria-hidden="true">business</span>
                  </div>
                  {org.role ? <RoleBadge role={org.role} /> : null}
                </div>
                <h3 className="mb-2 text-lg font-bold text-slate-900">{org.name}</h3>
                <div className="mt-auto flex items-center justify-between border-t border-slate-50 pt-6">
                  <div className="flex items-center text-slate-400">
                    <span className="material-symbols-outlined mr-1 text-sm" aria-hidden="true">group</span>
                  </div>
                  <span className="flex items-center text-sm font-semibold text-primary-600 transition-transform group-hover:translate-x-1">
                    {t('mine.view')} <span className="material-symbols-outlined ml-1 text-xs" aria-hidden="true">chevron_right</span>
                  </span>
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
