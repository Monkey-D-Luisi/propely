// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import type { PaginationState } from '@/hooks/orgs';

interface PaginationProps {
  pagination: PaginationState;
  onPageChange: (page: number) => void;
}

export function Pagination({ pagination, onPageChange }: PaginationProps) {
  const t = useTranslations('common.pagination');

  if (pagination.totalPages <= 1) return null;

  return (
    <nav
      aria-label="Pagination"
      className="mt-12 flex items-center justify-between border-t border-slate-200 pt-8"
    >
      <p className="text-sm text-slate-500">
        {t('pageOf', { page: pagination.pageNumber, total: pagination.totalPages })}
      </p>
      <div className="flex gap-2">
        <button
          type="button"
          disabled={!pagination.hasPreviousPage}
          onClick={() => onPageChange(pagination.pageNumber - 1)}
          className="rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-sm font-medium text-slate-500 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {t('previous')}
        </button>
        <button
          type="button"
          disabled={!pagination.hasNextPage}
          onClick={() => onPageChange(pagination.pageNumber + 1)}
          className="rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-sm font-medium text-slate-500 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {t('next')}
        </button>
      </div>
    </nav>
  );
}
