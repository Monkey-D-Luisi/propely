// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

interface LocationStepProps {
  data: Record<string, unknown>;
  onChange: (data: Record<string, unknown>) => void;
}

export function LocationStep({ data, onChange }: LocationStepProps) {
  const address = (data.address as Record<string, unknown>) ?? {};

  const updateAddress = (field: string, value: unknown) => {
    onChange({
      ...data,
      address: { ...address, [field]: value },
    });
  };

  const street = (address.street as string) ?? '';
  const city = (address.city as string) ?? '';
  const province = (address.province as string) ?? '';
  const postalCode = (address.postalCode as string) ?? '';
  const country = (address.country as string) ?? '';
  const latitude = address.latitude as number | undefined;
  const longitude = address.longitude as number | undefined;
  const hasCoords = latitude != null && longitude != null && !isNaN(latitude) && !isNaN(longitude);

  return (
    <div className="space-y-6">
      {/* Street */}
      <div>
        <label htmlFor="address-street" className="mb-1.5 block text-sm font-medium text-slate-700">
          Street
        </label>
        <input
          id="address-street"
          type="text"
          value={street}
          onChange={(e) => updateAddress('street', e.target.value)}
          maxLength={200}
          placeholder="e.g. Calle Gran Via 45, 3B"
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
        />
      </div>

      {/* City, Province, Postal Code */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div>
          <label htmlFor="address-city" className="mb-1.5 block text-sm font-medium text-slate-700">
            City
          </label>
          <input
            id="address-city"
            type="text"
            value={city}
            onChange={(e) => updateAddress('city', e.target.value)}
            maxLength={100}
            placeholder="e.g. Madrid"
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="address-province" className="mb-1.5 block text-sm font-medium text-slate-700">
            Province
          </label>
          <input
            id="address-province"
            type="text"
            value={province}
            onChange={(e) => updateAddress('province', e.target.value)}
            maxLength={100}
            placeholder="e.g. Madrid"
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="address-postalCode" className="mb-1.5 block text-sm font-medium text-slate-700">
            Postal Code
          </label>
          <input
            id="address-postalCode"
            type="text"
            value={postalCode}
            onChange={(e) => updateAddress('postalCode', e.target.value)}
            maxLength={20}
            placeholder="e.g. 28013"
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
      </div>

      {/* Country */}
      <div className="max-w-xs">
        <label htmlFor="address-country" className="mb-1.5 block text-sm font-medium text-slate-700">
          Country
        </label>
        <input
          id="address-country"
          type="text"
          value={country}
          onChange={(e) => updateAddress('country', e.target.value)}
          maxLength={5}
          placeholder="e.g. ES"
          className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
        />
      </div>

      {/* Latitude / Longitude */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="address-latitude" className="mb-1.5 block text-sm font-medium text-slate-700">
            Latitude
          </label>
          <input
            id="address-latitude"
            type="number"
            step="any"
            min={-90}
            max={90}
            value={latitude ?? ''}
            onChange={(e) => updateAddress('latitude', e.target.value ? parseFloat(e.target.value) : undefined)}
            placeholder="e.g. 40.4168"
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
        <div>
          <label htmlFor="address-longitude" className="mb-1.5 block text-sm font-medium text-slate-700">
            Longitude
          </label>
          <input
            id="address-longitude"
            type="number"
            step="any"
            min={-180}
            max={180}
            value={longitude ?? ''}
            onChange={(e) => updateAddress('longitude', e.target.value ? parseFloat(e.target.value) : undefined)}
            placeholder="e.g. -3.7038"
            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
        </div>
      </div>

      {/* Map placeholder */}
      {hasCoords && (
        <div
          className="flex h-48 items-center justify-center rounded-xl border border-slate-200 bg-slate-50"
          data-testid="map-placeholder"
        >
          <div className="text-center text-sm text-slate-400">
            <span className="material-symbols-outlined mb-1 block text-2xl" aria-hidden="true">map</span>
            Map preview ({latitude}, {longitude})
          </div>
        </div>
      )}
    </div>
  );
}
