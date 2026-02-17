// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

/**
 * Maps a status string to Tailwind color classes using the provided mapping.
 * Falls back to `defaultColor` when the status is not in the map.
 */
export function getStatusColor(
  status: string,
  colorMap: Record<string, string>,
  defaultColor = 'bg-slate-100 text-slate-600',
): string {
  return colorMap[status] ?? defaultColor;
}

/** Color map for payment statuses (succeeded, pending, failed). */
export const paymentStatusColors: Record<string, string> = {
  succeeded: 'bg-emerald-100 text-emerald-800',
  pending: 'bg-amber-100 text-amber-800',
  failed: 'bg-red-100 text-red-800',
};

/** Color map for subscription statuses (active, trialing, pastdue, cancelled). */
export const subscriptionStatusColors: Record<string, string> = {
  active: 'bg-emerald-100 text-emerald-800',
  trialing: 'bg-emerald-100 text-emerald-800',
  pastdue: 'bg-red-100 text-red-800',
  cancelled: 'bg-slate-100 text-slate-600',
};
