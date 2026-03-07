// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { propertiesApiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import type {
  PropertyListItem,
  Property,
  PropertyMedia,
  PagedResponse,
  StatusCount,
  PropertyTypeType,
  OperationTypeType,
  PropertyStatusType,
} from '@/lib/schemas';
import {
  PropertiesListResponseSchema,
  PropertySchema,
} from '@/lib/schemas';
import type { PaginationState } from '@/lib/pagination';
import { emptyPagination } from '@/lib/pagination';

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

export function useProperties(page = 1, pageSize = 20, filters: PropertyFilters = {}) {
  const [items, setItems] = useState<PropertyListItem[]>([]);
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

      const data = await propertiesApiFetch<PagedResponse<PropertyListItem>>(
        `/api/properties?${params}`,
        { method: 'GET', signal },
        PropertiesListResponseSchema,
      );
      const { items: fetchedItems, ...paginationData } = data;
      setItems(fetchedItems);
      setPagination(paginationData);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [page, pageSize, filters.type, filters.operation, filters.status, filters.minPrice, filters.maxPrice, filters.city, filters.agentId, filters.sortBy, filters.sortDesc, filters.search, filters.minBedrooms, filters.minBathrooms, filters.minArea, filters.maxArea, filters.hasPool, filters.hasGarden, filters.hasGarage, filters.hasElevator, filters.hasTerrace]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { items, pagination, isLoading, error, refetch: fetcher };
}

export function useProperty(id: string) {
  const [property, setProperty] = useState<Property | null>(null);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const data = await propertiesApiFetch<Property>(
        `/api/properties/${id}`,
        { method: 'GET', signal },
        PropertySchema,
      );
      setProperty(data);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { property, isLoading, error, refetch: fetcher };
}

export function usePropertyMedia(propertyId: string) {
  const [media, setMedia] = useState<PropertyMedia[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const data = await propertiesApiFetch<PropertyMedia[]>(
        `/api/properties/${propertyId}/media`,
        { method: 'GET', signal },
      );
      setMedia(data);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [propertyId]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { media, isLoading, error, refetch: fetcher };
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
  const [counts, setCounts] = useState<StatusCount[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const data = await propertiesApiFetch<StatusCount[]>(
        '/api/properties/count-by-status',
        { method: 'GET', signal },
      );
      setCounts(data);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { counts, isLoading, error, refetch: fetcher };
}
