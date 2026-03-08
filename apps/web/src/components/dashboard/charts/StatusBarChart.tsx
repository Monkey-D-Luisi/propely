// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell } from 'recharts';

const LEAD_STATUS_COLORS: Record<string, string> = {
  New: '#3b82f6',
  Contacted: '#f59e0b',
  Qualified: '#22c55e',
  Converted: '#8b5cf6',
  Lost: '#ef4444',
};

type StatusBarChartProps = {
  data: Record<string, number>;
  title: string;
  colors?: Record<string, string>;
};

export function StatusBarChart({ data, title, colors = LEAD_STATUS_COLORS }: StatusBarChartProps) {
  const t = useTranslations('dashboard');
  const chartData = Object.entries(data).map(([name, value]) => ({ name, value }));

  if (chartData.length === 0) {
    return (
      <div className="rounded-xl bg-white p-6 shadow-sm" data-testid="status-bar-chart">
        <h3 className="mb-4 text-base font-semibold text-slate-900">{title}</h3>
        <p className="py-8 text-center text-sm text-slate-400">{t('noData')}</p>
      </div>
    );
  }

  return (
    <div className="rounded-xl bg-white p-6 shadow-sm" data-testid="status-bar-chart">
      <h3 className="mb-4 text-base font-semibold text-slate-900">{title}</h3>
      <ResponsiveContainer width="100%" height={260}>
        <BarChart data={chartData} layout="vertical" margin={{ left: 20 }}>
          <XAxis type="number" hide />
          <YAxis type="category" dataKey="name" width={80} tick={{ fontSize: 13 }} />
          <Tooltip />
          <Bar dataKey="value" radius={[0, 4, 4, 0]} barSize={24}>
            {chartData.map((entry) => (
              <Cell key={entry.name} fill={colors[entry.name] ?? '#94a3b8'} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
