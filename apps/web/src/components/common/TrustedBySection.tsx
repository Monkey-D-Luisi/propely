// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

interface TrustedBySectionProps {
  heading: string;
}

const companies = [
  { icon: 'change_history', name: 'Acme' },
  { icon: 'hexagon', name: 'Capsule' },
  { icon: 'all_inclusive', name: 'Infinite' },
  { icon: 'bolt', name: 'BoltShift' },
  { icon: 'radio_button_checked', name: 'Global' },
] as const;

export function TrustedBySection({ heading }: TrustedBySectionProps) {
  return (
    <div className="mt-20 text-center">
      <p className="mb-8 text-sm font-semibold uppercase tracking-wider text-slate-500">
        {heading}
      </p>
      <ul className="flex flex-wrap items-center justify-center gap-x-12 gap-y-8 opacity-60 grayscale transition-all duration-500 hover:grayscale-0">
        {companies.map((company) => (
          <li key={company.name} className="flex items-center gap-2 text-xl font-bold text-slate-700">
            <span className="material-symbols-outlined" aria-hidden="true">{company.icon}</span>
            <span>{company.name}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
