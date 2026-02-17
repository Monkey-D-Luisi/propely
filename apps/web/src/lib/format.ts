// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

/**
 * Format a date string for short display (e.g. "Jan 1, 2026").
 * Falls back to the raw string on invalid input.
 */
export function formatDateShort(dateStr: string, locale?: string): string {
  try {
    const date = new Date(dateStr);
    if (Number.isNaN(date.getTime())) return dateStr;
    return date.toLocaleDateString(locale, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  } catch {
    return dateStr;
  }
}

/**
 * Format a date string for long display with time (e.g. "January 1, 2026, 02:30 PM").
 * Falls back to the raw string on invalid input.
 */
export function formatDateLong(dateStr: string, locale?: string): string {
  try {
    const date = new Date(dateStr);
    if (Number.isNaN(date.getTime())) return dateStr;
    return date.toLocaleString(locale, {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  } catch {
    return dateStr;
  }
}

/**
 * Format a date string for display with time including seconds (e.g. "Jan 1, 2026, 02:30:15 PM").
 * Falls back to the raw string on invalid input.
 */
export function formatDateTime(dateStr: string, locale?: string): string {
  try {
    const date = new Date(dateStr);
    if (Number.isNaN(date.getTime())) return dateStr;
    return date.toLocaleString(locale, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });
  } catch {
    return dateStr;
  }
}
