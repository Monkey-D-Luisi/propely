// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useActiveOrg } from '@/hooks/use-active-org';
import { useProperties, useDeleteProperty, useChangePropertyStatus, type PropertyFilters } from '@/hooks/properties';
import { useRequireAuth } from '@/hooks/useRequireAuth';
import { usePropertyFilters } from '@/hooks/usePropertyFilters';
import { PropertyTable } from '@/components/properties/PropertyTable';
import { PropertyCard } from '@/components/properties/PropertyCard';
import { PropertyFilterBar } from '@/components/properties/PropertyFilterBar';
import { AdvancedFilterPanel, type AdvancedFilters } from '@/components/properties/search/AdvancedFilterPanel';
import { ActiveFilterBadges } from '@/components/properties/search/ActiveFilterBadges';
import { FilterPresetManager } from '@/components/properties/search/FilterPresetManager';
import { ViewToggle } from '@/components/properties/ViewToggle';
import { Pagination } from '@/components/ui/pagination';
import { DialogOverlay } from '@/components/ui/dialog-overlay';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';
import type { PropertyStatusType } from '@/lib/schemas';

const emptyAdvancedFilters: AdvancedFilters = {
  types: [],
  operations: [],
  statuses: [],
};

/** Convert URL-based PropertyFilters to the AdvancedFilters shape used by the panel */
function toAdvancedFilters(f: PropertyFilters): AdvancedFilters {
  return {
    types: f.type ? [f.type] : [],
    operations: f.operation ? [f.operation] : [],
    statuses: f.status ? [f.status] : [],
    minPrice: f.minPrice,
    maxPrice: f.maxPrice,
    minArea: f.minArea,
    maxArea: f.maxArea,
    minBedrooms: f.minBedrooms,
    minBathrooms: f.minBathrooms,
    city: f.city,
    hasPool: f.hasPool,
    hasGarden: f.hasGarden,
    hasGarage: f.hasGarage,
    hasElevator: f.hasElevator,
    hasTerrace: f.hasTerrace,
  };
}

/** Convert the AdvancedFilters panel state back into PropertyFilters (preserving search & sort) */
function mergeAdvancedFilters(base: PropertyFilters, adv: AdvancedFilters): PropertyFilters {
  return {
    ...base,
    type: adv.types.length === 1 ? adv.types[0] : undefined,
    operation: adv.operations.length === 1 ? adv.operations[0] : undefined,
    status: adv.statuses.length === 1 ? adv.statuses[0] : undefined,
    minPrice: adv.minPrice,
    maxPrice: adv.maxPrice,
    minArea: adv.minArea,
    maxArea: adv.maxArea,
    minBedrooms: adv.minBedrooms,
    minBathrooms: adv.minBathrooms,
    city: adv.city,
    hasPool: adv.hasPool,
    hasGarden: adv.hasGarden,
    hasGarage: adv.hasGarage,
    hasElevator: adv.hasElevator,
    hasTerrace: adv.hasTerrace,
  };
}

export default function PropertiesPage() {
  const t = useTranslations('properties');
  const tCommon = useTranslations('common');
  const { user, isLoading: userLoading } = useRequireAuth();
  const { orgs, activeOrgId, hasOrgs, isLoading: orgsLoading } = useActiveOrg();
  const { toast } = useToast();

  const { filters, setFilters, page, setPage, clearAll, activeFilterCount } = usePropertyFilters();

  // Local search for debounce
  const [search, setSearch] = useState(filters.search ?? '');
  const [debouncedSearch, setDebouncedSearch] = useState(filters.search ?? '');

  const [view, setView] = useState<'table' | 'card'>(() => {
    if (typeof window !== 'undefined') {
      return (localStorage.getItem('propely-property-view') as 'table' | 'card') || 'table';
    }
    return 'table';
  });

  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [isDeleting, setDeleting] = useState(false);
  const [statusChangeTarget, setStatusChangeTarget] = useState<{ id: string; status: PropertyStatusType } | null>(null);
  const [isChangingStatus, setChangingStatus] = useState(false);
  const dialogRef = useRef<HTMLDivElement>(null);

  const deleteProperty = useDeleteProperty();
  const changeStatus = useChangePropertyStatus();

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      if (search !== (filters.search ?? '')) {
        setFilters((prev) => ({ ...prev, search: search || undefined }));
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [search, filters.search, setFilters]);

  // Persist view preference
  useEffect(() => {
    localStorage.setItem('propely-property-view', view);
  }, [view]);

  // Build the query filters (use debounced search)
  const queryFilters: PropertyFilters = {
    ...filters,
    search: debouncedSearch || undefined,
  };

  const { items, pagination, isLoading, error, refetch } = useProperties(
    page,
    view === 'card' ? 12 : 20,
    queryFilters,
  );

  // Filter bar handlers (for the simple dropdowns)
  const handleTypeChange = useCallback((val: string) => {
    setFilters((prev) => ({ ...prev, type: (val || undefined) as PropertyFilters['type'] }));
  }, [setFilters]);
  const handleOperationChange = useCallback((val: string) => {
    setFilters((prev) => ({ ...prev, operation: (val || undefined) as PropertyFilters['operation'] }));
  }, [setFilters]);
  const handleStatusFilterChange = useCallback((val: string) => {
    setFilters((prev) => ({ ...prev, status: (val || undefined) as PropertyFilters['status'] }));
  }, [setFilters]);
  const handleSortByChange = useCallback((val: string) => {
    setFilters((prev) => ({ ...prev, sortBy: val || undefined }));
  }, [setFilters]);

  // Advanced filter panel handlers
  const handleAdvancedFiltersChange = useCallback((adv: AdvancedFilters) => {
    setFilters((prev) => mergeAdvancedFilters(prev, adv));
  }, [setFilters]);
  const handleAdvancedFiltersClear = useCallback(() => {
    setFilters((prev) => ({
      search: prev.search,
      sortBy: prev.sortBy,
      sortDesc: prev.sortDesc,
    }));
  }, [setFilters]);

  // Active filter badge removal
  const handleRemoveFilter = useCallback((key: keyof PropertyFilters) => {
    setFilters((prev) => {
      const next = { ...prev };
      // When removing price range, remove both min and max together
      if (key === 'minPrice') {
        delete next.minPrice;
        delete next.maxPrice;
      } else if (key === 'maxPrice') {
        delete next.maxPrice;
      } else if (key === 'minArea') {
        delete next.minArea;
        delete next.maxArea;
      } else if (key === 'maxArea') {
        delete next.maxArea;
      } else {
        delete next[key];
      }
      return next;
    });
  }, [setFilters]);

  // Preset apply handler
  const handleApplyPreset = useCallback((preset: PropertyFilters) => {
    setSearch(preset.search ?? '');
    setFilters(preset);
  }, [setFilters]);

  const handleClearAll = useCallback(() => {
    setSearch('');
    clearAll();
  }, [clearAll]);

  const handleDelete = async () => {
    if (!deleteId) return;
    setDeleting(true);
    try {
      await deleteProperty(deleteId);
      toast({ title: t('delete.confirm'), variant: 'success' });
      setDeleteId(null);
      void refetch();
    } catch {
      toast({ title: t('loadError'), variant: 'destructive' });
    } finally {
      setDeleting(false);
    }
  };

  const handleStatusChange = async () => {
    if (!statusChangeTarget) return;
    setChangingStatus(true);
    try {
      await changeStatus(statusChangeTarget.id, statusChangeTarget.status);
      toast({ title: t('statusChange.success'), variant: 'success' });
      setStatusChangeTarget(null);
      void refetch();
    } catch {
      toast({ title: t('statusChange.error'), variant: 'destructive' });
    } finally {
      setChangingStatus(false);
    }
  };

  const handleChangeStatusRequest = (id: string, status: PropertyStatusType) => {
    setStatusChangeTarget({ id, status });
  };

  if (userLoading || orgsLoading) {
    return (
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
        <div className="animate-pulse space-y-4">
          <div className="h-8 w-48 rounded-lg bg-slate-200" />
          <div className="h-4 w-72 rounded-lg bg-slate-100" />
        </div>
      </div>
    );
  }

  if (!hasOrgs) {
    return (
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-8 text-center shadow-sm">
          <span className="material-symbols-outlined text-4xl text-slate-300" aria-hidden="true">domain_add</span>
          <p className="mt-3 text-sm text-slate-600">{t('noOrg')}</p>
          <Link
            href="/orgs/mine"
            className="mt-4 inline-flex items-center gap-1.5 rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98]"
          >
            {t('createOrg')}
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
      {/* Header */}
      <div className="flex flex-col gap-4 border-b border-slate-100 pb-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{t('title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
        </div>
        <div className="flex items-center gap-3">
          <ViewToggle view={view} onViewChange={setView} />
          <Link
            href="/properties/new"
            className="inline-flex items-center gap-1.5 rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
          >
            <span className="material-symbols-outlined text-sm" aria-hidden="true">add</span>
            {t('newProperty')}
          </Link>
        </div>
      </div>

      {/* Filter bar */}
      <PropertyFilterBar
        search={search}
        onSearchChange={setSearch}
        type={filters.type ?? ''}
        onTypeChange={handleTypeChange}
        operation={filters.operation ?? ''}
        onOperationChange={handleOperationChange}
        status={filters.status ?? ''}
        onStatusChange={handleStatusFilterChange}
        sortBy={filters.sortBy ?? ''}
        onSortByChange={handleSortByChange}
      />

      {/* Advanced filter panel */}
      <AdvancedFilterPanel
        filters={toAdvancedFilters(filters)}
        onFiltersChange={handleAdvancedFiltersChange}
        onClear={handleAdvancedFiltersClear}
      />

      {/* Active filter badges */}
      <ActiveFilterBadges
        filters={filters}
        onRemove={handleRemoveFilter}
        onClearAll={handleClearAll}
      />

      {/* Saved filter presets */}
      <FilterPresetManager
        currentFilters={filters}
        onApplyPreset={handleApplyPreset}
      />

      {/* Content */}
      {error ? (
        <ErrorMessage message={t('loadError')} />
      ) : items.length === 0 && !isLoading ? (
        <div className="rounded-xl border border-slate-200 bg-white p-12 text-center shadow-sm">
          <span className="material-symbols-outlined text-5xl text-slate-300" aria-hidden="true">apartment</span>
          <h2 className="mt-4 text-lg font-semibold text-slate-900">{t('empty.title')}</h2>
          <p className="mt-1 text-sm text-slate-500">{t('empty.subtitle')}</p>
          <Link
            href="/properties/new"
            className="mt-6 inline-flex items-center gap-1.5 rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98]"
          >
            <span className="material-symbols-outlined text-sm" aria-hidden="true">add</span>
            {t('empty.cta')}
          </Link>
        </div>
      ) : view === 'table' ? (
        <PropertyTable
          items={items}
          isLoading={isLoading}
          onDelete={setDeleteId}
          onChangeStatus={handleChangeStatusRequest}
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((item) => (
            <PropertyCard key={item.id} item={item} />
          ))}
        </div>
      )}

      <Pagination pagination={pagination} onPageChange={setPage} />

      {/* Delete confirmation dialog */}
      {deleteId && (
        <DialogOverlay dialogRef={dialogRef} isProcessing={isDeleting} onClose={() => setDeleteId(null)}>
          <div
            ref={dialogRef}
            role="alertdialog"
            aria-modal="true"
            aria-labelledby="delete-dialog-title"
            aria-describedby="delete-dialog-desc"
            className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-6 shadow-lg"
          >
            <h2 id="delete-dialog-title" className="text-lg font-semibold text-slate-900">{t('delete.dialogTitle')}</h2>
            <p id="delete-dialog-desc" className="mt-2 text-sm text-slate-500">{t('delete.dialogMessage')}</p>
            <div className="mt-6 flex justify-end gap-3">
              <button type="button" onClick={() => setDeleteId(null)} disabled={isDeleting}
                className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50">
                {t('delete.cancel')}
              </button>
              <button type="button" onClick={() => void handleDelete()} disabled={isDeleting}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-red-700 active:scale-[0.98] disabled:opacity-50">
                {isDeleting ? tCommon('loading') : t('delete.confirm')}
              </button>
            </div>
          </div>
        </DialogOverlay>
      )}

      {/* Status change confirmation dialog */}
      {statusChangeTarget && (
        <DialogOverlay dialogRef={dialogRef} isProcessing={isChangingStatus} onClose={() => setStatusChangeTarget(null)}>
          <div
            ref={dialogRef}
            role="alertdialog"
            aria-modal="true"
            aria-labelledby="status-dialog-title"
            aria-describedby="status-dialog-desc"
            className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-6 shadow-lg"
          >
            <h2 id="status-dialog-title" className="text-lg font-semibold text-slate-900">{t('statusChange.dialogTitle')}</h2>
            <p id="status-dialog-desc" className="mt-2 text-sm text-slate-500">
              {t('statusChange.dialogMessage', {
                from: t(`status.${items.find(i => i.id === statusChangeTarget.id)?.status ?? 'Draft'}`),
                to: t(`status.${statusChangeTarget.status}`),
              })}
            </p>
            <div className="mt-6 flex justify-end gap-3">
              <button type="button" onClick={() => setStatusChangeTarget(null)} disabled={isChangingStatus}
                className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50">
                {t('statusChange.cancel')}
              </button>
              <button type="button" onClick={() => void handleStatusChange()} disabled={isChangingStatus}
                className="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-primary-600/90 active:scale-[0.98] disabled:opacity-50">
                {isChangingStatus ? tCommon('loading') : t('statusChange.confirm')}
              </button>
            </div>
          </div>
        </DialogOverlay>
      )}
    </div>
  );
}
