// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { contactsApiFetch } from '@/lib/api';
import { usePaginatedFetch, type FetcherFn } from '@/hooks/use-fetch';

export type ContactRole = 'Buyer' | 'Seller' | 'Tenant' | 'Landlord' | 'Professional';
export type ContactSource = 'Portal' | 'WalkIn' | 'Referral' | 'Website' | 'Phone' | 'Other';
export type InterestType = 'Buying' | 'Renting' | 'Selling';

export interface ContactListItem {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  company: string | null;
  roles: ContactRole[];
  createdAtUtc: string;
}

export interface PropertyInterest {
  id: string;
  propertyId: string;
  interestType: InterestType;
  notes: string | null;
  createdAtUtc: string;
}

export interface Contact {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  secondaryPhone: string | null;
  company: string | null;
  notes: string | null;
  preferredLanguage: string | null;
  source: ContactSource | null;
  assignedAgentId: string | null;
  tenantId: string;
  roles: ContactRole[];
  propertyInterests: PropertyInterest[];
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface ContactFilters {
  search?: string;
  role?: ContactRole;
  sortBy?: string;
  sortDesc?: boolean;
}

function buildContactsUrl(page: number, pageSize: number, filters: ContactFilters): string {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });
  if (filters.search) params.set('search', filters.search);
  if (filters.role) params.set('role', filters.role);
  if (filters.sortBy) params.set('sortBy', filters.sortBy);
  if (filters.sortDesc) params.set('sortDesc', 'true');
  return `/api/contacts?${params}`;
}

export function useContacts(page = 1, pageSize = 20, filters: ContactFilters = {}) {
  const url = buildContactsUrl(page, pageSize, filters);
  return usePaginatedFetch<ContactListItem>(url, {
    fetcher: contactsApiFetch as FetcherFn,
  });
}
