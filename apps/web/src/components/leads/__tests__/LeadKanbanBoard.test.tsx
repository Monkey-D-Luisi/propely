// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { LeadKanbanBoard } from '../LeadKanbanBoard';
import type { LeadListItem } from '@/hooks/useLeads';

const mockItems: LeadListItem[] = [
  {
    id: 'lead-1',
    name: 'New Lead',
    email: 'new@example.com',
    source: 'Portal',
    propertyId: '00000000-0000-0000-0000-000000000001',
    status: 'New',
    assignedAgentId: null,
    createdAtUtc: new Date().toISOString(),
  },
  {
    id: 'lead-2',
    name: 'Contacted Lead',
    email: 'contacted@example.com',
    source: 'Website',
    propertyId: '00000000-0000-0000-0000-000000000002',
    status: 'Contacted',
    assignedAgentId: '00000000-0000-0000-0000-000000000099',
    createdAtUtc: '2026-02-01T10:00:00Z',
  },
];

describe('LeadKanbanBoard', () => {
  it('renders all status columns', () => {
    renderWithProviders(
      <LeadKanbanBoard items={[]} isLoading={false} onChangeStatus={vi.fn()} onViewDetails={vi.fn()} />,
    );
    expect(screen.getByTestId('kanban-column-New')).toBeInTheDocument();
    expect(screen.getByTestId('kanban-column-Contacted')).toBeInTheDocument();
    expect(screen.getByTestId('kanban-column-Qualified')).toBeInTheDocument();
    expect(screen.getByTestId('kanban-column-Converted')).toBeInTheDocument();
    expect(screen.getByTestId('kanban-column-Lost')).toBeInTheDocument();
  });

  it('groups leads into correct columns', () => {
    renderWithProviders(
      <LeadKanbanBoard items={mockItems} isLoading={false} onChangeStatus={vi.fn()} onViewDetails={vi.fn()} />,
    );
    expect(screen.getByTestId('lead-card-lead-1')).toBeInTheDocument();
    expect(screen.getByTestId('lead-card-lead-2')).toBeInTheDocument();
  });

  it('shows item count in column headers', () => {
    renderWithProviders(
      <LeadKanbanBoard items={mockItems} isLoading={false} onChangeStatus={vi.fn()} onViewDetails={vi.fn()} />,
    );
    const newColumn = screen.getByTestId('kanban-column-New');
    expect(newColumn).toHaveTextContent('1');
    const contactedColumn = screen.getByTestId('kanban-column-Contacted');
    expect(contactedColumn).toHaveTextContent('1');
  });

  it('renders skeleton when loading', () => {
    renderWithProviders(
      <LeadKanbanBoard items={[]} isLoading={true} onChangeStatus={vi.fn()} onViewDetails={vi.fn()} />,
    );
    expect(screen.queryByTestId('lead-kanban-board')).not.toBeInTheDocument();
  });
});
