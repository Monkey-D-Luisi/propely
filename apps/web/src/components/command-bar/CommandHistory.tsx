// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';

export interface CommandHistoryProps {
  commands: string[];
  onSelect: (command: string) => void;
}

export function CommandHistory({ commands, onSelect }: CommandHistoryProps) {
  const t = useTranslations('commandBar');

  return (
    <div className="px-2 py-2">
      <p className="px-2 py-1 text-xs font-medium text-slate-500">
        {t('recentCommands')}
      </p>
      {commands.length === 0 ? (
        <p className="px-2 py-2 text-xs text-slate-400">{t('noRecentCommands')}</p>
      ) : (
        <ul role="listbox" aria-label={t('recentCommands')}>
          {commands.map((command) => (
            <li key={command} role="option" aria-selected={false}>
              <button
                type="button"
                onClick={() => onSelect(command)}
                className="flex w-full items-center gap-2 rounded-lg px-2 py-2 text-left text-sm text-slate-700 transition-colors hover:bg-slate-100 focus:bg-slate-100 focus:outline-none"
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                  className="h-4 w-4 shrink-0 text-slate-400"
                  aria-hidden="true"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm.75-13a.75.75 0 00-1.5 0v5c0 .414.336.75.75.75h4a.75.75 0 000-1.5h-3.25V5z"
                    clipRule="evenodd"
                  />
                </svg>
                <span className="truncate">{command}</span>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
