// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PermissionDetailPanel } from '@/components/permissions/PermissionDetailPanel';
import { renderWithProviders, screen } from '@test/utils';

// Mock hooks
vi.mock('@/hooks/permissions', () => ({
  useUserPermissions: vi.fn(),
  useSetPermissionOverride: vi.fn(),
  useRemovePermissionOverride: vi.fn(),
}));

// Mock next-intl navigation
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [key: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

import { useUserPermissions, useSetPermissionOverride, useRemovePermissionOverride } from '@/hooks/permissions';

const agentPermissions = [
  { permission: 'PropertiesViewAll', granted: false, source: 'Role Default' },
  { permission: 'PropertiesEditAll', granted: false, source: 'Role Default' },
  { permission: 'ContactsViewAll', granted: false, source: 'Role Default' },
  { permission: 'ContactsEditAll', granted: false, source: 'Role Default' },
  { permission: 'AppointmentsViewAll', granted: false, source: 'Role Default' },
  { permission: 'PublishingManage', granted: false, source: 'Role Default' },
  { permission: 'LeadsManage', granted: true, source: 'Role Default' },
  { permission: 'ReportsView', granted: false, source: 'Role Default' },
];

const agentWithOverrides = [
  { permission: 'PropertiesViewAll', granted: true, source: 'Override' },
  { permission: 'PropertiesEditAll', granted: false, source: 'Role Default' },
  { permission: 'ContactsViewAll', granted: false, source: 'Role Default' },
  { permission: 'ContactsEditAll', granted: false, source: 'Role Default' },
  { permission: 'AppointmentsViewAll', granted: false, source: 'Role Default' },
  { permission: 'PublishingManage', granted: false, source: 'Role Default' },
  { permission: 'LeadsManage', granted: true, source: 'Role Default' },
  { permission: 'ReportsView', granted: false, source: 'Override' },
];

const ownerPermissions = [
  { permission: 'PropertiesViewAll', granted: true, source: 'Role Default' },
  { permission: 'PropertiesEditAll', granted: true, source: 'Role Default' },
  { permission: 'ContactsViewAll', granted: true, source: 'Role Default' },
  { permission: 'ContactsEditAll', granted: true, source: 'Role Default' },
  { permission: 'AppointmentsViewAll', granted: true, source: 'Role Default' },
  { permission: 'PublishingManage', granted: true, source: 'Role Default' },
  { permission: 'LeadsManage', granted: true, source: 'Role Default' },
  { permission: 'ReportsView', granted: true, source: 'Role Default' },
];

beforeEach(() => {
  vi.resetAllMocks();
  vi.mocked(useSetPermissionOverride).mockReturnValue(vi.fn());
  vi.mocked(useRemovePermissionOverride).mockReturnValue(vi.fn());
});

describe('PermissionDetailPanel', () => {
  it('shows loading skeleton when loading', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: [],
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u3"
        userName="Carol Agent"
        userRole="agent"
        viewerCanManage={true}
      />,
    );

    expect(screen.queryByText('Carol Agent')).not.toBeInTheDocument();
  });

  it('shows error message on error', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: [],
      isLoading: false,
      error: new Error('Network error'),
      refetch: vi.fn(),
    });

    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u3"
        userName="Carol Agent"
        userRole="agent"
        viewerCanManage={true}
      />,
    );

    expect(screen.getByText('Failed to load permissions')).toBeInTheDocument();
  });

  it('renders user name and role badge', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: agentPermissions,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u3"
        userName="Carol Agent"
        userRole="agent"
        viewerCanManage={true}
      />,
    );

    expect(screen.getByText('Carol Agent')).toBeInTheDocument();
    expect(screen.getByText('Agent')).toBeInTheDocument();
  });

  it('renders all 6 permission categories', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: agentPermissions,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u3"
        userName="Carol Agent"
        userRole="agent"
        viewerCanManage={true}
      />,
    );

    expect(screen.getByText('Properties')).toBeInTheDocument();
    expect(screen.getByText('Contacts')).toBeInTheDocument();
    expect(screen.getByText('Appointments')).toBeInTheDocument();
    expect(screen.getByText('Publishing')).toBeInTheDocument();
    expect(screen.getByText('Leads')).toBeInTheDocument();
    expect(screen.getByText('Reports')).toBeInTheDocument();
  });

  it('renders all 8 permission names', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: agentPermissions,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u3"
        userName="Carol Agent"
        userRole="agent"
        viewerCanManage={true}
      />,
    );

    expect(screen.getByText('View all properties')).toBeInTheDocument();
    expect(screen.getByText('Edit all properties')).toBeInTheDocument();
    expect(screen.getByText('View all contacts')).toBeInTheDocument();
    expect(screen.getByText('Edit all contacts')).toBeInTheDocument();
    expect(screen.getByText('View all appointments')).toBeInTheDocument();
    expect(screen.getByText('Manage publishing')).toBeInTheDocument();
    expect(screen.getByText('Manage leads')).toBeInTheDocument();
    expect(screen.getByText('View reports')).toBeInTheDocument();
  });

  it('renders 8 toggle switches for agent permissions', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: agentPermissions,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u3"
        userName="Carol Agent"
        userRole="agent"
        viewerCanManage={true}
      />,
    );

    const switches = screen.getAllByRole('switch');
    expect(switches).toHaveLength(8);
  });

  it('shows source badges for each permission', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: agentWithOverrides,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u3"
        userName="Carol Agent"
        userRole="agent"
        viewerCanManage={true}
      />,
    );

    const roleBadges = screen.getAllByText('Role Default');
    const overrideBadges = screen.getAllByText('Override');
    expect(roleBadges.length).toBe(6);
    expect(overrideBadges.length).toBe(2);
  });

  it('shows owner note banner for owner users', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: ownerPermissions,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u1"
        userName="Alice Owner"
        userRole="owner"
        viewerCanManage={true}
      />,
    );

    expect(screen.getByText('Owners always have full access. Permissions cannot be modified.')).toBeInTheDocument();
  });

  it('disables all toggles for owner users', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: ownerPermissions,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u1"
        userName="Alice Owner"
        userRole="owner"
        viewerCanManage={true}
      />,
    );

    const switches = screen.getAllByRole('switch');
    for (const s of switches) {
      expect(s).toBeDisabled();
    }
  });

  it('renders back to permissions link when no onBack provided', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: agentPermissions,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u3"
        userName="Carol Agent"
        userRole="agent"
        viewerCanManage={true}
      />,
    );

    expect(screen.getByText('Back to Permissions')).toBeInTheDocument();
  });

  it('renders back button when onBack is provided', () => {
    vi.mocked(useUserPermissions).mockReturnValue({
      permissions: agentPermissions,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    const onBack = vi.fn();
    renderWithProviders(
      <PermissionDetailPanel
        orgId="org-1"
        userId="u3"
        userName="Carol Agent"
        userRole="agent"
        viewerCanManage={true}
        onBack={onBack}
      />,
    );

    expect(screen.getByText('Back to Permissions')).toBeInTheDocument();
  });
});
