// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { useCommandBar } from '@/hooks/use-command-bar';

export function AiFloatingButton() {
  const t = useTranslations('commandBar');
  const { open } = useCommandBar();

  return (
    <button
      type="button"
      onClick={open}
      aria-label={t('aiAssistant')}
      className="fixed bottom-6 right-6 z-40 flex h-14 w-14 items-center justify-center rounded-full bg-primary-600 text-white shadow-lg transition-all hover:bg-primary-600/90 hover:shadow-xl hover:scale-105 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 active:scale-95 md:bottom-8 md:right-8"
    >
      <span className="material-symbols-outlined text-[28px]">auto_awesome</span>
    </button>
  );
}
