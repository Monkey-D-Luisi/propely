// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';
import type { PaginationState } from '@/hooks/orgs';

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

const emptyPagination: PaginationState = {
  pageNumber: 1,
  totalPages: 0,
  totalCount: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

export function useContacts(page = 1, pageSize = 20, filters: ContactFilters = {}) {
  const [items, setItems] = useState<ContactListItem[]>([]);
  const [pagination, setPagination] = useState<PaginationState>(emptyPagination);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
      });
      if (filters.search) params.set('search', filters.search);
      if (filters.role) params.set('role', filters.role);
      if (filters.sortBy) params.set('sortBy', filters.sortBy);
      if (filters.sortDesc) params.set('sortDesc', 'true');

      const data = await contactsApiFetch<{
        items: ContactListItem[];
        pageNumber: number;
        totalPages: number;
        totalCount: number;
        hasPreviousPage: boolean;
        hasNextPage: boolean;
      }>(`/api/contacts?${params}`, { method: 'GET', signal });
      const { items: fetchedItems, ...paginationData } = data;
      setItems(fetchedItems);
      setPagination(paginationData);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [page, pageSize, filters.search, filters.role, filters.sortBy, filters.sortDesc]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { items, pagination, isLoading, error, refetch: fetcher };
}
