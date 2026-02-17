// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useTranslations } from 'next-intl';
import { RoleBadge } from '@/components/orgs/RoleBadge';
import { RoleSchema, type Member, type Role } from '@/lib/schemas';

interface MembersTableProps {
  members: Member[];
  currentUserId: string | null;
  currentUserRole: Role | null;
  pendingIds?: string[];
  onRoleChange?: (member: Member, role: Role) => void;
  onRemove?: (member: Member) => void;
}

const roleOptionsByMyRole: Record<Role, Role[]> = {
  owner: ['owner', 'admin', 'member', 'viewer'],
  admin: ['admin', 'member', 'viewer'],
  member: ['member'],
  viewer: ['viewer']
};

function canManageRole(myRole: Role | null): boolean {
  return myRole === 'owner' || myRole === 'admin';
}

export function MembersTable({ members, currentUserId, currentUserRole, pendingIds = [], onRoleChange, onRemove }: MembersTableProps) {
  const t = useTranslations('orgs');
  const isManager = canManageRole(currentUserRole ?? null);

  if (members.length === 0) {
    return (
      <div className="rounded-xl border border-dashed border-slate-300 bg-white p-8 text-center text-sm text-slate-500">
        {t('members.emptyState')}
      </div>
    );
  }

  const pendingSet = new Set(pendingIds);

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <table className="min-w-full divide-y divide-slate-200 text-sm">
        <thead className="bg-slate-50/50 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
          <tr>
            <th scope="col" className="px-6 py-4">{t('members.tableHeaders.email')}</th>
            <th scope="col" className="px-6 py-4">{t('members.tableHeaders.name')}</th>
            <th scope="col" className="px-6 py-4">{t('members.tableHeaders.role')}</th>
            <th scope="col" className="px-6 py-4 text-right">{t('members.tableHeaders.actions')}</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-200 bg-white">
          {members.map(member => {
            const isCurrentUser = member.userId === currentUserId;
            const rowClasses = `group hover:bg-slate-50 transition-colors ${isCurrentUser ? 'bg-slate-50/70' : ''}`;
            const options = currentUserRole ? roleOptionsByMyRole[currentUserRole] : [];
            const isPending = pendingSet.has(member.userId);

            const canChange = Boolean(
              isManager &&
              !isPending &&
              onRoleChange &&
              (currentUserRole === 'owner' || (currentUserRole === 'admin' && member.role !== 'owner')) &&
              (!isCurrentUser || currentUserRole === 'owner')
            );

            const canRemove = Boolean(
              isManager &&
              !isCurrentUser &&
              !isPending &&
              onRemove &&
              (currentUserRole === 'owner' || (currentUserRole === 'admin' && member.role !== 'owner' && member.role !== 'admin'))
            );

            return (
              <tr key={member.userId} className={rowClasses}>
                <td className="px-6 py-5 font-medium text-slate-900">{member.email}</td>
                <td className="px-6 py-5 text-slate-600">{member.name ?? t('members.noName')}</td>
                <td className="px-6 py-5">
                  <RoleBadge role={member.role} />
                </td>
                <td className="px-6 py-5 text-right">
                  {canChange || canRemove ? (
                    <div className="flex items-center justify-end gap-2 opacity-0 transition-opacity group-hover:opacity-100 focus-within:opacity-100">
                      {canChange ? (
                        <select
                          aria-label={t('members.changeRole')}
                          value={member.role}
                          onChange={event => {
                            const parsed = RoleSchema.safeParse(event.target.value);
                            if (parsed.success) onRoleChange?.(member, parsed.data);
                          }}
                          className="rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-600"
                          disabled={isPending}
                        >
                          {options.map(option => (
                            <option key={option} value={option}>
                              {t(`roles.${option}`)}
                            </option>
                          ))}
                        </select>
                      ) : null}
                      {canRemove ? (
                        <button
                          type="button"
                          onClick={() => onRemove?.(member)}
                          className="p-1.5 text-slate-400 transition-colors hover:text-red-500"
                        >
                          {t('removeMember.button')}
                        </button>
                      ) : null}
                    </div>
                  ) : (
                    <span className="text-xs text-slate-400">
                      {isPending
                        ? t('members.updating')
                        : isManager
                          ? t('members.noActionsAvailable')
                          : t('members.noPermission')}
                    </span>
                  )}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}