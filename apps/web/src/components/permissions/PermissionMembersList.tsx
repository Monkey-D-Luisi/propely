// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { useCurrentUser, useMembers } from '@/hooks/orgs';
import { RoleBadge } from '@/components/orgs/RoleBadge';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { Skeleton, SkeletonTable } from '@/components/ui/skeleton';
import { ChevronRightIcon, ArrowLeftIcon } from '@/components/ui/icons';
import { Link } from '@/i18n/navigation';
import { isManager } from '@/lib/roles';
import type { Member } from '@/lib/schemas';

interface PermissionMembersListProps {
  orgId: string;
}

export function PermissionMembersList({ orgId }: PermissionMembersListProps) {
  const t = useTranslations('permissions');
  const tOrgs = useTranslations('orgs');
  const { user, isLoading: isUserLoading } = useCurrentUser();
  const { members, isLoading: isMembersLoading, error } = useMembers(orgId);

  const isLoading = isUserLoading || isMembersLoading;

  const currentMember = members.find((m) => m.userId === user?.id) ?? null;
  const myRole = currentMember?.role ?? null;
  const canManage = isManager(myRole);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <header className="flex flex-col gap-1">
          <Skeleton className="h-7 w-48" />
          <Skeleton className="h-4 w-64" />
        </header>
        <SkeletonTable rows={4} columns={3} />
      </div>
    );
  }

  if (error) {
    return <ErrorMessage message={t('loadError')} />;
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-col gap-1">
        <Link
          href={`/orgs/${orgId}/members`}
          className="mb-2 inline-flex items-center gap-1.5 text-sm font-medium text-slate-600 transition hover:text-slate-900"
        >
          <ArrowLeftIcon className="h-4 w-4" />
          {t('backToMembers')}
        </Link>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">
          {t('title')}
        </h1>
        <p className="text-sm text-slate-500">{t('subtitle')}</p>
      </header>

      {!canManage ? (
        <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p className="text-sm font-medium text-slate-600">{t('readOnly')}</p>
        </div>
      ) : null}

      {members.length === 0 ? (
        <div className="rounded-xl border border-slate-200 bg-white p-8 text-center shadow-sm">
          <p className="text-sm text-slate-500">{t('noMembers')}</p>
        </div>
      ) : (
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50">
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                  {tOrgs('members.headerMember')}
                </th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                  {tOrgs('members.headerRole')}
                </th>
                <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                  {t('manageButton')}
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200">
              {members.map((member: Member) => (
                <MemberRow
                  key={member.userId}
                  member={member}
                  orgId={orgId}
                  manageLabel={t('manageButton')}
                />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function MemberRow({
  member,
  orgId,
  manageLabel,
}: {
  member: Member;
  orgId: string;
  manageLabel: string;
}) {
  return (
    <tr className="transition hover:bg-slate-50">
      <td className="px-4 py-3">
        <div>
          <div className="text-sm font-medium text-slate-900">
            {member.name ?? member.email}
          </div>
          {member.name ? (
            <div className="text-xs text-slate-500">{member.email}</div>
          ) : null}
        </div>
      </td>
      <td className="px-4 py-3">
        <RoleBadge role={member.role} />
      </td>
      <td className="px-4 py-3 text-right">
        <Link
          href={`/orgs/${orgId}/permissions/${member.userId}`}
          className="inline-flex items-center gap-1 rounded-lg border border-slate-200 px-3 py-1.5 text-sm font-medium text-slate-700 transition hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
        >
          {manageLabel}
          <ChevronRightIcon className="h-4 w-4" />
        </Link>
      </td>
    </tr>
  );
}
