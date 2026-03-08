// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { Button } from '@/components/ui/button';
import { DialogOverlay } from '@/components/ui/dialog-overlay';
import { InviteForm } from '@/components/orgs/InviteForm';
import { LeaveOrgButton } from '@/components/orgs/LeaveOrgButton';
import { MembersTable } from '@/components/orgs/MembersTable';
import { RoleBadge } from '@/components/orgs/RoleBadge';
import { Pagination } from '@/components/ui/pagination';
import { ErrorMessage } from '@/components/ui/ErrorMessage';
import { useToast } from '@/components/ui/toast';
import { Skeleton, SkeletonTable } from '@/components/ui/skeleton';
import { useCurrentUser, useMembers, useUpdateRole, useRemoveMember } from '@/hooks/orgs';
import type { Member, Role } from '@/lib/schemas';
import { isApiError, getDomainErrorCode } from '@/lib/api';
import { isManager } from '@/lib/roles';

export function MembersManager({ orgId }: { orgId: string }) {
  const t = useTranslations('orgs');
  const tErrors = useTranslations('errors');
  const tCommon = useTranslations('common');
  const { user, isLoading: isUserLoading } = useCurrentUser();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const { members, pagination, isLoading: isMembersLoading, error: membersError, refetch, setMembers } = useMembers(orgId, page, 20, debouncedSearch || undefined);
  const [currentMember, setCurrentMember] = useState<Member | null>(null);

  // Debounce search input
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 300);
    return () => clearTimeout(timer);
  }, [search]);

  useEffect(() => {
    if (!user) {
      setCurrentMember(null);
      return;
    }

    const found = members.find(member => member.userId === user.id);
    if (found) {
      setCurrentMember(found);
    }
  }, [members, user]);

  const updateRole = useUpdateRole(orgId);
  const removeMember = useRemoveMember(orgId);
  const { toast } = useToast();
  const router = useRouter();
  const [pendingRoleChanges, setPendingRoleChanges] = useState<string[]>([]);
  const [memberToRemove, setMemberToRemove] = useState<Member | null>(null);
  const [isRemoving, setIsRemoving] = useState(false);
  const isLoading = isUserLoading || isMembersLoading;

  const myRole = currentMember?.role ?? null;
  const canManageMembers = isManager(myRole);

  const handleRoleChange = useCallback(
    async (member: Member, nextRole: Role) => {
      if (member.role === nextRole) return;

      setPendingRoleChanges(prev => [...prev, member.userId]);
      const previousMembers = members;
      setMembers(() =>
        previousMembers.map(item =>
          item.userId === member.userId ? { ...item, role: nextRole } : item
        )
      );

      try {
        await updateRole(member.userId, nextRole);
        toast({
          title: t('roleChange.successTitle'),
          description: t('roleChange.successDescription', { email: member.email, role: t(`roles.${nextRole}`) }),
          variant: 'success'
        });
      } catch (error) {
        setMembers(() => previousMembers);
        if (isApiError(error)) {
          if (error.status === 403) {
            toast({
              title: t('roleChange.permissionErrorTitle'),
              description: t('roleChange.permissionErrorDescription'),
              variant: 'destructive'
            });
          } else if (getDomainErrorCode(error) === 'CANNOT_REMOVE_LAST_OWNER') {
            toast({
              title: t('roleChange.lastOwnerTitle'),
              description: t('roleChange.lastOwnerDescription'),
              variant: 'destructive'
            });
          } else {
            toast({
              title: t('roleChange.errorTitle'),
              description: t('roleChange.errorDescription'),
              variant: 'destructive'
            });
          }
        } else {
          toast({
            title: t('roleChange.errorTitle'),
            description: t('roleChange.errorDescription'),
            variant: 'destructive'
          });
        }
      } finally {
        setPendingRoleChanges(prev => prev.filter(id => id !== member.userId));
      }
    },
    [members, setMembers, updateRole, toast]
  );

  const handleRemoveRequest = useCallback((member: Member) => {
    setMemberToRemove(member);
  }, []);

  const handleRemoveConfirm = useCallback(async () => {
    if (!memberToRemove) return;
    setIsRemoving(true);
    try {
      await removeMember(memberToRemove.userId);
      toast({
        title: t('removeMember.successTitle'),
        description: t('removeMember.successDescription', { name: memberToRemove.name ?? memberToRemove.email }),
        variant: 'success'
      });
      await refetch();
    } catch (error) {
      if (isApiError(error)) {
        if (error.status === 403) {
          toast({ title: t('removeMember.errorTitle'), description: t('removeMember.forbidden'), variant: 'destructive' });
        } else if (getDomainErrorCode(error) === 'CANNOT_REMOVE_LAST_OWNER') {
          toast({ title: t('removeMember.errorTitle'), description: t('removeMember.lastOwner'), variant: 'destructive' });
        } else if (getDomainErrorCode(error) === 'CANNOT_REMOVE_SELF') {
          toast({ title: t('removeMember.errorTitle'), description: t('removeMember.cannotRemoveSelf'), variant: 'destructive' });
        } else {
          toast({ title: t('removeMember.errorTitle'), description: t('removeMember.errorDescription'), variant: 'destructive' });
        }
      } else {
        toast({ title: t('removeMember.errorTitle'), description: t('removeMember.errorDescription'), variant: 'destructive' });
      }
    } finally {
      setIsRemoving(false);
      setMemberToRemove(null);
    }
  }, [memberToRemove, removeMember, toast, refetch]);

  const handleRefetch = useCallback(async () => {
    await refetch();
  }, [refetch]);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <header className="flex flex-col gap-1">
          <Skeleton className="h-7 w-32" />
          <Skeleton className="h-4 w-48" />
        </header>
        <SkeletonTable rows={4} columns={4} />
        <div className="grid gap-6 md:grid-cols-[2fr,1fr]">
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <Skeleton className="h-5 w-32" />
            <div className="mt-3 flex flex-col gap-3">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full rounded-md" />
            </div>
          </div>
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <Skeleton className="h-5 w-28" />
            <Skeleton className="mt-2 h-4 w-full" />
            <Skeleton className="mt-4 h-10 w-full rounded-md" />
          </div>
        </div>
      </div>
    );
  }

  if (membersError) {
    if (isApiError(membersError) && membersError.status === 403) {
      return (
        <div className="mx-auto mt-10 max-w-md space-y-4 text-center">
          <h1 className="text-xl font-semibold text-slate-900">{tErrors('insufficientPermissions.title')}</h1>
          <p className="text-sm text-slate-600">
            {tErrors('insufficientPermissions.membersDescription')}
          </p>
          <Button type="button" onClick={() => router.push('/orgs/mine')}>
            {tCommon('backToOrgs')}
          </Button>
        </div>
      );
    }

    return <ErrorMessage message={t('members.loadError')} />;
  }

  return (
    <div className="space-y-6">
      <header>
        <h1 className="tracking-tight text-3xl font-bold leading-tight text-slate-900">{t('members.title')}</h1>
        {currentMember ? (
          <span className="text-sm text-slate-500">
            {t.rich('members.yourRole', { roleBadge: () => <RoleBadge role={currentMember.role} /> })}
          </span>
        ) : (
          <p className="text-sm text-slate-500">{t('members.notMember')}</p>
        )}
      </header>

      {canManageMembers ? (
        <section className="rounded-xl bg-white border border-slate-200 shadow-sm overflow-hidden">
          <div className="p-6 border-b border-slate-200 bg-slate-50/50">
            <h2 className="text-lg font-semibold leading-tight flex items-center gap-2 text-slate-900">
              <span className="material-symbols-outlined text-primary-600 text-[20px]" aria-hidden="true">person_add</span>
              {t('invite.sectionTitle')}
            </h2>
          </div>
          <div className="p-6">
            <InviteForm orgId={orgId} canInvite={canManageMembers} onInvited={handleRefetch} />
          </div>
        </section>
      ) : null}

      <section className="rounded-xl bg-white border border-slate-200 shadow-sm overflow-hidden">
        <div className="p-5 border-b border-slate-200 flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-slate-50/50">
          <h2 className="text-lg font-semibold text-slate-900 flex items-center gap-2">
            <span className="material-symbols-outlined text-slate-500 text-[20px]" aria-hidden="true">group</span>
            {t('members.title')}
          </h2>
          <div className="relative w-full sm:w-72">
            <span className="material-symbols-outlined text-slate-400 text-[20px] absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none" aria-hidden="true">search</span>
            <input
              type="search"
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder={tCommon('pagination.search')}
              className="form-input w-full rounded-lg border border-slate-300 bg-white h-10 pl-10 pr-4 text-sm focus:ring-2 focus:ring-primary-600/50 focus:border-primary-600"
            />
          </div>
        </div>

        <MembersTable
          members={members}
          currentUserId={currentMember?.userId ?? null}
          currentUserRole={currentMember?.role ?? null}
          pendingIds={pendingRoleChanges}
          onRoleChange={canManageMembers ? handleRoleChange : undefined}
          onRemove={canManageMembers ? handleRemoveRequest : undefined}
        />

        {pagination.totalCount > 0 && (
          <div className="p-4 border-t border-slate-200 flex items-center justify-between text-sm text-slate-500 bg-slate-50/30">
            <span>{tCommon('pagination.showing', { count: members.length, total: pagination.totalCount })}</span>
            <Pagination pagination={pagination} onPageChange={setPage} />
          </div>
        )}
      </section>

      {canManageMembers ? (
        <div className="flex flex-col gap-4 overflow-hidden rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="text-lg font-semibold text-slate-900">{t('quickActions.title')}</h2>
          <p className="text-sm text-slate-500">{t('quickActions.description')}</p>
          <LeaveOrgButton
            orgId={orgId}
            members={members}
            currentMember={currentMember}
            onAfterLeave={() => {
              void refetch();
            }}
          />
        </div>
      ) : null}

      {memberToRemove ? (
        <RemoveConfirmDialog
          memberName={memberToRemove.name ?? memberToRemove.email}
          isRemoving={isRemoving}
          onConfirm={handleRemoveConfirm}
          onCancel={() => setMemberToRemove(null)}
        />
      ) : null}
    </div>
  );
}

function RemoveConfirmDialog({
  memberName,
  isRemoving,
  onConfirm,
  onCancel,
}: {
  memberName: string;
  isRemoving: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}) {
  const t = useTranslations('orgs');
  const tCommon = useTranslations('common');
  const dialogRef = useRef<HTMLDivElement>(null);

  return (
    <DialogOverlay
      dialogRef={dialogRef}
      isProcessing={isRemoving}
      onClose={onCancel}
    >
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="remove-dialog-title"
        className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-xl"
      >
        <h3 id="remove-dialog-title" className="text-lg font-semibold text-slate-900">
          {t('removeMember.dialogTitle')}
        </h3>
        <p className="mt-2 text-sm text-slate-600">
          {t('removeMember.dialogDescription', { name: memberName })}
        </p>
        <div className="mt-6 flex justify-end gap-3">
          <button
            type="button"
            className="rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50 focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
            onClick={() => {
              if (!isRemoving) onCancel();
            }}
          >
            {tCommon('cancel')}
          </button>
          <button
            type="button"
            className="rounded-lg bg-red-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-red-500 disabled:cursor-not-allowed disabled:opacity-60 focus:ring-2 focus:ring-primary-600 focus:ring-offset-2"
            onClick={onConfirm}
            disabled={isRemoving}
          >
            {isRemoving ? t('removeMember.removing') : t('removeMember.confirmButton')}
          </button>
        </div>
      </div>
    </DialogOverlay>
  );
}
