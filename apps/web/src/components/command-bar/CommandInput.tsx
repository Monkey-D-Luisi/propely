// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useRef, useCallback, type ReactNode } from 'react';
import { useTranslations } from 'next-intl';

export interface CommandInputProps {
  onSubmit: (text: string) => void;
  isLoading: boolean;
  disabled?: boolean;
  /** Optional slot rendered between the input and submit button (e.g. voice mic). */
  trailingSlot?: ReactNode;
}

export function CommandInput({
  onSubmit,
  isLoading,
  disabled = false,
  trailingSlot,
}: CommandInputProps) {
  const t = useTranslations('commandBar');
  const [value, setValue] = useState('');
  const inputRef = useRef<HTMLInputElement>(null);

  const handleSubmit = useCallback(() => {
    const trimmed = value.trim();
    if (!trimmed || isLoading || disabled) return;
    onSubmit(trimmed);
  }, [value, isLoading, disabled, onSubmit]);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLInputElement>) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        handleSubmit();
      }
    },
    [handleSubmit],
  );

  return (
    <div className="flex items-center gap-2 border-b border-slate-200 px-4 py-3">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 20 20"
        fill="currentColor"
        className="h-5 w-5 shrink-0 text-slate-400"
        aria-hidden="true"
      >
        <path
          fillRule="evenodd"
          d="M9 3.5a5.5 5.5 0 100 11 5.5 5.5 0 000-11zM2 9a7 7 0 1112.452 4.391l3.328 3.329a.75.75 0 11-1.06 1.06l-3.329-3.328A7 7 0 012 9z"
          clipRule="evenodd"
        />
      </svg>
      <input
        ref={inputRef}
        type="text"
        value={value}
        onChange={(e) => setValue(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder={t('placeholder')}
        disabled={isLoading || disabled}
        className="flex-1 bg-transparent text-sm text-slate-900 placeholder:text-slate-400 outline-none disabled:opacity-50"
        autoFocus
        aria-label={t('placeholder')}
      />
      {trailingSlot}
      <button
        type="button"
        onClick={handleSubmit}
        disabled={!value.trim() || isLoading || disabled}
        className="rounded-lg bg-primary-600 px-3 py-1.5 text-xs font-medium text-white shadow-sm transition-all hover:bg-primary-600/90 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {isLoading ? t('loading') : t('submit')}
      </button>
    </div>
  );
}
