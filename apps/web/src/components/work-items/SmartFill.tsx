// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { useParseWorkItem } from '@/hooks/work-items';
import { WorkItemStatusEnum, WorkItemPriorityEnum, WorkItemTypeEnum, WorkItemEffortEnum, type WorkItemFormData } from '@/lib/schemas';

interface SmartFillProps {
  onFill: (data: Partial<WorkItemFormData>) => void;
}

export function SmartFill({ onFill }: SmartFillProps) {
  const t = useTranslations('workItems.smartFill');
  const [text, setText] = useState('');
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const { parse, isLoading } = useParseWorkItem();

  const handleSmartFill = async () => {
    if (!text.trim()) return;
    setMessage(null);

    const result = await parse(text);
    if (result && result.confidence > 0) {
      onFill({
        title: result.title,
        description: result.description ?? '',
        status: WorkItemStatusEnum.catch('Pending').parse(result.status),
        priority: result.priority ? WorkItemPriorityEnum.catch('Medium').parse(result.priority) : undefined,
        type: result.type ? WorkItemTypeEnum.catch('Task').parse(result.type) : undefined,
        dueDate: result.dueDateUtc ?? undefined,
        estimatedEffort: result.estimatedEffort ? WorkItemEffortEnum.catch('M').parse(result.estimatedEffort) : undefined,
      });
      setMessage({ type: 'success', text: t('success') });
    } else {
      setMessage({ type: 'error', text: t('error') });
    }
  };

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
      <div className="mb-3 flex items-center gap-2">
        <span className="material-symbols-outlined text-lg text-primary-600" aria-hidden="true">
          auto_awesome
        </span>
        <h3 className="text-sm font-semibold text-slate-900">{t('title')}</h3>
      </div>

      <textarea
        value={text}
        onChange={(e) => setText(e.target.value)}
        placeholder={t('placeholder')}
        rows={3}
        className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
      />

      <div className="mt-3 flex items-center justify-between">
        <p className="text-xs text-slate-500">{t('hint')}</p>
        <button
          type="button"
          onClick={() => void handleSmartFill()}
          disabled={isLoading || !text.trim()}
          className="inline-flex items-center gap-1.5 rounded-lg bg-primary-600 px-3 py-1.5 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] disabled:opacity-50"
        >
          <span className="material-symbols-outlined text-sm" aria-hidden="true">auto_awesome</span>
          {isLoading ? t('loading') : t('button')}
        </button>
      </div>

      {message && (
        <p className={`mt-2 text-sm ${message.type === 'success' ? 'text-emerald-600' : 'text-red-600'}`}>
          {message.text}
        </p>
      )}
    </div>
  );
}
