// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';

const priorityStyles: Record<string, string> = {
  Low: 'bg-slate-50 text-slate-600 border-slate-200',
  Medium: 'bg-blue-50 text-blue-700 border-blue-200',
  High: 'bg-orange-50 text-orange-700 border-orange-200',
  Critical: 'bg-red-50 text-red-700 border-red-200',
};

const typeStyles: Record<string, string> = {
  Task: 'bg-slate-50 text-slate-600 border-slate-200',
  Bug: 'bg-red-50 text-red-700 border-red-200',
  Feature: 'bg-purple-50 text-purple-700 border-purple-200',
  Improvement: 'bg-teal-50 text-teal-700 border-teal-200',
};

const effortStyles: Record<string, string> = {
  XS: 'bg-emerald-50 text-emerald-700 border-emerald-200',
  S: 'bg-emerald-50 text-emerald-700 border-emerald-200',
  M: 'bg-blue-50 text-blue-700 border-blue-200',
  L: 'bg-orange-50 text-orange-700 border-orange-200',
  XL: 'bg-red-50 text-red-700 border-red-200',
};

interface PriorityBadgeProps {
  priority: string;
}

export function PriorityBadge({ priority }: PriorityBadgeProps) {
  const t = useTranslations('workItems.priority');
  const style = priorityStyles[priority] ?? priorityStyles.Medium;
  const label = t.has(priority) ? t(priority) : priority;
  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${style}`}
    >
      {label}
    </span>
  );
}

interface TypeBadgeProps {
  type: string;
}

export function TypeBadge({ type }: TypeBadgeProps) {
  const t = useTranslations('workItems.type');
  const style = typeStyles[type] ?? typeStyles.Task;
  const label = t.has(type) ? t(type) : type;
  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${style}`}
    >
      {label}
    </span>
  );
}

interface EffortBadgeProps {
  effort: string;
}

export function EffortBadge({ effort }: EffortBadgeProps) {
  const t = useTranslations('workItems.effort');
  const style = effortStyles[effort] ?? effortStyles.M;
  const label = t.has(effort) ? t(effort) : effort;
  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${style}`}
    >
      {label}
    </span>
  );
}
