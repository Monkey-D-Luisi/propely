// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { LeadListView } from '../LeadListView';
import type { LeadListItem } from '@/hooks/useLeads';

const mockItems: LeadListItem[] = [
  {
    id: 'lead-1',
    name: 'Alice Johnson',
    email: 'alice@example.com',
    source: 'Portal',
    propertyId: '00000000-0000-0000-0000-000000000001',
    status: 'New',
    assignedAgentId: null,
    createdAtUtc: '2026-01-10T10:00:00Z',
  },
  {
    id: 'lead-2',
    name: 'Bob Smith',
    email: 'bob@example.com',
    source: null,
    propertyId: '00000000-0000-0000-0000-000000000002',
    status: 'Converted',
    assignedAgentId: '00000000-0000-0000-0000-000000000099',
    createdAtUtc: '2026-02-15T14:30:00Z',
  },
];

describe('LeadListView', () => {
  it('renders table headers', () => {
    renderWithProviders(
      <LeadListView items={mockItems} isLoading={false} onViewDetails={vi.fn()} onChangeStatus={vi.fn()} onConvert={vi.fn()} />,
    );
    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('Email')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
    expect(screen.getByText('Source')).toBeInTheDocument();
    expect(screen.getByText('Created')).toBeInTheDocument();
  });

  it('renders lead rows', () => {
    renderWithProviders(
      <LeadListView items={mockItems} isLoading={false} onViewDetails={vi.fn()} onChangeStatus={vi.fn()} onConvert={vi.fn()} />,
    );
    expect(screen.getByText('Alice Johnson')).toBeInTheDocument();
    expect(screen.getByText('alice@example.com')).toBeInTheDocument();
    expect(screen.getByText('Bob Smith')).toBeInTheDocument();
  });

  it('shows convert button only for non-Converted non-Lost leads', () => {
    renderWithProviders(
      <LeadListView items={mockItems} isLoading={false} onViewDetails={vi.fn()} onChangeStatus={vi.fn()} onConvert={vi.fn()} />,
    );
    const convertButtons = screen.getAllByText('Convert to Contact');
    // Only Alice (New status) should have convert button, not Bob (Converted)
    expect(convertButtons).toHaveLength(1);
  });

  it('calls onViewDetails when name is clicked', async () => {
    const onViewDetails = vi.fn();
    const { user } = renderWithProviders(
      <LeadListView items={mockItems} isLoading={false} onViewDetails={onViewDetails} onChangeStatus={vi.fn()} onConvert={vi.fn()} />,
    );
    await user.click(screen.getByText('Alice Johnson'));
    expect(onViewDetails).toHaveBeenCalledWith('lead-1');
  });

  it('renders skeleton when loading', () => {
    renderWithProviders(
      <LeadListView items={[]} isLoading={true} onViewDetails={vi.fn()} onChangeStatus={vi.fn()} onConvert={vi.fn()} />,
    );
    expect(screen.queryByTestId('lead-list-view')).not.toBeInTheDocument();
  });
});
