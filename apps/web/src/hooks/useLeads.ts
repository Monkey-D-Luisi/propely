// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';
import type { PaginationState } from '@/hooks/orgs';

export type LeadStatus = 'New' | 'Contacted' | 'Qualified' | 'Converted' | 'Lost';

export interface LeadListItem {
  id: string;
  name: string;
  email: string;
  source: string | null;
  propertyId: string;
  status: LeadStatus;
  assignedAgentId: string | null;
  createdAtUtc: string;
}

export interface Lead {
  id: string;
  name: string;
  email: string;
  phone: string | null;
  message: string | null;
  source: string | null;
  propertyId: string;
  tenantId: string;
  status: LeadStatus;
  assignedAgentId: string | null;
  contactId: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface LeadFilters {
  search?: string;
  status?: LeadStatus;
  propertyId?: string;
  assignedAgentId?: string;
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

export function useLeads(page = 1, pageSize = 100, filters: LeadFilters = {}) {
  const [items, setItems] = useState<LeadListItem[]>([]);
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
      if (filters.status) params.set('status', filters.status);
      if (filters.propertyId) params.set('propertyId', filters.propertyId);
      if (filters.assignedAgentId) params.set('assignedAgentId', filters.assignedAgentId);
      if (filters.sortBy) params.set('sortBy', filters.sortBy);
      if (filters.sortDesc) params.set('sortDesc', 'true');

      const data = await contactsApiFetch<{
        items: LeadListItem[];
        pageNumber: number;
        totalPages: number;
        totalCount: number;
        hasPreviousPage: boolean;
        hasNextPage: boolean;
      }>(`/api/leads?${params}`, { method: 'GET', signal });
      const { items: fetchedItems, ...paginationData } = data;
      setItems(fetchedItems);
      setPagination(paginationData);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [page, pageSize, filters.search, filters.status, filters.propertyId, filters.assignedAgentId, filters.sortBy, filters.sortDesc]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { items, pagination, isLoading, error, refetch: fetcher };
}
