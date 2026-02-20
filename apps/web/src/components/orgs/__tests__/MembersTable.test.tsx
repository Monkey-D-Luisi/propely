// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MembersTable } from '@/components/orgs/MembersTable';
import { renderWithProviders, screen } from '@test/utils';
import type { Member } from '@/lib/schemas';

const mockMembers: Member[] = [
  { userId: 'u1', email: 'alice@example.com', name: 'Alice', role: 'owner' },
  { userId: 'u2', email: 'bob@example.com', name: 'Bob', role: 'member' },
  { userId: 'u3', email: 'carol@example.com', name: null, role: 'viewer' },
];

beforeEach(() => {
  vi.resetAllMocks();
});

describe('MembersTable', () => {
  it('renders empty state when no members', () => {
    renderWithProviders(
      <MembersTable members={[]} currentUserId={null} currentUserRole={null} />,
    );

    expect(screen.getByText(/no members/i)).toBeInTheDocument();
  });

  it('renders table headers', () => {
    renderWithProviders(
      <MembersTable members={mockMembers} currentUserId="u1" currentUserRole="owner" />,
    );

    expect(screen.getByText('Email')).toBeInTheDocument();
    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('Role')).toBeInTheDocument();
    expect(screen.getByText('Actions')).toBeInTheDocument();
  });

  it('renders member emails and names', () => {
    renderWithProviders(
      <MembersTable members={mockMembers} currentUserId="u1" currentUserRole="owner" />,
    );

    expect(screen.getByText('alice@example.com')).toBeInTheDocument();
    expect(screen.getByText('Alice')).toBeInTheDocument();
    expect(screen.getByText('bob@example.com')).toBeInTheDocument();
    expect(screen.getByText('Bob')).toBeInTheDocument();
    expect(screen.getByText('carol@example.com')).toBeInTheDocument();
  });

  it('shows dash for members with null name', () => {
    renderWithProviders(
      <MembersTable members={mockMembers} currentUserId="u1" currentUserRole="owner" />,
    );

    // Carol has null name, should show the placeholder
    expect(screen.getByText('—')).toBeInTheDocument();
  });

  it('renders role badges', () => {
    renderWithProviders(
      <MembersTable members={mockMembers} currentUserId="u1" currentUserRole="owner" />,
    );

    expect(screen.getByText('Owner')).toBeInTheDocument();
    expect(screen.getByText('Member')).toBeInTheDocument();
    expect(screen.getByText('Viewer')).toBeInTheDocument();
  });

  it('shows role select dropdown for owner users', () => {
    const onRoleChange = vi.fn();
    renderWithProviders(
      <MembersTable
        members={mockMembers}
        currentUserId="u1"
        currentUserRole="owner"
        onRoleChange={onRoleChange}
      />,
    );

    const selects = screen.getAllByLabelText('Change role');
    // Owner can change roles of other members (bob and carol)
    // Owner can also change their own role since currentUserRole === 'owner'
    expect(selects.length).toBeGreaterThanOrEqual(2);
  });

  it('does not show role select for regular members', () => {
    renderWithProviders(
      <MembersTable
        members={mockMembers}
        currentUserId="u2"
        currentUserRole="member"
      />,
    );

    expect(screen.queryAllByLabelText('Change role')).toHaveLength(0);
  });

  it('calls onRoleChange when role is changed', async () => {
    const onRoleChange = vi.fn();
    const { user } = renderWithProviders(
      <MembersTable
        members={mockMembers}
        currentUserId="u1"
        currentUserRole="owner"
        onRoleChange={onRoleChange}
      />,
    );

    const selects = screen.getAllByLabelText('Change role');
    // Change Bob's role (the second member in the table)
    await user.selectOptions(selects[1], 'admin');

    expect(onRoleChange).toHaveBeenCalled();
  });

  it('disables select for pending role changes', () => {
    const onRoleChange = vi.fn();
    renderWithProviders(
      <MembersTable
        members={mockMembers}
        currentUserId="u1"
        currentUserRole="owner"
        pendingIds={['u2']}
        onRoleChange={onRoleChange}
      />,
    );

    // Bob's select should show "Updating..." text instead of select
    expect(screen.getByText('Updating…')).toBeInTheDocument();
  });

  it('shows no-permission text for non-manager roles', () => {
    renderWithProviders(
      <MembersTable
        members={mockMembers}
        currentUserId="u3"
        currentUserRole="viewer"
      />,
    );

    const noPermTexts = screen.getAllByText('No permission');
    expect(noPermTexts.length).toBeGreaterThan(0);
  });

  it('shows remove button for owner viewing other members', () => {
    const onRemove = vi.fn();
    renderWithProviders(
      <MembersTable
        members={mockMembers}
        currentUserId="u1"
        currentUserRole="owner"
        onRemove={onRemove}
      />,
    );

    // Owner should see Remove buttons for non-self members (Bob and Carol)
    const removeButtons = screen.getAllByText('Remove');
    expect(removeButtons.length).toBe(2);
  });

  it('does not show remove button for the current user row', () => {
    const onRemove = vi.fn();
    renderWithProviders(
      <MembersTable
        members={mockMembers}
        currentUserId="u1"
        currentUserRole="owner"
        onRemove={onRemove}
      />,
    );

    // Alice (u1) is the current user — should not see Remove for herself
    const removeButtons = screen.getAllByText('Remove');
    expect(removeButtons.length).toBe(2); // only Bob and Carol
  });

  it('does not show remove button for admin viewing other admins/owners', () => {
    const membersWithAdmin: Member[] = [
      { userId: 'u1', email: 'alice@example.com', name: 'Alice', role: 'owner' },
      { userId: 'u2', email: 'bob@example.com', name: 'Bob', role: 'admin' },
      { userId: 'u3', email: 'carol@example.com', name: 'Carol', role: 'member' },
    ];
    const onRemove = vi.fn();
    renderWithProviders(
      <MembersTable
        members={membersWithAdmin}
        currentUserId="u2"
        currentUserRole="admin"
        onRemove={onRemove}
      />,
    );

    // Admin (u2) can only remove members, not owners or other admins
    const removeButtons = screen.getAllByText('Remove');
    expect(removeButtons.length).toBe(1); // only Carol (member)
  });

  it('calls onRemove when remove button is clicked', async () => {
    const onRemove = vi.fn();
    const { user } = renderWithProviders(
      <MembersTable
        members={mockMembers}
        currentUserId="u1"
        currentUserRole="owner"
        onRemove={onRemove}
      />,
    );

    const removeButtons = screen.getAllByText('Remove');
    await user.click(removeButtons[0]);

    expect(onRemove).toHaveBeenCalledWith(mockMembers[1]); // Bob
  });
});
