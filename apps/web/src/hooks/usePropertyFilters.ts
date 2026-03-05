// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useCallback, useEffect, useMemo } from 'react';
import { useSearchParams, useRouter, usePathname } from 'next/navigation';
import type { PropertyFilters } from '@/hooks/properties';
import type { PropertyTypeType, OperationTypeType, PropertyStatusType } from '@/lib/schemas';

const FILTER_KEYS = [
  'search', 'type', 'operation', 'status',
  'minPrice', 'maxPrice', 'city', 'agentId',
  'sortBy', 'sortDesc',
  'minBedrooms', 'minBathrooms', 'minArea', 'maxArea',
  'hasPool', 'hasGarden', 'hasGarage', 'hasElevator', 'hasTerrace',
  'page',
] as const;

function parseFiltersFromParams(params: URLSearchParams): PropertyFilters {
  const filters: PropertyFilters = {};

  const type = params.get('type');
  if (type) filters.type = type as PropertyTypeType;

  const operation = params.get('operation');
  if (operation) filters.operation = operation as OperationTypeType;

  const status = params.get('status');
  if (status) filters.status = status as PropertyStatusType;

  const minPrice = params.get('minPrice');
  if (minPrice) filters.minPrice = Number(minPrice);

  const maxPrice = params.get('maxPrice');
  if (maxPrice) filters.maxPrice = Number(maxPrice);

  const city = params.get('city');
  if (city) filters.city = city;

  const agentId = params.get('agentId');
  if (agentId) filters.agentId = agentId;

  const sortBy = params.get('sortBy');
  if (sortBy) filters.sortBy = sortBy;

  const sortDesc = params.get('sortDesc');
  if (sortDesc === 'true') filters.sortDesc = true;

  const search = params.get('search');
  if (search) filters.search = search;

  const minBedrooms = params.get('minBedrooms');
  if (minBedrooms) filters.minBedrooms = Number(minBedrooms);

  const minBathrooms = params.get('minBathrooms');
  if (minBathrooms) filters.minBathrooms = Number(minBathrooms);

  const minArea = params.get('minArea');
  if (minArea) filters.minArea = Number(minArea);

  const maxArea = params.get('maxArea');
  if (maxArea) filters.maxArea = Number(maxArea);

  if (params.get('hasPool') === 'true') filters.hasPool = true;
  if (params.get('hasGarden') === 'true') filters.hasGarden = true;
  if (params.get('hasGarage') === 'true') filters.hasGarage = true;
  if (params.get('hasElevator') === 'true') filters.hasElevator = true;
  if (params.get('hasTerrace') === 'true') filters.hasTerrace = true;

  return filters;
}

function filtersToParams(filters: PropertyFilters, page: number): URLSearchParams {
  const params = new URLSearchParams();

  if (filters.search) params.set('search', filters.search);
  if (filters.type) params.set('type', filters.type);
  if (filters.operation) params.set('operation', filters.operation);
  if (filters.status) params.set('status', filters.status);
  if (filters.minPrice != null) params.set('minPrice', String(filters.minPrice));
  if (filters.maxPrice != null) params.set('maxPrice', String(filters.maxPrice));
  if (filters.city) params.set('city', filters.city);
  if (filters.agentId) params.set('agentId', filters.agentId);
  if (filters.sortBy) params.set('sortBy', filters.sortBy);
  if (filters.sortDesc) params.set('sortDesc', 'true');
  if (filters.minBedrooms != null) params.set('minBedrooms', String(filters.minBedrooms));
  if (filters.minBathrooms != null) params.set('minBathrooms', String(filters.minBathrooms));
  if (filters.minArea != null) params.set('minArea', String(filters.minArea));
  if (filters.maxArea != null) params.set('maxArea', String(filters.maxArea));
  if (filters.hasPool) params.set('hasPool', 'true');
  if (filters.hasGarden) params.set('hasGarden', 'true');
  if (filters.hasGarage) params.set('hasGarage', 'true');
  if (filters.hasElevator) params.set('hasElevator', 'true');
  if (filters.hasTerrace) params.set('hasTerrace', 'true');
  if (page > 1) params.set('page', String(page));

  return params;
}

export function usePropertyFilters() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  const [filters, setFiltersState] = useState<PropertyFilters>(() =>
    parseFiltersFromParams(searchParams)
  );
  const [page, setPageState] = useState(() => {
    const p = searchParams.get('page');
    return p ? Math.max(1, Number(p)) : 1;
  });

  // Sync URL when filters change
  useEffect(() => {
    const params = filtersToParams(filters, page);
    const queryString = params.toString();
    const newUrl = queryString ? `${pathname}?${queryString}` : pathname;
    router.replace(newUrl, { scroll: false });
  }, [filters, page, pathname, router]);

  const setFilters = useCallback((next: PropertyFilters | ((prev: PropertyFilters) => PropertyFilters)) => {
    setFiltersState(next);
    setPageState(1);
  }, []);

  const setPage = useCallback((p: number) => {
    setPageState(p);
  }, []);

  const clearAll = useCallback(() => {
    setFiltersState({});
    setPageState(1);
  }, []);

  const activeFilterCount = useMemo(() => {
    let count = 0;
    if (filters.search) count++;
    if (filters.type) count++;
    if (filters.operation) count++;
    if (filters.status) count++;
    if (filters.minPrice != null) count++;
    if (filters.maxPrice != null) count++;
    if (filters.city) count++;
    if (filters.agentId) count++;
    if (filters.minBedrooms != null) count++;
    if (filters.minBathrooms != null) count++;
    if (filters.minArea != null) count++;
    if (filters.maxArea != null) count++;
    if (filters.hasPool) count++;
    if (filters.hasGarden) count++;
    if (filters.hasGarage) count++;
    if (filters.hasElevator) count++;
    if (filters.hasTerrace) count++;
    return count;
  }, [filters]);

  return {
    filters,
    setFilters,
    page,
    setPage,
    clearAll,
    activeFilterCount,
  };
}
