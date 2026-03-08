// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useFetch } from '@/hooks/use-fetch';
import { propertiesApiFetch, contactsApiFetch, appointmentsApiFetch } from '@/lib/api';

export type DashboardData = {
  properties: { byStatus: Record<string, number> } | null;
  leads: { byStatus: Record<string, number> } | null;
  appointments: {
    byStatus: Record<string, number>;
    upcoming: number;
  } | null;
  isLoading: boolean;
  error: unknown;
};

export function useDashboard(): DashboardData {
  const properties = useFetch<Record<string, number>>(
    '/api/properties/count-by-status',
    { fetcher: propertiesApiFetch },
  );

  const leads = useFetch<Record<string, number>>(
    '/api/leads/count-by-status',
    { fetcher: contactsApiFetch },
  );

  const appointmentsByStatus = useFetch<Record<string, number>>(
    '/api/appointments/count-by-status',
    { fetcher: appointmentsApiFetch },
  );

  const upcoming = useFetch<{ count: number }>(
    '/api/appointments/count-upcoming?days=7',
    { fetcher: appointmentsApiFetch },
  );

  const isLoading =
    properties.isLoading ||
    leads.isLoading ||
    appointmentsByStatus.isLoading ||
    upcoming.isLoading;

  const error =
    properties.error || leads.error || appointmentsByStatus.error || upcoming.error;

  return {
    properties: properties.data ? { byStatus: properties.data } : null,
    leads: leads.data ? { byStatus: leads.data } : null,
    appointments:
      appointmentsByStatus.data
        ? {
            byStatus: appointmentsByStatus.data,
            upcoming: upcoming.data?.count ?? 0,
          }
        : null,
    isLoading,
    error,
  };
}
