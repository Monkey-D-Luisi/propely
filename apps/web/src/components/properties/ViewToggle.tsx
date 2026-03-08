// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

interface ViewToggleProps {
  view: 'table' | 'card';
  onViewChange: (view: 'table' | 'card') => void;
}

export function ViewToggle({ view, onViewChange }: ViewToggleProps) {
  return (
    <div className="flex rounded-lg border border-slate-200 bg-white p-0.5" role="radiogroup" aria-label="View toggle">
      <button
        type="button"
        role="radio"
        aria-checked={view === 'table'}
        aria-label="Table view"
        onClick={() => onViewChange('table')}
        className={`rounded-lg px-3 py-1.5 text-sm font-medium transition ${
          view === 'table'
            ? 'bg-primary-600 text-white shadow-sm'
            : 'text-slate-500 hover:text-slate-700'
        }`}
      >
        <span className="material-symbols-outlined text-sm" aria-hidden="true">view_list</span>
      </button>
      <button
        type="button"
        role="radio"
        aria-checked={view === 'card'}
        aria-label="Card view"
        onClick={() => onViewChange('card')}
        className={`rounded-lg px-3 py-1.5 text-sm font-medium transition ${
          view === 'card'
            ? 'bg-primary-600 text-white shadow-sm'
            : 'text-slate-500 hover:text-slate-700'
        }`}
      >
        <span className="material-symbols-outlined text-sm" aria-hidden="true">grid_view</span>
      </button>
    </div>
  );
}
