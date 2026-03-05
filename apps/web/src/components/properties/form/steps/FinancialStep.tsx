// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

interface FinancialStepProps {
  data: Record<string, unknown>;
  onChange: (data: Record<string, unknown>) => void;
}

export function FinancialStep({ data, onChange }: FinancialStepProps) {
  const financials = (data.financials as Record<string, unknown>) ?? {};

  const updateFinancial = (field: string, value: unknown) => {
    onChange({
      ...data,
      financials: { ...financials, [field]: value },
    });
  };

  const numVal = (field: string): string => {
    const v = financials[field];
    return v != null ? String(v) : '';
  };

  const strVal = (field: string): string => {
    return (financials[field] as string) ?? '';
  };

  return (
    <div className="space-y-6">
      {/* Price */}
      <div>
        <label htmlFor="fin-price" className="mb-1.5 block text-sm font-medium text-slate-700">
          Price <span className="text-red-500">*</span>
        </label>
        <div className="relative">
          <span className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-sm text-slate-400">
            &euro;
          </span>
          <input
            id="fin-price"
            type="number"
            min={0}
            step="0.01"
            value={numVal('price')}
            onChange={(e) => updateFinancial('price', e.target.value ? parseFloat(e.target.value) : undefined)}
            placeholder="0.00"
            className="w-full rounded-lg border border-slate-200 py-2 pl-8 pr-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
      </div>

      {/* Community Fees / IBI Tax */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="fin-communityFees" className="mb-1.5 block text-sm font-medium text-slate-700">
            Community Fees
          </label>
          <div className="relative">
            <span className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-sm text-slate-400">
              &euro;
            </span>
            <input
              id="fin-communityFees"
              type="number"
              min={0}
              step="0.01"
              value={numVal('communityFees')}
              onChange={(e) => updateFinancial('communityFees', e.target.value ? parseFloat(e.target.value) : undefined)}
              placeholder="0.00"
              className="w-full rounded-lg border border-slate-200 py-2 pl-8 pr-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
            />
          </div>
          <p className="mt-1 text-xs text-slate-400">/month</p>
        </div>
        <div>
          <label htmlFor="fin-ibiTax" className="mb-1.5 block text-sm font-medium text-slate-700">
            IBI Tax
          </label>
          <div className="relative">
            <span className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-sm text-slate-400">
              &euro;
            </span>
            <input
              id="fin-ibiTax"
              type="number"
              min={0}
              step="0.01"
              value={numVal('ibiTax')}
              onChange={(e) => updateFinancial('ibiTax', e.target.value ? parseFloat(e.target.value) : undefined)}
              placeholder="0.00"
              className="w-full rounded-lg border border-slate-200 py-2 pl-8 pr-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
            />
          </div>
          <p className="mt-1 text-xs text-slate-400">/year</p>
        </div>
      </div>

      {/* Catastro Reference */}
      <div>
        <label htmlFor="fin-catastroRef" className="mb-1.5 block text-sm font-medium text-slate-700">
          Catastro Reference
        </label>
        <input
          id="fin-catastroRef"
          type="text"
          value={strVal('catastroReference')}
          onChange={(e) => updateFinancial('catastroReference', e.target.value || undefined)}
          maxLength={50}
          placeholder="e.g. 1234567AB1234S0001QE"
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
        />
      </div>
    </div>
  );
}
