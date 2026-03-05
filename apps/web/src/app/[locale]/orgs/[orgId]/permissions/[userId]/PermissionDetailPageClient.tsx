// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useMembers } from '@/hooks/orgs';
import { useTranslations } from 'next-intl';
import { PermissionDetailPanel } from '@/components/permissions/PermissionDetailPanel';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { Skeleton } from '@/components/ui/skeleton';

interface PermissionDetailPageClientProps {
  orgId: string;
  userId: string;
}

export function PermissionDetailPageClient({
  orgId,
  userId,
}: PermissionDetailPageClientProps) {
  const t = useTranslations('permissions');
  const { members, isLoading, error } = useMembers(orgId, 1, 100);

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

  const member = members.find((m) => m.userId === userId);

  if (!member) {
    return <ErrorMessage message={t('loadError')} />;
  }

  return (
    <PermissionDetailPanel
      orgId={orgId}
      userId={userId}
      userName={member.name ?? member.email}
      userRole={member.role}
    />
  );
}
