// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { appointmentsApiFetch } from '@/lib/api';

export type AppointmentType = 'PropertyViewing' | 'OwnerMeeting' | 'Generic';
export type AppointmentStatus = 'Scheduled' | 'Confirmed' | 'Completed' | 'Cancelled' | 'NoShow';

export interface AppointmentListItem {
  id: string;
  title: string;
  type: AppointmentType;
  status: AppointmentStatus;
  startTimeUtc: string;
  endTimeUtc: string;
  location: string | null;
  isAllDay: boolean;
  propertyId: string | null;
  contactId: string | null;
  agentId: string;
}

export interface Appointment extends AppointmentListItem {
  description: string | null;
  tenantId: string;
  notes: string | null;
  cancellationReason: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface AppointmentFilters {
  search?: string;
  status?: AppointmentStatus;
  type?: AppointmentType;
  fromUtc?: string;
  toUtc?: string;
  agentId?: string;
  propertyId?: string;
  contactId?: string;
}

export function useAppointments(filters: AppointmentFilters = {}) {
  const [items, setItems] = useState<AppointmentListItem[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams({
        page: '1',
        pageSize: '200',
      });
      if (filters.search) params.set('search', filters.search);
      if (filters.status) params.set('status', filters.status);
      if (filters.type) params.set('type', filters.type);
      if (filters.fromUtc) params.set('fromUtc', filters.fromUtc);
      if (filters.toUtc) params.set('toUtc', filters.toUtc);
      if (filters.agentId) params.set('agentId', filters.agentId);
      if (filters.propertyId) params.set('propertyId', filters.propertyId);
      if (filters.contactId) params.set('contactId', filters.contactId);

      const data = await appointmentsApiFetch<{
        items: AppointmentListItem[];
        pageNumber: number;
        totalPages: number;
        totalCount: number;
        hasPreviousPage: boolean;
        hasNextPage: boolean;
      }>(`/api/appointments?${params}`, { method: 'GET', signal });
      setItems(data.items);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [filters.search, filters.status, filters.type, filters.fromUtc, filters.toUtc, filters.agentId, filters.propertyId, filters.contactId]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { items, isLoading, error, refetch: fetcher };
}
