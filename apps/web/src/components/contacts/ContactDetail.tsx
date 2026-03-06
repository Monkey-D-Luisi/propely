// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations, useLocale } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useContact } from '@/hooks/useContact';
import { ContactRoleBadge } from './ContactRoleBadge';
import { PropertyInterestsList } from './PropertyInterestsList';
import { ErrorMessage } from '@/components/ui/ErrorMessage';

interface ContactDetailProps {
  id: string;
}

function formatDate(dateStr: string | null | undefined, locale: string): string {
  if (!dateStr) return '\u2014';
  return new Intl.DateTimeFormat(locale, { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(dateStr));
}

export function ContactDetail({ id }: ContactDetailProps) {
  const t = useTranslations('contacts.detail');
  const tContacts = useTranslations('contacts');
  const tSources = useTranslations('contacts.source');
  const locale = useLocale();

  const { contact, isLoading, error } = useContact(id);

  if (isLoading) {
    return (
      <div className="flex-1 overflow-y-auto p-8" data-testid="contact-loading">
        <div className="mx-auto max-w-[1200px] animate-pulse space-y-6">
          <div className="h-4 w-32 rounded-lg bg-slate-200" />
          <div className="h-8 w-64 rounded-lg bg-slate-200" />
          <div className="space-y-4">
            <div className="h-48 rounded-xl bg-slate-100" />
            <div className="h-48 rounded-xl bg-slate-100" />
          </div>
        </div>
      </div>
    );
  }

  if (error || !contact) {
    return (
      <div className="flex-1 overflow-y-auto p-8" data-testid="contact-error">
        <div className="mx-auto max-w-[1200px]">
          <ErrorMessage message={t('loadError')} />
          <Link
            href="/contacts"
            className="mt-4 inline-flex items-center gap-1 text-sm text-primary-600 hover:text-primary-700"
          >
            <span className="material-symbols-outlined text-sm" aria-hidden="true">arrow_back</span>
            {t('backToList')}
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 overflow-y-auto p-8" data-testid="contact-detail">
      <div className="mx-auto max-w-[1200px]">
      {/* Breadcrumb */}
      <nav className="mb-4 flex items-center gap-2 text-sm text-slate-500" aria-label="Breadcrumb">
        <Link href="/contacts" className="hover:text-primary-600">
          {tContacts('title')}
        </Link>
        <span aria-hidden="true">/</span>
        <span className="text-slate-900">{contact.firstName} {contact.lastName}</span>
      </nav>

      {/* Back link */}
      <Link
        href="/contacts"
        className="mb-6 inline-flex items-center gap-1 text-sm text-slate-500 hover:text-primary-600"
      >
        <span className="material-symbols-outlined text-sm" aria-hidden="true">arrow_back</span>
        {t('backToList')}
      </Link>

      {/* Header */}
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900" data-testid="contact-name">
            {contact.firstName} {contact.lastName}
          </h1>
          <p className="mt-1 text-sm text-slate-500">{contact.email}</p>
        </div>
      </div>

      {/* Roles */}
      {contact.roles.length > 0 && (
        <div className="mb-6 flex flex-wrap gap-2">
          {contact.roles.map((role) => (
            <ContactRoleBadge key={role} role={role} label={tContacts(`role.${role}`)} />
          ))}
        </div>
      )}

      {/* Contact Information Card */}
      <div className="mb-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="mb-4 text-lg font-semibold text-slate-900">{t('information')}</h2>
        <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {contact.company && (
            <div>
              <dt className="text-sm font-medium text-slate-500">{t('company')}</dt>
              <dd className="mt-1 text-sm text-slate-900">{contact.company}</dd>
            </div>
          )}
          {contact.phone && (
            <div>
              <dt className="text-sm font-medium text-slate-500">{t('phone')}</dt>
              <dd className="mt-1 text-sm text-slate-900">{contact.phone}</dd>
            </div>
          )}
          {contact.secondaryPhone && (
            <div>
              <dt className="text-sm font-medium text-slate-500">{t('secondaryPhone')}</dt>
              <dd className="mt-1 text-sm text-slate-900">{contact.secondaryPhone}</dd>
            </div>
          )}
          {contact.preferredLanguage && (
            <div>
              <dt className="text-sm font-medium text-slate-500">{t('preferredLanguage')}</dt>
              <dd className="mt-1 text-sm text-slate-900">{contact.preferredLanguage}</dd>
            </div>
          )}
          {contact.source && (
            <div>
              <dt className="text-sm font-medium text-slate-500">{t('source')}</dt>
              <dd className="mt-1 text-sm text-slate-900">{tSources(contact.source)}</dd>
            </div>
          )}
          <div>
            <dt className="text-sm font-medium text-slate-500">{t('created')}</dt>
            <dd className="mt-1 text-sm text-slate-900">{formatDate(contact.createdAtUtc, locale)}</dd>
          </div>
          {contact.updatedAtUtc && (
            <div>
              <dt className="text-sm font-medium text-slate-500">{t('updated')}</dt>
              <dd className="mt-1 text-sm text-slate-900">{formatDate(contact.updatedAtUtc, locale)}</dd>
            </div>
          )}
        </dl>
        {contact.notes && (
          <div className="mt-4 border-t border-slate-100 pt-4">
            <dt className="text-sm font-medium text-slate-500">{t('notes')}</dt>
            <dd className="mt-1 text-sm text-slate-900 whitespace-pre-wrap">{contact.notes}</dd>
          </div>
        )}
      </div>

      {/* Property Interests */}
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="mb-4 text-lg font-semibold text-slate-900">{t('propertyInterests')}</h2>
        <PropertyInterestsList interests={contact.propertyInterests} />
      </div>
      </div>
    </div>
  );
}
