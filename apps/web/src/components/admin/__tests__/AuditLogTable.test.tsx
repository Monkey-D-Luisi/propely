// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect } from 'vitest';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { AuditLogTable } from '@/components/admin/AuditLogTable';
import type { AuditLog } from '@/lib/schemas';

const mockLogs: AuditLog[] = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    userId: '22222222-2222-2222-2222-222222222222',
    organizationId: '33333333-3333-3333-3333-333333333333',
    action: 'Added',
    entityType: 'Organization',
    entityId: '44444444-4444-4444-4444-444444444444',
    changes: '{"name":"Acme"}',
    correlationId: null,
    createdAtUtc: '2026-01-15T10:30:00Z',
  },
  {
    id: '55555555-5555-5555-5555-555555555555',
    userId: null,
    organizationId: null,
    action: 'Deleted',
    entityType: 'User',
    entityId: '66666666-6666-6666-6666-666666666666',
    changes: null,
    correlationId: 'corr-1',
    createdAtUtc: '2026-01-16T14:00:00Z',
  },
];

describe('AuditLogTable', () => {
  it('shows loading skeleton when isLoading is true', () => {
    renderWithProviders(<AuditLogTable logs={[]} isLoading={true} />);

    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  it('shows empty state when logs array is empty', () => {
    renderWithProviders(<AuditLogTable logs={[]} isLoading={false} />);

    expect(screen.getByText('No audit log entries found.')).toBeInTheDocument();
  });

  it('renders table with log entries', () => {
    renderWithProviders(<AuditLogTable logs={mockLogs} isLoading={false} />);

    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getByText('Added')).toBeInTheDocument();
    expect(screen.getByText('Deleted')).toBeInTheDocument();
    expect(screen.getByText('Organization')).toBeInTheDocument();
    expect(screen.getByText('User')).toBeInTheDocument();
  });

  it('shows expand button only for logs with changes', () => {
    renderWithProviders(<AuditLogTable logs={mockLogs} isLoading={false} />);

    const expandButtons = screen.getAllByRole('button', { name: 'Show changes' });
    expect(expandButtons).toHaveLength(1);
  });

  it('expands and collapses changes when expand button is clicked', async () => {
    const { user } = renderWithProviders(
      <AuditLogTable logs={mockLogs} isLoading={false} />,
    );

    const expandButton = screen.getByRole('button', { name: 'Show changes' });
    expect(expandButton).toHaveAttribute('aria-expanded', 'false');

    await user.click(expandButton);

    await waitFor(() => {
      expect(expandButton).toHaveAttribute('aria-expanded', 'true');
    });

    expect(screen.getByText('Changes')).toBeInTheDocument();

    await user.click(expandButton);

    await waitFor(() => {
      expect(expandButton).toHaveAttribute('aria-expanded', 'false');
    });
  });

  it('renders dash for missing userId', () => {
    renderWithProviders(<AuditLogTable logs={mockLogs} isLoading={false} />);

    expect(screen.getByText('—')).toBeInTheDocument();
  });

  it('renders column headers', () => {
    renderWithProviders(<AuditLogTable logs={mockLogs} isLoading={false} />);

    expect(screen.getByText('Timestamp')).toBeInTheDocument();
    expect(screen.getByText('Action')).toBeInTheDocument();
    expect(screen.getByText('Entity type')).toBeInTheDocument();
    expect(screen.getByText('Entity ID')).toBeInTheDocument();
    expect(screen.getByText('User ID')).toBeInTheDocument();
  });
});
