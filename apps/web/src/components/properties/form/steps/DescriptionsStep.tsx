// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState } from 'react';

const LANGUAGES = [
  { code: 'es', label: 'ES', full: 'Spanish' },
  { code: 'pt', label: 'PT', full: 'Portuguese' },
  { code: 'en', label: 'EN', full: 'English' },
  { code: 'fr', label: 'FR', full: 'French' },
  { code: 'de', label: 'DE', full: 'German' },
  { code: 'nl', label: 'NL', full: 'Dutch' },
] as const;

const PRIMARY_LANGUAGES = ['es', 'pt', 'en'] as const;

interface DescriptionsStepProps {
  data: Record<string, unknown>;
  onChange: (data: Record<string, unknown>) => void;
}

export function DescriptionsStep({ data, onChange }: DescriptionsStepProps) {
  const description = (data.description as Record<string, string>) ?? {};
  const [activeTab, setActiveTab] = useState<string>('es');
  const [showExtra, setShowExtra] = useState(false);

  const updateDescription = (lang: string, value: string) => {
    onChange({
      ...data,
      description: { ...description, [lang]: value },
    });
  };

  const visibleLanguages = showExtra
    ? LANGUAGES
    : LANGUAGES.filter((l) => PRIMARY_LANGUAGES.includes(l.code as typeof PRIMARY_LANGUAGES[number]));

  return (
    <div className="space-y-4">
      {/* Language tabs */}
      <div className="flex items-center gap-1 border-b border-slate-200">
        {visibleLanguages.map((lang) => {
          const isActive = activeTab === lang.code;
          const hasContent = !!description[lang.code]?.trim();
          return (
            <button
              key={lang.code}
              type="button"
              onClick={() => setActiveTab(lang.code)}
              className={`relative border-b-2 px-3 py-2 text-sm font-medium transition ${
                isActive
                  ? 'border-primary-600 text-primary-700'
                  : 'border-transparent text-slate-500 hover:text-slate-700'
              }`}
            >
              {lang.label}
              {hasContent && (
                <span className="ml-1.5 inline-block h-1.5 w-1.5 rounded-full bg-primary-600" />
              )}
            </button>
          );
        })}
        {!showExtra && (
          <button
            type="button"
            onClick={() => setShowExtra(true)}
            className="ml-1 px-2 py-2 text-xs text-slate-400 hover:text-slate-600"
          >
            + More
          </button>
        )}
      </div>

      {/* Textarea for active language */}
      {visibleLanguages.map((lang) => (
        <div key={lang.code} className={activeTab === lang.code ? 'block' : 'hidden'}>
          <label htmlFor={`desc-${lang.code}`} className="mb-1.5 block text-sm font-medium text-slate-700">
            Description ({lang.full})
          </label>
          <textarea
            id={`desc-${lang.code}`}
            value={description[lang.code] ?? ''}
            onChange={(e) => updateDescription(lang.code, e.target.value)}
            maxLength={10000}
            rows={8}
            placeholder={`Write the property description in ${lang.full}...`}
            className="w-full resize-y rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
          />
          <p className="mt-1 text-xs text-slate-400">
            {(description[lang.code] ?? '').length}/10,000
          </p>
        </div>
      ))}
    </div>
  );
}
