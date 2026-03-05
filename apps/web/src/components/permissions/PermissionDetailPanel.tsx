// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback, useState } from 'react';
import { useTranslations } from 'next-intl';
import { useUserPermissions, useSetPermissionOverride, useRemovePermissionOverride } from '@/hooks/permissions';
import { PermissionSourceBadge } from '@/components/permissions/PermissionSourceBadge';
import { PermissionToggle } from '@/components/permissions/PermissionToggle';
import { PermissionCategoryGroup } from '@/components/permissions/PermissionCategoryGroup';
import { OverrideConfirmDialog } from '@/components/permissions/OverrideConfirmDialog';
import { RoleBadge } from '@/components/orgs/RoleBadge';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/toast';
import { ArrowLeftIcon } from '@/components/ui/icons';
import { Link } from '@/i18n/navigation';
import { isManager } from '@/lib/roles';
import { isApiError } from '@/lib/api';
import type { Role } from '@/lib/schemas';
import type { EffectivePermission } from '@/lib/schemas';

// Permission categories with their member permissions
const PERMISSION_CATEGORIES: Record<string, string[]> = {
  properties: ['PropertiesViewAll', 'PropertiesEditAll'],
  contacts: ['ContactsViewAll', 'ContactsEditAll'],
  appointments: ['AppointmentsViewAll'],
  publishing: ['PublishingManage'],
  leads: ['LeadsManage'],
  reports: ['ReportsView'],
};

// Default permissions by role
const ROLE_DEFAULTS: Record<string, string[]> = {
  owner: [
    'PropertiesViewAll', 'PropertiesEditAll',
    'ContactsViewAll', 'ContactsEditAll',
    'AppointmentsViewAll', 'PublishingManage',
    'LeadsManage', 'ReportsView',
  ],
  admin: [
    'PropertiesViewAll', 'PropertiesEditAll',
    'ContactsViewAll', 'ContactsEditAll',
    'AppointmentsViewAll', 'PublishingManage',
    'LeadsManage', 'ReportsView',
  ],
  agent: ['LeadsManage'],
  viewer: [],
};

// Category icons as simple SVGs
function BuildingIcon() {
  return (
    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 21h19.5m-18-18v18m10.5-18v18m6-13.5V21M6.75 6.75h.75m-.75 3h.75m-.75 3h.75m3-6h.75m-.75 3h.75m-.75 3h.75M6.75 21v-3.375c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21M3 3h12m-.75 4.5H21m-3.75 3h.008v.008h-.008v-.008zm0 3h.008v.008h-.008v-.008zm0 3h.008v.008h-.008v-.008z" />
    </svg>
  );
}

function UsersIcon() {
  return (
    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 018.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0111.964-3.07M12 6.375a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zm8.25 2.25a2.625 2.625 0 11-5.25 0 2.625 2.625 0 015.25 0z" />
    </svg>
  );
}

function CalendarIcon() {
  return (
    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 012.25-2.25h13.5A2.25 2.25 0 0121 7.5v11.25m-18 0A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75m-18 0v-7.5A2.25 2.25 0 015.25 9h13.5A2.25 2.25 0 0121 11.25v7.5" />
    </svg>
  );
}

function GlobeIcon() {
  return (
    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M12 21a9.004 9.004 0 008.716-6.747M12 21a9.004 9.004 0 01-8.716-6.747M12 21c2.485 0 4.5-4.03 4.5-9S14.485 3 12 3m0 18c-2.485 0-4.5-4.03-4.5-9S9.515 3 12 3m0 0a8.997 8.997 0 017.843 4.582M12 3a8.997 8.997 0 00-7.843 4.582m15.686 0A11.953 11.953 0 0112 10.5c-2.998 0-5.74-1.1-7.843-2.918m15.686 0A8.959 8.959 0 0121 12c0 .778-.099 1.533-.284 2.253m0 0A17.919 17.919 0 0112 16.5c-3.162 0-6.133-.815-8.716-2.247m0 0A9.015 9.015 0 013 12c0-1.605.42-3.113 1.157-4.418" />
    </svg>
  );
}

function TagIcon() {
  return (
    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M9.568 3H5.25A2.25 2.25 0 003 5.25v4.318c0 .597.237 1.17.659 1.591l9.581 9.581c.699.699 1.78.872 2.607.33a18.095 18.095 0 005.223-5.223c.542-.827.369-1.908-.33-2.607L11.16 3.66A2.25 2.25 0 009.568 3z" />
      <path strokeLinecap="round" strokeLinejoin="round" d="M6 6h.008v.008H6V6z" />
    </svg>
  );
}

function ChartIcon() {
  return (
    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M3 13.125C3 12.504 3.504 12 4.125 12h2.25c.621 0 1.125.504 1.125 1.125v6.75C7.5 20.496 6.996 21 6.375 21h-2.25A1.125 1.125 0 013 19.875v-6.75zM9.75 8.625c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125v11.25c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 01-1.125-1.125V8.625zM16.5 4.125c0-.621.504-1.125 1.125-1.125h2.25C20.496 3 21 3.504 21 4.125v15.75c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 01-1.125-1.125V4.125z" />
    </svg>
  );
}

const CATEGORY_ICONS: Record<string, React.ReactNode> = {
  properties: <BuildingIcon />,
  contacts: <UsersIcon />,
  appointments: <CalendarIcon />,
  publishing: <GlobeIcon />,
  leads: <TagIcon />,
  reports: <ChartIcon />,
};

interface PermissionDetailPanelProps {
  orgId: string;
  userId: string;
  userName: string;
  userRole: Role;
  onBack?: () => void;
}

export function PermissionDetailPanel({
  orgId,
  userId,
  userName,
  userRole,
  onBack,
}: PermissionDetailPanelProps) {
  const t = useTranslations('permissions');
  const { permissions, isLoading, error, refetch } = useUserPermissions(orgId, userId);
  const setPermissionOverride = useSetPermissionOverride(orgId);
  const removePermissionOverride = useRemovePermissionOverride(orgId);
  const { toast } = useToast();

  const [pendingPermissions, setPendingPermissions] = useState<Set<string>>(new Set());
  const [confirmDialog, setConfirmDialog] = useState<{
    permission: string;
    permissionName: string;
  } | null>(null);
  const [isConfirming, setIsConfirming] = useState(false);

  const isOwner = userRole === 'owner';
  const canEdit = isManager(userRole) || isOwner;

  // Build a permission lookup map
  const permissionMap = new Map<string, EffectivePermission>();
  for (const p of permissions) {
    permissionMap.set(p.permission, p);
  }

  const getRoleDefault = useCallback(
    (permission: string): boolean => {
      return ROLE_DEFAULTS[userRole]?.includes(permission) ?? false;
    },
    [userRole],
  );

  const handleToggle = useCallback(
    async (permission: string, currentGranted: boolean, currentSource: string) => {
      const newGranted = !currentGranted;
      const roleDefault = getRoleDefault(permission);

      // If toggling back to role default, remove the override
      if (newGranted === roleDefault && currentSource === 'Override') {
        setPendingPermissions((prev) => new Set(prev).add(permission));
        try {
          await removePermissionOverride(userId, permission);
          toast({
            title: t('toast.overrideRemoved'),
            variant: 'success',
          });
          await refetch();
        } catch (e) {
          if (isApiError(e)) {
            toast({
              title: t('toast.error'),
              variant: 'destructive',
            });
          }
        } finally {
          setPendingPermissions((prev) => {
            const next = new Set(prev);
            next.delete(permission);
            return next;
          });
        }
        return;
      }

      // If creating a deny override (revoking a permission the role normally grants)
      if (!newGranted && roleDefault) {
        const permName = t(`names.${permission}` as Parameters<typeof t>[0]);
        setConfirmDialog({ permission, permissionName: permName });
        return;
      }

      // Otherwise, set the override
      setPendingPermissions((prev) => new Set(prev).add(permission));
      try {
        await setPermissionOverride(userId, permission, newGranted);
        toast({
          title: t('toast.overrideSet'),
          variant: 'success',
        });
        await refetch();
      } catch (e) {
        if (isApiError(e)) {
          toast({
            title: t('toast.error'),
            variant: 'destructive',
          });
        }
      } finally {
        setPendingPermissions((prev) => {
          const next = new Set(prev);
          next.delete(permission);
          return next;
        });
      }
    },
    [userId, getRoleDefault, setPermissionOverride, removePermissionOverride, refetch, toast, t],
  );

  const handleConfirmDeny = useCallback(async () => {
    if (!confirmDialog) return;

    setIsConfirming(true);
    const { permission } = confirmDialog;

    setPendingPermissions((prev) => new Set(prev).add(permission));
    try {
      await setPermissionOverride(userId, permission, false);
      toast({
        title: t('toast.overrideSet'),
        variant: 'success',
      });
      await refetch();
    } catch (e) {
      if (isApiError(e)) {
        toast({
          title: t('toast.error'),
          variant: 'destructive',
        });
      }
    } finally {
      setPendingPermissions((prev) => {
        const next = new Set(prev);
        next.delete(permission);
        return next;
      });
      setIsConfirming(false);
      setConfirmDialog(null);
    }
  }, [confirmDialog, userId, setPermissionOverride, refetch, toast, t]);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-7 w-48" />
        <Skeleton className="h-4 w-64" />
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="space-y-2">
              <Skeleton className="h-4 w-32" />
              <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
                <Skeleton className="h-10 w-full" />
                <Skeleton className="mt-2 h-10 w-full" />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return <ErrorMessage message={t('loadError')} />;
  }

  return (
    <div className="space-y-6">
      <div>
        {onBack ? (
          <button
            type="button"
            onClick={onBack}
            className="mb-4 inline-flex items-center gap-1.5 text-sm font-medium text-slate-600 transition hover:text-slate-900"
          >
            <ArrowLeftIcon className="h-4 w-4" />
            {t('backToPermissions')}
          </button>
        ) : (
          <Link
            href={`/orgs/${orgId}/permissions`}
            className="mb-4 inline-flex items-center gap-1.5 text-sm font-medium text-slate-600 transition hover:text-slate-900"
          >
            <ArrowLeftIcon className="h-4 w-4" />
            {t('backToPermissions')}
          </Link>
        )}

        <div className="flex items-center gap-3">
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">
            {userName}
          </h1>
          <RoleBadge role={userRole} />
        </div>
        <p className="mt-1 text-sm text-slate-500">{t('subtitle')}</p>
      </div>

      {isOwner ? (
        <div className="rounded-xl border border-amber-200 bg-amber-50 p-4">
          <p className="text-sm font-medium text-amber-800">{t('ownerNote')}</p>
        </div>
      ) : null}

      {!canEdit && !isOwner ? (
        <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p className="text-sm font-medium text-slate-600">{t('readOnly')}</p>
        </div>
      ) : null}

      <div className="space-y-6">
        {Object.entries(PERMISSION_CATEGORIES).map(([category, perms]) => (
          <PermissionCategoryGroup
            key={category}
            category={category}
            icon={CATEGORY_ICONS[category]}
          >
            {perms.map((permission) => {
              const effectivePerm = permissionMap.get(permission);
              const granted = effectivePerm?.granted ?? getRoleDefault(permission);
              const source = effectivePerm?.source ?? 'Role Default';
              const isPending = pendingPermissions.has(permission);

              return (
                <div
                  key={permission}
                  className="flex items-center justify-between px-4 py-3"
                >
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-medium text-slate-900">
                        {t(`names.${permission}` as Parameters<typeof t>[0])}
                      </span>
                      <PermissionSourceBadge source={source} />
                    </div>
                    <p className="mt-0.5 text-xs text-slate-500">
                      {t(`descriptions.${permission}` as Parameters<typeof t>[0])}
                    </p>
                  </div>
                  <div className="ml-4 shrink-0">
                    <PermissionToggle
                      enabled={granted}
                      onChange={() => handleToggle(permission, granted, source)}
                      disabled={isOwner || !canEdit}
                      isLoading={isPending}
                    />
                  </div>
                </div>
              );
            })}
          </PermissionCategoryGroup>
        ))}
      </div>

      {confirmDialog ? (
        <OverrideConfirmDialog
          permissionName={confirmDialog.permissionName}
          userName={userName}
          isProcessing={isConfirming}
          onConfirm={handleConfirmDeny}
          onCancel={() => setConfirmDialog(null)}
        />
      ) : null}
    </div>
  );
}
