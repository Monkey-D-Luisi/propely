// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import type { ContactRole, ContactSource, Contact } from '@/hooks/useContacts';

const ALL_ROLES: ContactRole[] = ['Buyer', 'Seller', 'Tenant', 'Landlord', 'Professional'];
const ALL_SOURCES: ContactSource[] = ['Portal', 'WalkIn', 'Referral', 'Website', 'Phone', 'Other'];

interface ContactFormProps {
  initialData?: Contact | null;
  onSubmit: (data: Record<string, unknown>) => Promise<void>;
  isSubmitting: boolean;
}

export function ContactForm({ initialData, onSubmit, isSubmitting }: ContactFormProps) {
  const t = useTranslations('contacts.form');
  const tRoles = useTranslations('contacts.role');
  const tSources = useTranslations('contacts.source');

  const [firstName, setFirstName] = useState(initialData?.firstName ?? '');
  const [lastName, setLastName] = useState(initialData?.lastName ?? '');
  const [email, setEmail] = useState(initialData?.email ?? '');
  const [phone, setPhone] = useState(initialData?.phone ?? '');
  const [secondaryPhone, setSecondaryPhone] = useState(initialData?.secondaryPhone ?? '');
  const [company, setCompany] = useState(initialData?.company ?? '');
  const [notes, setNotes] = useState(initialData?.notes ?? '');
  const [roles, setRoles] = useState<ContactRole[]>(initialData?.roles ?? []);
  const [preferredLanguage, setPreferredLanguage] = useState(initialData?.preferredLanguage ?? '');
  const [source, setSource] = useState<ContactSource | ''>(initialData?.source ?? '');
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};
    if (!firstName.trim()) newErrors.firstName = t('required');
    if (!lastName.trim()) newErrors.lastName = t('required');
    if (!email.trim()) newErrors.email = t('required');
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) newErrors.email = t('invalidEmail');
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    await onSubmit({
      firstName: firstName.trim(),
      lastName: lastName.trim(),
      email: email.trim(),
      phone: phone.trim() || null,
      secondaryPhone: secondaryPhone.trim() || null,
      company: company.trim() || null,
      notes: notes.trim() || null,
      roles,
      preferredLanguage: preferredLanguage.trim() || null,
      source: source || null,
    });
  };

  const toggleRole = (role: ContactRole) => {
    setRoles((prev) =>
      prev.includes(role) ? prev.filter((r) => r !== role) : [...prev, role]
    );
  };

  return (
    <form onSubmit={(e) => void handleSubmit(e)} className="space-y-6" data-testid="contact-form">
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="mb-4 text-lg font-semibold text-slate-900">{t('title')}</h2>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {/* First Name */}
          <div>
            <label htmlFor="firstName" className="mb-1 block text-sm font-medium text-slate-700">{t('firstName')}</label>
            <input
              id="firstName"
              type="text"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
            />
            {errors.firstName && <p className="mt-1 text-xs text-red-600" role="alert">{errors.firstName}</p>}
          </div>

          {/* Last Name */}
          <div>
            <label htmlFor="lastName" className="mb-1 block text-sm font-medium text-slate-700">{t('lastName')}</label>
            <input
              id="lastName"
              type="text"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
            />
            {errors.lastName && <p className="mt-1 text-xs text-red-600" role="alert">{errors.lastName}</p>}
          </div>

          {/* Email */}
          <div>
            <label htmlFor="email" className="mb-1 block text-sm font-medium text-slate-700">{t('email')}</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
            />
            {errors.email && <p className="mt-1 text-xs text-red-600" role="alert">{errors.email}</p>}
          </div>

          {/* Phone */}
          <div>
            <label htmlFor="phone" className="mb-1 block text-sm font-medium text-slate-700">{t('phone')}</label>
            <input
              id="phone"
              type="tel"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
            />
          </div>

          {/* Secondary Phone */}
          <div>
            <label htmlFor="secondaryPhone" className="mb-1 block text-sm font-medium text-slate-700">{t('secondaryPhone')}</label>
            <input
              id="secondaryPhone"
              type="tel"
              value={secondaryPhone}
              onChange={(e) => setSecondaryPhone(e.target.value)}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
            />
          </div>

          {/* Company */}
          <div>
            <label htmlFor="company" className="mb-1 block text-sm font-medium text-slate-700">{t('company')}</label>
            <input
              id="company"
              type="text"
              value={company}
              onChange={(e) => setCompany(e.target.value)}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
            />
          </div>

          {/* Preferred Language */}
          <div>
            <label htmlFor="preferredLanguage" className="mb-1 block text-sm font-medium text-slate-700">{t('preferredLanguage')}</label>
            <input
              id="preferredLanguage"
              type="text"
              value={preferredLanguage}
              onChange={(e) => setPreferredLanguage(e.target.value)}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
            />
          </div>

          {/* Source */}
          <div>
            <label htmlFor="source" className="mb-1 block text-sm font-medium text-slate-700">{t('source')}</label>
            <select
              id="source"
              value={source}
              onChange={(e) => setSource(e.target.value as ContactSource | '')}
              className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
            >
              <option value="">{t('selectSource')}</option>
              {ALL_SOURCES.map((s) => (
                <option key={s} value={s}>{tSources(s)}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Roles */}
        <div className="mt-4">
          <span className="mb-2 block text-sm font-medium text-slate-700">{t('roles')}</span>
          <div className="flex flex-wrap gap-3">
            {ALL_ROLES.map((role) => (
              <label key={role} className="flex items-center gap-2 text-sm text-slate-700">
                <input
                  type="checkbox"
                  checked={roles.includes(role)}
                  onChange={() => toggleRole(role)}
                  className="h-4 w-4 rounded border-slate-300 text-primary-600 focus:ring-primary-600"
                />
                {tRoles(role)}
              </label>
            ))}
          </div>
        </div>

        {/* Notes */}
        <div className="mt-4">
          <label htmlFor="notes" className="mb-1 block text-sm font-medium text-slate-700">{t('notes')}</label>
          <textarea
            id="notes"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={3}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm transition focus:border-transparent focus:ring-2 focus:ring-primary-600"
          />
        </div>
      </div>

      <div className="flex justify-end">
        <button
          type="submit"
          disabled={isSubmitting}
          className="inline-flex items-center gap-1.5 rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 disabled:opacity-50"
        >
          {isSubmitting ? t('saving') : t('save')}
        </button>
      </div>
    </form>
  );
}
