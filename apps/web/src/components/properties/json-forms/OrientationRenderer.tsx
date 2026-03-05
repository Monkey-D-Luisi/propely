'use client';

import { rankWith, scopeEndsWith } from '@jsonforms/core';
import type { ControlProps } from '@jsonforms/core';
import { withJsonFormsControlProps } from '@jsonforms/react';

const DIRECTIONS = ['N', 'NE', 'E', 'SE', 'S', 'SW', 'W', 'NW'] as const;

const DIRECTION_LABELS: Record<string, string> = {
  N: 'N', NE: 'NE', E: 'E', SE: 'SE',
  S: 'S', SW: 'SW', W: 'W', NW: 'NW',
};

const DIRECTION_ANGLES: Record<string, number> = {
  N: 0, NE: 45, E: 90, SE: 135,
  S: 180, SW: 225, W: 270, NW: 315,
};

function OrientationRendererComponent({ data, handleChange, path, label }: ControlProps) {
  return (
    <div className="flex flex-col gap-2">
      <label className="text-sm font-medium text-slate-700">{label || 'Orientation'}</label>
      <div className="relative w-40 h-40 mx-auto" role="radiogroup" aria-label="Orientation">
        {/* Compass circle */}
        <div className="absolute inset-0 rounded-full border-2 border-slate-200" />
        {DIRECTIONS.map((dir) => {
          const angle = DIRECTION_ANGLES[dir];
          const radian = ((angle - 90) * Math.PI) / 180;
          const radius = 60;
          const x = 80 + radius * Math.cos(radian);
          const y = 80 + radius * Math.sin(radian);

          return (
            <button
              key={dir}
              type="button"
              role="radio"
              aria-checked={data === dir}
              onClick={() => handleChange(path, dir)}
              className={`absolute w-8 h-8 -ml-4 -mt-4 rounded-full text-xs font-bold
                flex items-center justify-center transition-all ${
                data === dir
                  ? 'bg-primary-600 text-white ring-2 ring-offset-1 ring-primary-600'
                  : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
              }`}
              style={{ left: `${x}px`, top: `${y}px` }}
            >
              {DIRECTION_LABELS[dir]}
            </button>
          );
        })}
      </div>
    </div>
  );
}

export const orientationTester = rankWith(5, scopeEndsWith('orientation'));
export const OrientationRenderer = withJsonFormsControlProps(OrientationRendererComponent);
