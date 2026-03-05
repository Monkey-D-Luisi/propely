// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { rankWith, scopeEndsWith } from '@jsonforms/core';
import type { ControlProps } from '@jsonforms/core';
import { withJsonFormsControlProps } from '@jsonforms/react';

const RATINGS = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'Exempt', 'InProgress'] as const;

const RATING_COLORS: Record<string, string> = {
  A: 'bg-green-600',
  B: 'bg-green-500',
  C: 'bg-yellow-400',
  D: 'bg-yellow-500',
  E: 'bg-orange-400',
  F: 'bg-orange-500',
  G: 'bg-red-500',
  Exempt: 'bg-slate-400',
  InProgress: 'bg-slate-300',
};

const RATING_LABELS: Record<string, string> = {
  A: 'A',
  B: 'B',
  C: 'C',
  D: 'D',
  E: 'E',
  F: 'F',
  G: 'G',
  Exempt: 'Exempt',
  InProgress: 'In Progress',
};

function EnergyRatingRendererComponent({ data, handleChange, path, label }: ControlProps) {
  return (
    <div className="flex flex-col gap-2">
      <label className="text-sm font-medium text-slate-700">{label || 'Energy Rating'}</label>
      <div className="flex flex-wrap gap-1.5" role="radiogroup" aria-label="Energy Rating">
        {RATINGS.map((rating) => (
          <button
            key={rating}
            type="button"
            role="radio"
            aria-checked={data === rating}
            onClick={() => handleChange(path, rating)}
            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-all ${
              data === rating
                ? `${RATING_COLORS[rating]} text-white ring-2 ring-offset-1 ring-primary-600`
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            }`}
          >
            {RATING_LABELS[rating]}
          </button>
        ))}
      </div>
    </div>
  );
}

export const energyRatingTester = rankWith(5, scopeEndsWith('energyRating'));
export const EnergyRatingRenderer = withJsonFormsControlProps(EnergyRatingRendererComponent);
