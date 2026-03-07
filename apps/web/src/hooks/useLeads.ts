// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { contactsApiFetch } from '@/lib/api';
import { usePaginatedFetch, type FetcherFn } from '@/hooks/use-fetch';

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

function buildLeadsUrl(page: number, pageSize: number, filters: LeadFilters): string {
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
  return `/api/leads?${params}`;
}

export function useLeads(page = 1, pageSize = 100, filters: LeadFilters = {}) {
  const url = buildLeadsUrl(page, pageSize, filters);
  return usePaginatedFetch<LeadListItem>(url, {
    fetcher: contactsApiFetch as FetcherFn,
  });
}
