// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback, useRef } from 'react';
import { useTranslations } from 'next-intl';
import { useActiveOrg } from '@/hooks/use-active-org';
import { useContacts, type ContactFilters, type ContactRole } from '@/hooks/useContacts';
import { useDeleteContact } from '@/hooks/useDeleteContact';
import { useRequireAuth } from '@/hooks/useRequireAuth';
import { ContactsTable } from '@/components/contacts/ContactsTable';
import { ContactForm } from '@/components/contacts/ContactForm';
import { useCreateContact } from '@/hooks/useCreateContact';
import { Pagination } from '@/components/ui/pagination';
import { DialogOverlay } from '@/components/ui/dialog-overlay';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';

const ALL_ROLES: ContactRole[] = ['Buyer', 'Seller', 'Tenant', 'Landlord', 'Professional'];

export default function ContactsPage() {
  const t = useTranslations('contacts');
  const tCommon = useTranslations('common');
  const { toast } = useToast();
  const { isLoading: userLoading } = useRequireAuth();
  const { hasOrgs, isLoading: orgsLoading } = useActiveOrg();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState<ContactRole | ''>('');
  const [showForm, setShowForm] = useState(false);
  const [isSubmitting, setSubmitting] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [isDeleting, setDeleting] = useState(false);
  const dialogRef = useRef<HTMLDivElement>(null);

  const createContact = useCreateContact();
  const deleteContact = useDeleteContact();

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
    }, 300);
    return () => clearTimeout(timer);
  }, [search]);

  const filters: ContactFilters = {
    search: debouncedSearch || undefined,
    role: roleFilter || undefined,
  };

  const { items, pagination, isLoading, error, refetch } = useContacts(page, 20, filters);

  const handleCreate = useCallback(async (data: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      await createContact(data);
      toast({ title: t('form.createSuccess'), variant: 'success' });
      setShowForm(false);
      void refetch();
    } catch {
      toast({ title: t('loadError'), variant: 'destructive' });
    } finally {
      setSubmitting(false);
    }
  }, [createContact, refetch, toast, t]);

  const handleDelete = async () => {
    if (!deleteId) return;
    setDeleting(true);
    try {
      await deleteContact(deleteId);
      toast({ title: t('delete.success'), variant: 'success' });
      setDeleteId(null);
      void refetch();
    } catch {
      toast({ title: t('loadError'), variant: 'destructive' });
    } finally {
      setDeleting(false);
    }
  };

  if (userLoading || orgsLoading) {
    return (
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
        <div className="animate-pulse space-y-4">
          <div className="h-8 w-48 rounded-lg bg-slate-200" />
          <div className="h-4 w-72 rounded-lg bg-slate-100" />
        </div>
      </div>
    );
  }

  if (!hasOrgs) {
    return (
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-8 text-center shadow-sm">
          <span className="material-symbols-outlined text-4xl text-slate-300" aria-hidden="true">group</span>
          <p className="mt-3 text-sm text-slate-600">{t('noOrg')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
      {/* Header */}
      <div className="flex flex-col gap-4 border-b border-slate-100 pb-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <button
          type="button"
          onClick={() => setShowForm(!showForm)}
          className="inline-flex items-center gap-1.5 rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
        >
          <span className="material-symbols-outlined text-sm" aria-hidden="true">add</span>
          {t('newContact')}
        </button>
      </div>

      {/* New Contact Form */}
      {showForm && (
        <ContactForm
          onSubmit={handleCreate}
          isSubmitting={isSubmitting}
        />
      )}

      {/* Search and Filter */}
      <div className="flex flex-col gap-3 sm:flex-row">
        <div className="relative flex-1">
          <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-lg text-slate-400" aria-hidden="true">search</span>
          <input
            type="text"
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            placeholder={t('search.placeholder')}
            className="w-full rounded-lg border border-slate-200 py-2 pl-10 pr-3 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <select
          value={roleFilter}
          onChange={(e) => { setRoleFilter(e.target.value as ContactRole | ''); setPage(1); }}
          className="rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
        >
          <option value="">{t('filter.allRoles')}</option>
          {ALL_ROLES.map((role) => (
            <option key={role} value={role}>{t(`role.${role}`)}</option>
          ))}
        </select>
      </div>

      {/* Content */}
      {error ? (
        <ErrorMessage message={t('loadError')} />
      ) : items.length === 0 && !isLoading ? (
        <div className="rounded-xl border border-slate-200 bg-white p-12 text-center shadow-sm">
          <span className="material-symbols-outlined text-5xl text-slate-300" aria-hidden="true">person_off</span>
          <h2 className="mt-4 text-lg font-semibold text-slate-900">{t('empty.title')}</h2>
          <p className="mt-1 text-sm text-slate-500">{t('empty.subtitle')}</p>
          <button
            type="button"
            onClick={() => setShowForm(true)}
            className="mt-6 inline-flex items-center gap-1.5 rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98]"
          >
            <span className="material-symbols-outlined text-sm" aria-hidden="true">add</span>
            {t('empty.cta')}
          </button>
        </div>
      ) : (
        <ContactsTable
          items={items}
          isLoading={isLoading}
          onDelete={setDeleteId}
        />
      )}

      <Pagination pagination={pagination} onPageChange={setPage} />

      {/* Delete confirmation dialog */}
      {deleteId && (
        <DialogOverlay dialogRef={dialogRef} isProcessing={isDeleting} onClose={() => setDeleteId(null)}>
          <div
            ref={dialogRef}
            role="alertdialog"
            aria-modal="true"
            aria-labelledby="delete-dialog-title"
            aria-describedby="delete-dialog-desc"
            className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-6 shadow-lg"
          >
            <h2 id="delete-dialog-title" className="text-lg font-semibold text-slate-900">{t('delete.dialogTitle')}</h2>
            <p id="delete-dialog-desc" className="mt-2 text-sm text-slate-500">{t('delete.dialogMessage')}</p>
            <div className="mt-6 flex justify-end gap-3">
              <button type="button" onClick={() => setDeleteId(null)} disabled={isDeleting}
                className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50">
                {t('delete.cancel')}
              </button>
              <button type="button" onClick={() => void handleDelete()} disabled={isDeleting}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-red-700 active:scale-[0.98] disabled:opacity-50">
                {isDeleting ? tCommon('loading') : t('delete.confirm')}
              </button>
            </div>
          </div>
        </DialogOverlay>
      )}
    </div>
  );
}
