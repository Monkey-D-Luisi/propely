// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import Link from 'next/link';
import { useSuggestions, type Suggestion } from '@/hooks/use-suggestions';

const priorityStyles: Record<string, { bg: string; icon: string; iconColor: string }> = {
  High: { bg: 'bg-red-50', icon: 'priority_high', iconColor: 'text-red-500' },
  Medium: { bg: 'bg-amber-50', icon: 'info', iconColor: 'text-amber-500' },
  Low: { bg: 'bg-sky-50', icon: 'lightbulb', iconColor: 'text-sky-500' },
};

function SuggestionCard({ suggestion }: { suggestion: Suggestion }) {
  const style = priorityStyles[suggestion.priority] ?? priorityStyles.Low;

  return (
    <div data-testid="suggestion-card" className={`${style.bg} rounded-xl p-4 flex items-start gap-3`}>
      <span className={`material-symbols-outlined text-[20px] mt-0.5 ${style.iconColor}`}>
        {style.icon}
      </span>
      <div className="flex-1 min-w-0">
        <p className="text-sm text-slate-700">{suggestion.message}</p>
        {suggestion.actionUrl && suggestion.actionLabel && (
          <Link
            href={suggestion.actionUrl}
            className="text-sm text-primary-700 underline hover:text-primary-800 mt-1 inline-block"
          >
            {suggestion.actionLabel}
          </Link>
        )}
      </div>
    </div>
  );
}

export function SuggestionFeed() {
  const t = useTranslations('suggestions');
  const { suggestions, isLoading } = useSuggestions();

  if (isLoading) {
    return (
      <div data-testid="suggestions-loading" className="flex flex-col gap-3">
        {[1, 2].map((i) => (
          <div key={i} className="bg-slate-100 rounded-xl h-16 animate-pulse" />
        ))}
      </div>
    );
  }

  if (suggestions.length === 0) {
    return (
      <div data-testid="suggestions-empty" className="text-center py-8 text-slate-500">
        <span className="material-symbols-outlined text-[32px] block mb-2">check_circle</span>
        <p className="text-sm">{t('allCaughtUp')}</p>
      </div>
    );
  }

  return (
    <div data-testid="suggestion-feed" className="flex flex-col gap-3">
      <h3 className="text-base font-semibold text-slate-900">{t('title')}</h3>
      {suggestions.map((s, i) => (
        <SuggestionCard key={`${s.type}-${i}`} suggestion={s} />
      ))}
    </div>
  );
}
