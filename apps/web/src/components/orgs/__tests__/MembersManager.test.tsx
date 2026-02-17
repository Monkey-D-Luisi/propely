// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MembersManager } from '@/components/orgs/MembersManager';
import { renderWithProviders, screen } from '@test/utils';
import type { Member, Me } from '@/lib/schemas';
import { ApiError } from '@/lib/api';

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return { ...actual, apiFetch: vi.fn() };
});
vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(),
  useMembers: vi.fn(),
  useUpdateRole: vi.fn(),
  useRemoveMember: vi.fn(() => vi.fn()),
  useInviteMember: vi.fn(() => vi.fn().mockResolvedValue({ ok: true })),
  useLeaveOrg: vi.fn(() => vi.fn().mockResolvedValue({ ok: true })),
}));
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { useCurrentUser, useMembers, useUpdateRole } from '@/hooks/orgs';

const mockUseCurrentUser = vi.mocked(useCurrentUser);
const mockUseMembers = vi.mocked(useMembers);
const mockUseUpdateRole = vi.mocked(useUpdateRole);

const ORG_ID = '00000000-0000-0000-0000-000000000010';

const mockUser: Me = {
  id: 'u1',
  email: 'owner@example.com',
  name: 'Owner',
  emailVerified: true,
};

const mockMembers: Member[] = [
  { userId: 'u1', email: 'owner@example.com', name: 'Owner', role: 'owner' },
  { userId: 'u2', email: 'bob@example.com', name: 'Bob', role: 'member' },
  { userId: 'u3', email: 'carol@example.com', name: 'Carol', role: 'viewer' },
];

const mockPagination = {
  pageNumber: 1,
  totalPages: 1,
  totalCount: 3,
  hasPreviousPage: false,
  hasNextPage: false,
};

beforeEach(() => {
  vi.resetAllMocks();

  mockUseCurrentUser.mockReturnValue({
    user: mockUser,
    isLoading: false,
    error: null,
  });

  mockUseMembers.mockReturnValue({
    members: mockMembers,
    pagination: mockPagination,
    isLoading: false,
    error: null,
    refetch: vi.fn(),
    setMembers: vi.fn(),
  });

  mockUseUpdateRole.mockReturnValue(vi.fn().mockResolvedValue(undefined));
});

describe('MembersManager', () => {
  it('renders loading skeletons when data is loading', () => {
    mockUseCurrentUser.mockReturnValue({
      user: null,
      isLoading: true,
      error: null,
    });
    mockUseMembers.mockReturnValue({
      members: [],
      pagination: mockPagination,
      isLoading: true,
      error: null,
      refetch: vi.fn(),
      setMembers: vi.fn(),
    });

    renderWithProviders(<MembersManager orgId={ORG_ID} />);

    // Loading state shows skeletons (no heading visible)
    expect(screen.queryByRole('heading', { name: 'Members' })).not.toBeInTheDocument();
  });

  it('renders members list after loading', () => {
    renderWithProviders(<MembersManager orgId={ORG_ID} />);

    expect(screen.getByRole('heading', { name: 'Members' })).toBeInTheDocument();
    expect(screen.getByText('owner@example.com')).toBeInTheDocument();
    expect(screen.getByText('bob@example.com')).toBeInTheDocument();
    expect(screen.getByText('carol@example.com')).toBeInTheDocument();
  });

  it('shows "your role" text for current member', () => {
    renderWithProviders(<MembersManager orgId={ORG_ID} />);

    // The role badge should be displayed in the header
    const allOwnerTexts = screen.getAllByText('Owner');
    expect(allOwnerTexts.length).toBeGreaterThanOrEqual(1);
  });

  it('shows invite form for manager (owner) users', () => {
    renderWithProviders(<MembersManager orgId={ORG_ID} />);

    expect(screen.getByRole('heading', { name: 'Invite a member' })).toBeInTheDocument();
  });

  it('hides invite form for non-manager users', () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: 'u3', email: 'carol@example.com', name: 'Carol', emailVerified: true },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<MembersManager orgId={ORG_ID} />);

    expect(screen.queryByRole('heading', { name: 'Invite a member' })).not.toBeInTheDocument();
  });

  it('shows error message when members fail to load', () => {
    mockUseMembers.mockReturnValue({
      members: [],
      pagination: mockPagination,
      isLoading: false,
      error: new Error('Failed to load'),
      refetch: vi.fn(),
      setMembers: vi.fn(),
    });

    renderWithProviders(<MembersManager orgId={ORG_ID} />);

    expect(screen.getByText(/couldn't load the members/i)).toBeInTheDocument();
  });

  it('shows permission denied when 403 error occurs', () => {
    mockUseMembers.mockReturnValue({
      members: [],
      pagination: mockPagination,
      isLoading: false,
      error: new ApiError('Forbidden', 403),
      refetch: vi.fn(),
      setMembers: vi.fn(),
    });

    renderWithProviders(<MembersManager orgId={ORG_ID} />);

    expect(screen.getByText('Insufficient permissions')).toBeInTheDocument();
  });

  it('shows refresh button', () => {
    renderWithProviders(<MembersManager orgId={ORG_ID} />);

    expect(screen.getByText('Refresh list')).toBeInTheDocument();
  });

  it('calls refetch when refresh button is clicked', async () => {
    const refetch = vi.fn();
    mockUseMembers.mockReturnValue({
      members: mockMembers,
      pagination: mockPagination,
      isLoading: false,
      error: null,
      refetch,
      setMembers: vi.fn(),
    });

    const { user } = renderWithProviders(<MembersManager orgId={ORG_ID} />);

    await user.click(screen.getByText('Refresh list'));

    expect(refetch).toHaveBeenCalled();
  });

  it('shows quick actions section with leave button', () => {
    renderWithProviders(<MembersManager orgId={ORG_ID} />);

    expect(screen.getByText('Quick actions')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Leave organization' })).toBeInTheDocument();
  });

  it('shows "not a member" text when current user is not in the org', () => {
    mockUseCurrentUser.mockReturnValue({
      user: { id: 'unknown', email: 'unknown@example.com', name: 'Unknown', emailVerified: true },
      isLoading: false,
      error: null,
    });

    renderWithProviders(<MembersManager orgId={ORG_ID} />);

    expect(screen.getByText(/not part of this organization/i)).toBeInTheDocument();
  });
});
