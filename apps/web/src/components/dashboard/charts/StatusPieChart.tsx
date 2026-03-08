// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from 'recharts';

const STATUS_COLORS: Record<string, string> = {
  Draft: '#94a3b8',
  Active: '#22c55e',
  Reserved: '#f59e0b',
  Sold: '#3b82f6',
  Withdrawn: '#ef4444',
};

type StatusPieChartProps = {
  data: Record<string, number>;
  title: string;
  colors?: Record<string, string>;
};

export function StatusPieChart({ data, title, colors = STATUS_COLORS }: StatusPieChartProps) {
  const t = useTranslations('dashboard');
  const chartData = Object.entries(data).map(([name, value]) => ({ name, value }));

  if (chartData.length === 0) {
    return (
      <div className="rounded-xl bg-white p-6 shadow-sm" data-testid="status-pie-chart">
        <h3 className="mb-4 text-base font-semibold text-slate-900">{title}</h3>
        <p className="py-8 text-center text-sm text-slate-400">{t('noData')}</p>
      </div>
    );
  }

  return (
    <div className="rounded-xl bg-white p-6 shadow-sm" data-testid="status-pie-chart">
      <h3 className="mb-4 text-base font-semibold text-slate-900">{title}</h3>
      <ResponsiveContainer width="100%" height={260}>
        <PieChart>
          <Pie
            data={chartData}
            cx="50%"
            cy="50%"
            innerRadius={55}
            outerRadius={90}
            paddingAngle={2}
            dataKey="value"
          >
            {chartData.map((entry) => (
              <Cell key={entry.name} fill={colors[entry.name] ?? '#94a3b8'} />
            ))}
          </Pie>
          <Tooltip />
          <Legend />
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
}
