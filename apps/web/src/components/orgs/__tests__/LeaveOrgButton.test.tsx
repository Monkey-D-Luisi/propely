// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LeaveOrgButton } from '@/components/orgs/LeaveOrgButton';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import type { Member } from '@/lib/schemas';

vi.mock('@/hooks/orgs', () => ({
  useLeaveOrg: vi.fn(),
}));

import { useLeaveOrg } from '@/hooks/orgs';

const mockUseLeaveOrg = vi.mocked(useLeaveOrg);
const ORG_ID = '00000000-0000-0000-0000-000000000010';

const mockMembers: Member[] = [
  { userId: 'u1', email: 'owner@example.com', name: 'Owner', role: 'owner' },
  { userId: 'u2', email: 'member@example.com', name: 'Member', role: 'agent' },
];

beforeEach(() => {
  vi.resetAllMocks();
  mockUseLeaveOrg.mockReturnValue(vi.fn().mockResolvedValue({ ok: true }));
});

describe('LeaveOrgButton', () => {
  it('renders nothing when currentMember is null', () => {
    renderWithProviders(
      <LeaveOrgButton orgId={ORG_ID} members={mockMembers} currentMember={null} />,
    );

    expect(screen.queryByRole('button', { name: 'Leave organization' })).not.toBeInTheDocument();
  });

  it('renders the leave button when currentMember is set', () => {
    renderWithProviders(
      <LeaveOrgButton orgId={ORG_ID} members={mockMembers} currentMember={mockMembers[1]} />,
    );

    expect(screen.getByRole('button', { name: 'Leave organization' })).toBeInTheDocument();
  });

  it('opens confirmation dialog when button is clicked', async () => {
    const { user } = renderWithProviders(
      <LeaveOrgButton orgId={ORG_ID} members={mockMembers} currentMember={mockMembers[1]} />,
    );

    await user.click(screen.getByRole('button', { name: 'Leave organization' }));

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByText('Leave the organization?')).toBeInTheDocument();
    expect(screen.getByText(/lose immediate access/i)).toBeInTheDocument();
  });

  it('closes dialog when cancel is clicked', async () => {
    const { user } = renderWithProviders(
      <LeaveOrgButton orgId={ORG_ID} members={mockMembers} currentMember={mockMembers[1]} />,
    );

    await user.click(screen.getByRole('button', { name: 'Leave organization' }));
    expect(screen.getByRole('dialog')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  it('calls leaveOrg and shows success toast on confirm', async () => {
    const leaveFn = vi.fn().mockResolvedValue({ ok: true });
    mockUseLeaveOrg.mockReturnValue(leaveFn);
    const onAfterLeave = vi.fn();

    const { user } = renderWithProviders(
      <LeaveOrgButton
        orgId={ORG_ID}
        members={mockMembers}
        currentMember={mockMembers[1]}
        onAfterLeave={onAfterLeave}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Leave organization' }));
    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(leaveFn).toHaveBeenCalledWith({
        members: mockMembers,
        currentMember: mockMembers[1],
      });
    });

    await waitFor(() => {
      expect(screen.getByText('You left the organization')).toBeInTheDocument();
    });
  });

  it('shows last-owner toast when leave returns LAST_OWNER', async () => {
    const singleOwnerMembers: Member[] = [
      { userId: 'u1', email: 'owner@example.com', name: 'Owner', role: 'owner' },
    ];

    const leaveFn = vi.fn().mockResolvedValue({ ok: false, reason: 'LAST_OWNER' });
    mockUseLeaveOrg.mockReturnValue(leaveFn);

    const { user } = renderWithProviders(
      <LeaveOrgButton
        orgId={ORG_ID}
        members={singleOwnerMembers}
        currentMember={singleOwnerMembers[0]}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Leave organization' }));
    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(screen.getByText(/can't leave the organization/i)).toBeInTheDocument();
    });
  });

  it('shows error toast on API failure', async () => {
    const leaveFn = vi.fn().mockRejectedValue(new Error('Server error'));
    mockUseLeaveOrg.mockReturnValue(leaveFn);

    const { user } = renderWithProviders(
      <LeaveOrgButton orgId={ORG_ID} members={mockMembers} currentMember={mockMembers[1]} />,
    );

    await user.click(screen.getByRole('button', { name: 'Leave organization' }));
    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(screen.getByText(/couldn't process/i)).toBeInTheDocument();
    });
  });

  it('closes dialog after successful leave', async () => {
    const leaveFn = vi.fn().mockResolvedValue({ ok: true });
    mockUseLeaveOrg.mockReturnValue(leaveFn);

    const { user } = renderWithProviders(
      <LeaveOrgButton orgId={ORG_ID} members={mockMembers} currentMember={mockMembers[1]} />,
    );

    await user.click(screen.getByRole('button', { name: 'Leave organization' }));
    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });
});
