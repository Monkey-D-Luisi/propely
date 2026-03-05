// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import type { PropertyFilters } from '@/hooks/properties';

const STORAGE_KEY = 'propely-filter-presets';

interface FilterPreset {
  name: string;
  filters: PropertyFilters;
}

interface FilterPresetManagerProps {
  currentFilters: PropertyFilters;
  onApplyPreset: (filters: PropertyFilters) => void;
}

function loadPresets(): FilterPreset[] {
  if (typeof window === 'undefined') return [];
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as FilterPreset[]) : [];
  } catch {
    return [];
  }
}

function savePresets(presets: FilterPreset[]): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(presets));
}

export function FilterPresetManager({ currentFilters, onApplyPreset }: FilterPresetManagerProps) {
  const tSearch = useTranslations('properties.search');
  const [presets, setPresets] = useState<FilterPreset[]>([]);
  const [showSave, setShowSave] = useState(false);
  const [presetName, setPresetName] = useState('');

  useEffect(() => {
    setPresets(loadPresets());
  }, []);

  const handleSave = useCallback(() => {
    if (!presetName.trim()) return;
    const next = [...presets, { name: presetName.trim(), filters: currentFilters }];
    setPresets(next);
    savePresets(next);
    setPresetName('');
    setShowSave(false);
  }, [presetName, presets, currentFilters]);

  const handleDelete = useCallback((index: number) => {
    const next = presets.filter((_, i) => i !== index);
    setPresets(next);
    savePresets(next);
  }, [presets]);

  const hasFilters = Object.values(currentFilters).some((v) =>
    v !== undefined && v !== '' && v !== false
  );

  return (
    <div className="flex flex-wrap items-center gap-2" data-testid="filter-preset-manager">
      <span className="text-xs font-medium text-slate-500">{tSearch('savedPresets')}</span>

      {presets.map((preset, i) => (
        <span key={i} className="inline-flex items-center gap-1">
          <button
            type="button"
            onClick={() => onApplyPreset(preset.filters)}
            className="rounded-full bg-slate-100 px-3 py-1 text-xs font-medium text-slate-700 transition hover:bg-slate-200"
          >
            {preset.name}
          </button>
          <button
            type="button"
            onClick={() => handleDelete(i)}
            className="inline-flex h-4 w-4 items-center justify-center rounded-full text-slate-400 hover:text-red-500 transition"
            aria-label={`Delete preset ${preset.name}`}
          >
            <span className="material-symbols-outlined text-xs" aria-hidden="true">close</span>
          </button>
        </span>
      ))}

      {hasFilters && !showSave && (
        <button
          type="button"
          onClick={() => setShowSave(true)}
          className="inline-flex items-center gap-1 rounded-full border border-dashed border-slate-300 px-3 py-1 text-xs font-medium text-slate-500 transition hover:border-primary-400 hover:text-primary-600"
        >
          <span className="material-symbols-outlined text-xs" aria-hidden="true">bookmark_add</span>
          {tSearch('savePreset')}
        </button>
      )}

      {showSave && (
        <div className="inline-flex items-center gap-1.5">
          <input
            type="text"
            value={presetName}
            onChange={(e) => setPresetName(e.target.value)}
            placeholder={tSearch('presetName')}
            className="w-32 rounded-lg border border-slate-200 px-2 py-1 text-xs focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
            onKeyDown={(e) => { if (e.key === 'Enter') handleSave(); }}
            data-testid="preset-name-input"
          />
          <button
            type="button"
            onClick={handleSave}
            disabled={!presetName.trim()}
            className="rounded-lg bg-primary-600 px-2 py-1 text-xs font-medium text-white transition hover:bg-primary-600/90 disabled:opacity-50"
          >
            Save
          </button>
          <button
            type="button"
            onClick={() => { setShowSave(false); setPresetName(''); }}
            className="text-xs text-slate-500 hover:text-slate-700"
          >
            Cancel
          </button>
        </div>
      )}
    </div>
  );
}
