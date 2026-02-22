// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { InviteForm } from '@/components/orgs/InviteForm';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/hooks/orgs', () => ({
  useInviteMember: vi.fn(),
}));

import { useInviteMember } from '@/hooks/orgs';

const mockUseInviteMember = vi.mocked(useInviteMember);
const ORG_ID = '00000000-0000-0000-0000-000000000010';

beforeEach(() => {
  vi.resetAllMocks();
  mockUseInviteMember.mockReturnValue(vi.fn().mockResolvedValue({ ok: true }));
});

describe('InviteForm', () => {
  it('renders nothing when canInvite is false', () => {
    renderWithProviders(
      <InviteForm orgId={ORG_ID} canInvite={false} />,
    );

    expect(screen.queryByRole('heading', { name: 'Invite a member' })).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Email')).not.toBeInTheDocument();
  });

  it('renders the form when canInvite is true', () => {
    renderWithProviders(
      <InviteForm orgId={ORG_ID} canInvite={true} />,
    );

    expect(screen.getByRole('heading', { name: 'Invite a member' })).toBeInTheDocument();
    expect(screen.getByLabelText('Email')).toBeInTheDocument();
    expect(screen.getByLabelText('Initial role')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Send invitation' })).toBeInTheDocument();
  });

  it('shows role options: Admin, Agent, Viewer', () => {
    renderWithProviders(
      <InviteForm orgId={ORG_ID} canInvite={true} />,
    );

    const select = screen.getByLabelText('Initial role');
    const options = select.querySelectorAll('option');
    const values = Array.from(options).map(o => o.getAttribute('value'));

    expect(values).toContain('admin');
    expect(values).toContain('agent');
    expect(values).toContain('viewer');
  });

  it('shows validation error for empty email', async () => {
    const { user } = renderWithProviders(
      <InviteForm orgId={ORG_ID} canInvite={true} />,
    );

    await user.click(screen.getByRole('button', { name: 'Send invitation' }));

    await waitFor(() => {
      expect(screen.getByText('Email is required.')).toBeInTheDocument();
    });
  });

  it('shows validation error for invalid email', async () => {
    const { user } = renderWithProviders(
      <InviteForm orgId={ORG_ID} canInvite={true} />,
    );

    await user.type(screen.getByLabelText('Email'), 'not-an-email');
    await user.click(screen.getByRole('button', { name: 'Send invitation' }));

    await waitFor(() => {
      expect(screen.getByText('Enter a valid email address.')).toBeInTheDocument();
    });
  });

  it('submits successfully and shows success toast', async () => {
    const inviteFn = vi.fn().mockResolvedValue({ ok: true });
    mockUseInviteMember.mockReturnValue(inviteFn);
    const onInvited = vi.fn();

    const { user } = renderWithProviders(
      <InviteForm orgId={ORG_ID} canInvite={true} onInvited={onInvited} />,
    );

    await user.type(screen.getByLabelText('Email'), 'new@example.com');
    await user.click(screen.getByRole('button', { name: 'Send invitation' }));

    await waitFor(() => {
      expect(inviteFn).toHaveBeenCalledWith({ email: 'new@example.com', role: 'agent' });
    });

    await waitFor(() => {
      expect(screen.getByText('Invitation sent')).toBeInTheDocument();
    });
  });

  it('shows error toast on failure', async () => {
    const inviteFn = vi.fn().mockRejectedValue(new Error('Invite failed'));
    mockUseInviteMember.mockReturnValue(inviteFn);

    const { user } = renderWithProviders(
      <InviteForm orgId={ORG_ID} canInvite={true} />,
    );

    await user.type(screen.getByLabelText('Email'), 'fail@example.com');
    await user.click(screen.getByRole('button', { name: 'Send invitation' }));

    await waitFor(() => {
      expect(screen.getByText('Invitation failed')).toBeInTheDocument();
    });
  });

  it('allows changing the role before submitting', async () => {
    const inviteFn = vi.fn().mockResolvedValue({ ok: true });
    mockUseInviteMember.mockReturnValue(inviteFn);

    const { user } = renderWithProviders(
      <InviteForm orgId={ORG_ID} canInvite={true} />,
    );

    await user.type(screen.getByLabelText('Email'), 'admin@example.com');
    await user.selectOptions(screen.getByLabelText('Initial role'), 'admin');
    await user.click(screen.getByRole('button', { name: 'Send invitation' }));

    await waitFor(() => {
      expect(inviteFn).toHaveBeenCalledWith({ email: 'admin@example.com', role: 'admin' });
    });
  });
});
