// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { propertiesApiFetch } from '@/lib/api';
import { useFetch, usePaginatedFetch, type FetcherFn } from '@/hooks/use-fetch';
import { ensureCsrfToken } from '@/lib/csrf';
import type {
  PropertyListItem,
  Property,
  PropertyMedia,
  StatusCount,
  PropertyTypeType,
  OperationTypeType,
  PropertyStatusType,
} from '@/lib/schemas';
import {
  PropertiesListResponseSchema,
  PropertySchema,
} from '@/lib/schemas';

export interface PropertyFilters {
  type?: PropertyTypeType;
  operation?: OperationTypeType;
  status?: PropertyStatusType;
  minPrice?: number;
  maxPrice?: number;
  city?: string;
  agentId?: string;
  sortBy?: string;
  sortDesc?: boolean;
  search?: string;
  minBedrooms?: number;
  minBathrooms?: number;
  minArea?: number;
  maxArea?: number;
  hasPool?: boolean;
  hasGarden?: boolean;
  hasGarage?: boolean;
  hasElevator?: boolean;
  hasTerrace?: boolean;
}

function buildPropertiesUrl(page: number, pageSize: number, filters: PropertyFilters): string {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });
  if (filters.type) params.set('type', filters.type);
  if (filters.operation) params.set('operation', filters.operation);
  if (filters.status) params.set('status', filters.status);
  if (filters.minPrice != null) params.set('minPrice', String(filters.minPrice));
  if (filters.maxPrice != null) params.set('maxPrice', String(filters.maxPrice));
  if (filters.city) params.set('city', filters.city);
  if (filters.agentId) params.set('agentId', filters.agentId);
  if (filters.sortBy) params.set('sortBy', filters.sortBy);
  if (filters.sortDesc) params.set('sortDesc', 'true');
  if (filters.search) params.set('search', filters.search);
  if (filters.minBedrooms != null) params.set('minBedrooms', String(filters.minBedrooms));
  if (filters.minBathrooms != null) params.set('minBathrooms', String(filters.minBathrooms));
  if (filters.minArea != null) params.set('minArea', String(filters.minArea));
  if (filters.maxArea != null) params.set('maxArea', String(filters.maxArea));
  if (filters.hasPool) params.set('hasPool', 'true');
  if (filters.hasGarden) params.set('hasGarden', 'true');
  if (filters.hasGarage) params.set('hasGarage', 'true');
  if (filters.hasElevator) params.set('hasElevator', 'true');
  if (filters.hasTerrace) params.set('hasTerrace', 'true');
  return `/api/properties?${params}`;
}

export function useProperties(page = 1, pageSize = 20, filters: PropertyFilters = {}) {
  const url = buildPropertiesUrl(page, pageSize, filters);
  return usePaginatedFetch<PropertyListItem>(url, {
    fetcher: propertiesApiFetch as FetcherFn,
    schema: PropertiesListResponseSchema,
  });
}

export function useProperty(id: string) {
  const { data: property, ...rest } = useFetch<Property>(`/api/properties/${id}`, {
    fetcher: propertiesApiFetch as FetcherFn,
    schema: PropertySchema,
  });
  return { property, ...rest };
}

export function usePropertyMedia(propertyId: string) {
  const { data: media, ...rest } = useFetch<PropertyMedia[]>(`/api/properties/${propertyId}/media`, {
    fetcher: propertiesApiFetch as FetcherFn,
  });
  return { media: media ?? [], ...rest };
}

export function useCreateProperty() {
  return useCallback(async (data: Record<string, unknown>) => {
    const csrfToken = await ensureCsrfToken();
    return propertiesApiFetch<Property>('/api/properties', {
      method: 'POST',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify(data),
    }, PropertySchema);
  }, []);
}

export function useUpdateProperty() {
  return useCallback(async (id: string, data: Record<string, unknown>) => {
    const csrfToken = await ensureCsrfToken();
    return propertiesApiFetch<Property>(`/api/properties/${id}`, {
      method: 'PUT',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify(data),
    }, PropertySchema);
  }, []);
}

export function useDeleteProperty() {
  return useCallback(async (id: string) => {
    const csrfToken = await ensureCsrfToken();
    await propertiesApiFetch<void>(`/api/properties/${id}`, {
      method: 'DELETE',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
    });
  }, []);
}

export function useChangePropertyStatus() {
  return useCallback(async (id: string, status: PropertyStatusType) => {
    const csrfToken = await ensureCsrfToken();
    await propertiesApiFetch<void>(`/api/properties/${id}/status`, {
      method: 'PATCH',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify({ status }),
    });
  }, []);
}

export function useStatusCounts() {
  const { data: counts, ...rest } = useFetch<StatusCount[]>('/api/properties/count-by-status', {
    fetcher: propertiesApiFetch as FetcherFn,
  });
  return { counts: counts ?? [], ...rest };
}
