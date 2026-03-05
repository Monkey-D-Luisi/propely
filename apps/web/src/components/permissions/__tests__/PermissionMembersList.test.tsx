// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PermissionMembersList } from '@/components/permissions/PermissionMembersList';
import { renderWithProviders, screen } from '@test/utils';

// Mock the hooks
vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(),
  useMembers: vi.fn(),
}));

// Mock next-intl navigation
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [key: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { useCurrentUser, useMembers } from '@/hooks/orgs';

const mockMembers = [
  { userId: 'u1', email: 'alice@example.com', name: 'Alice Owner', role: 'owner' as const },
  { userId: 'u2', email: 'bob@example.com', name: 'Bob Admin', role: 'admin' as const },
  { userId: 'u3', email: 'carol@example.com', name: 'Carol Agent', role: 'agent' as const },
  { userId: 'u4', email: 'dave@example.com', name: null, role: 'viewer' as const },
];

const emptyPagination = {
  pageNumber: 1,
  totalPages: 1,
  totalCount: 4,
  hasPreviousPage: false,
  hasNextPage: false,
};

beforeEach(() => {
  vi.resetAllMocks();
});

describe('PermissionMembersList', () => {
  it('shows loading skeleton when loading', () => {
    vi.mocked(useCurrentUser).mockReturnValue({
      user: null,
      isLoading: true,
      error: null,
    });
    vi.mocked(useMembers).mockReturnValue({
      members: [],
      setMembers: vi.fn(),
      pagination: emptyPagination,
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PermissionMembersList orgId="org-1" />);

    // Should show skeletons, not the title
    expect(screen.queryByText('Permission Management')).not.toBeInTheDocument();
  });

  it('shows error message when loading fails', () => {
    vi.mocked(useCurrentUser).mockReturnValue({
      user: { id: 'u1', email: 'alice@example.com', name: 'Alice', emailVerified: true, isSystemAdmin: false },
      isLoading: false,
      error: null,
    });
    vi.mocked(useMembers).mockReturnValue({
      members: [],
      setMembers: vi.fn(),
      pagination: emptyPagination,
      isLoading: false,
      error: new Error('Network error'),
      refetch: vi.fn(),
    });

    renderWithProviders(<PermissionMembersList orgId="org-1" />);

    expect(screen.getByText('Failed to load permissions')).toBeInTheDocument();
  });

  it('renders page title and subtitle', () => {
    vi.mocked(useCurrentUser).mockReturnValue({
      user: { id: 'u1', email: 'alice@example.com', name: 'Alice', emailVerified: true, isSystemAdmin: false },
      isLoading: false,
      error: null,
    });
    vi.mocked(useMembers).mockReturnValue({
      members: mockMembers,
      setMembers: vi.fn(),
      pagination: emptyPagination,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PermissionMembersList orgId="org-1" />);

    expect(screen.getByText('Permission Management')).toBeInTheDocument();
    expect(screen.getByText('Manage fine-grained permissions for branch members')).toBeInTheDocument();
  });

  it('renders back to members link', () => {
    vi.mocked(useCurrentUser).mockReturnValue({
      user: { id: 'u1', email: 'alice@example.com', name: 'Alice', emailVerified: true, isSystemAdmin: false },
      isLoading: false,
      error: null,
    });
    vi.mocked(useMembers).mockReturnValue({
      members: mockMembers,
      setMembers: vi.fn(),
      pagination: emptyPagination,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PermissionMembersList orgId="org-1" />);

    expect(screen.getByText('Back to Members')).toBeInTheDocument();
  });

  it('renders all member names in the table', () => {
    vi.mocked(useCurrentUser).mockReturnValue({
      user: { id: 'u1', email: 'alice@example.com', name: 'Alice', emailVerified: true, isSystemAdmin: false },
      isLoading: false,
      error: null,
    });
    vi.mocked(useMembers).mockReturnValue({
      members: mockMembers,
      setMembers: vi.fn(),
      pagination: emptyPagination,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PermissionMembersList orgId="org-1" />);

    expect(screen.getByText('Alice Owner')).toBeInTheDocument();
    expect(screen.getByText('Bob Admin')).toBeInTheDocument();
    expect(screen.getByText('Carol Agent')).toBeInTheDocument();
    // Dave has null name, should show email
    expect(screen.getByText('dave@example.com')).toBeInTheDocument();
  });

  it('renders role badges', () => {
    vi.mocked(useCurrentUser).mockReturnValue({
      user: { id: 'u1', email: 'alice@example.com', name: 'Alice', emailVerified: true, isSystemAdmin: false },
      isLoading: false,
      error: null,
    });
    vi.mocked(useMembers).mockReturnValue({
      members: mockMembers,
      setMembers: vi.fn(),
      pagination: emptyPagination,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PermissionMembersList orgId="org-1" />);

    expect(screen.getByText('Owner')).toBeInTheDocument();
    expect(screen.getByText('Admin')).toBeInTheDocument();
    expect(screen.getByText('Agent')).toBeInTheDocument();
    expect(screen.getByText('Viewer')).toBeInTheDocument();
  });

  it('renders manage links for each member', () => {
    vi.mocked(useCurrentUser).mockReturnValue({
      user: { id: 'u1', email: 'alice@example.com', name: 'Alice', emailVerified: true, isSystemAdmin: false },
      isLoading: false,
      error: null,
    });
    vi.mocked(useMembers).mockReturnValue({
      members: mockMembers,
      setMembers: vi.fn(),
      pagination: emptyPagination,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PermissionMembersList orgId="org-1" />);

    const manageLinks = screen.getAllByRole('link', { name: /Manage/ });
    expect(manageLinks).toHaveLength(4);
  });

  it('shows read-only banner for non-manager users', () => {
    vi.mocked(useCurrentUser).mockReturnValue({
      user: { id: 'u3', email: 'carol@example.com', name: 'Carol', emailVerified: true, isSystemAdmin: false },
      isLoading: false,
      error: null,
    });
    vi.mocked(useMembers).mockReturnValue({
      members: mockMembers,
      setMembers: vi.fn(),
      pagination: emptyPagination,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PermissionMembersList orgId="org-1" />);

    expect(screen.getByText('You can view permissions but cannot make changes')).toBeInTheDocument();
  });

  it('does not show read-only banner for owner', () => {
    vi.mocked(useCurrentUser).mockReturnValue({
      user: { id: 'u1', email: 'alice@example.com', name: 'Alice', emailVerified: true, isSystemAdmin: false },
      isLoading: false,
      error: null,
    });
    vi.mocked(useMembers).mockReturnValue({
      members: mockMembers,
      setMembers: vi.fn(),
      pagination: emptyPagination,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PermissionMembersList orgId="org-1" />);

    expect(screen.queryByText('You can view permissions but cannot make changes')).not.toBeInTheDocument();
  });

  it('shows empty state when no members', () => {
    vi.mocked(useCurrentUser).mockReturnValue({
      user: { id: 'u1', email: 'alice@example.com', name: 'Alice', emailVerified: true, isSystemAdmin: false },
      isLoading: false,
      error: null,
    });
    vi.mocked(useMembers).mockReturnValue({
      members: [],
      setMembers: vi.fn(),
      pagination: emptyPagination,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PermissionMembersList orgId="org-1" />);

    expect(screen.getByText('No members found')).toBeInTheDocument();
  });
});
