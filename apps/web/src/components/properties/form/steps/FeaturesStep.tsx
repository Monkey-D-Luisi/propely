// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

const ORIENTATIONS = ['N', 'NE', 'E', 'SE', 'S', 'SW', 'W', 'NW'] as const;

const ENERGY_RATINGS = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'Exempt', 'InProgress'] as const;

const ENERGY_RATING_COLORS: Record<string, string> = {
  A: 'bg-green-600 text-white',
  B: 'bg-green-500 text-white',
  C: 'bg-lime-500 text-white',
  D: 'bg-yellow-400 text-slate-900',
  E: 'bg-orange-400 text-white',
  F: 'bg-orange-600 text-white',
  G: 'bg-red-600 text-white',
  Exempt: 'bg-slate-400 text-white',
  InProgress: 'bg-slate-300 text-slate-700',
};

const AMENITIES = [
  { key: 'hasPool', icon: 'pool', label: 'Pool' },
  { key: 'hasGarden', icon: 'yard', label: 'Garden' },
  { key: 'hasGarage', icon: 'garage', label: 'Garage' },
  { key: 'hasElevator', icon: 'elevator', label: 'Elevator' },
  { key: 'hasTerrace', icon: 'deck', label: 'Terrace' },
  { key: 'airConditioning', icon: 'ac_unit', label: 'Air Conditioning' },
  { key: 'heating', icon: 'local_fire_department', label: 'Heating' },
  { key: 'furnished', icon: 'chair', label: 'Furnished' },
] as const;

interface FeaturesStepProps {
  data: Record<string, unknown>;
  onChange: (data: Record<string, unknown>) => void;
}

export function FeaturesStep({ data, onChange }: FeaturesStepProps) {
  const features = (data.features as Record<string, unknown>) ?? {};

  const updateFeature = (field: string, value: unknown) => {
    onChange({
      ...data,
      features: { ...features, [field]: value },
    });
  };

  const numVal = (field: string): string => {
    const v = features[field];
    return v != null ? String(v) : '';
  };

  const boolVal = (field: string): boolean => {
    return !!features[field];
  };

  return (
    <div className="space-y-6">
      {/* Rooms */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
        <div>
          <label htmlFor="feat-bedrooms" className="mb-1.5 block text-sm font-medium text-slate-700">
            Bedrooms
          </label>
          <input
            id="feat-bedrooms"
            type="number"
            min={0}
            value={numVal('bedrooms')}
            onChange={(e) => updateFeature('bedrooms', e.target.value ? parseInt(e.target.value, 10) : undefined)}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="feat-bathrooms" className="mb-1.5 block text-sm font-medium text-slate-700">
            Bathrooms
          </label>
          <input
            id="feat-bathrooms"
            type="number"
            min={0}
            value={numVal('bathrooms')}
            onChange={(e) => updateFeature('bathrooms', e.target.value ? parseInt(e.target.value, 10) : undefined)}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="feat-parkingSpaces" className="mb-1.5 block text-sm font-medium text-slate-700">
            Parking Spaces
          </label>
          <input
            id="feat-parkingSpaces"
            type="number"
            min={0}
            value={numVal('parkingSpaces')}
            onChange={(e) => updateFeature('parkingSpaces', e.target.value ? parseInt(e.target.value, 10) : undefined)}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
      </div>

      {/* Areas */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div>
          <label htmlFor="feat-builtArea" className="mb-1.5 block text-sm font-medium text-slate-700">
            Built Area (m&sup2;)
          </label>
          <input
            id="feat-builtArea"
            type="number"
            min={0}
            step="0.01"
            value={numVal('builtArea')}
            onChange={(e) => updateFeature('builtArea', e.target.value ? parseFloat(e.target.value) : undefined)}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="feat-usableArea" className="mb-1.5 block text-sm font-medium text-slate-700">
            Usable Area (m&sup2;)
          </label>
          <input
            id="feat-usableArea"
            type="number"
            min={0}
            step="0.01"
            value={numVal('usableArea')}
            onChange={(e) => updateFeature('usableArea', e.target.value ? parseFloat(e.target.value) : undefined)}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="feat-plotArea" className="mb-1.5 block text-sm font-medium text-slate-700">
            Plot Area (m&sup2;)
          </label>
          <input
            id="feat-plotArea"
            type="number"
            min={0}
            step="0.01"
            value={numVal('plotArea')}
            onChange={(e) => updateFeature('plotArea', e.target.value ? parseFloat(e.target.value) : undefined)}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
      </div>

      {/* Floor / Year Built */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
        <div>
          <label htmlFor="feat-floor" className="mb-1.5 block text-sm font-medium text-slate-700">
            Floor
          </label>
          <input
            id="feat-floor"
            type="number"
            value={numVal('floor')}
            onChange={(e) => updateFeature('floor', e.target.value ? parseInt(e.target.value, 10) : undefined)}
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="feat-yearBuilt" className="mb-1.5 block text-sm font-medium text-slate-700">
            Year Built
          </label>
          <input
            id="feat-yearBuilt"
            type="number"
            min={1800}
            max={2100}
            value={numVal('yearBuilt')}
            onChange={(e) => updateFeature('yearBuilt', e.target.value ? parseInt(e.target.value, 10) : undefined)}
            placeholder="e.g. 2020"
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
      </div>

      {/* Orientation */}
      <div>
        <label className="mb-2 block text-sm font-medium text-slate-700">Orientation</label>
        <div className="flex flex-wrap gap-1.5">
          {ORIENTATIONS.map((dir) => (
            <button
              key={dir}
              type="button"
              onClick={() => updateFeature('orientation', features.orientation === dir ? undefined : dir)}
              className={`rounded-lg border px-3 py-1.5 text-xs font-medium transition ${
                features.orientation === dir
                  ? 'border-primary-600 bg-primary-600 text-white'
                  : 'border-slate-200 bg-white text-slate-600 hover:border-slate-300 hover:bg-slate-50'
              }`}
            >
              {dir}
            </button>
          ))}
        </div>
      </div>

      {/* Energy Rating */}
      <div>
        <label className="mb-2 block text-sm font-medium text-slate-700">Energy Rating</label>
        <div className="flex flex-wrap gap-1.5">
          {ENERGY_RATINGS.map((rating) => {
            const isSelected = features.energyRating === rating;
            return (
              <button
                key={rating}
                type="button"
                onClick={() => updateFeature('energyRating', isSelected ? undefined : rating)}
                className={`rounded-lg border px-3 py-1.5 text-xs font-semibold transition ${
                  isSelected
                    ? `${ENERGY_RATING_COLORS[rating]} border-transparent`
                    : 'border-slate-200 bg-white text-slate-600 hover:border-slate-300 hover:bg-slate-50'
                }`}
              >
                {rating}
              </button>
            );
          })}
        </div>
      </div>

      {/* Amenities */}
      <div>
        <label className="mb-2 block text-sm font-medium text-slate-700">Amenities</label>
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
          {AMENITIES.map(({ key, icon, label }) => (
            <label
              key={key}
              className={`flex cursor-pointer items-center gap-2 rounded-lg border px-3 py-2 text-sm transition ${
                boolVal(key)
                  ? 'border-primary-600 bg-primary-50 text-primary-700'
                  : 'border-slate-200 bg-white text-slate-600 hover:border-slate-300 hover:bg-slate-50'
              }`}
            >
              <input
                type="checkbox"
                checked={boolVal(key)}
                onChange={(e) => updateFeature(key, e.target.checked)}
                className="sr-only"
              />
              <span className="material-symbols-outlined text-base" aria-hidden="true">{icon}</span>
              {label}
            </label>
          ))}
        </div>
      </div>
    </div>
  );
}
