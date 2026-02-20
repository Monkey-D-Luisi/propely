// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { WorkItemsTable } from '@/components/work-items/WorkItemsTable';
import type { WorkItem } from '@/lib/schemas';

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

const mockItems: WorkItem[] = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    orgId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    title: 'Fix login bug',
    description: 'Users cannot log in with special characters',
    status: 'Active',
    priority: 'High',
    type: 'Bug',
    dueDateUtc: '2026-02-01T00:00:00Z',
    estimatedEffort: 'M',
    createdAtUtc: '2026-01-15T10:30:00Z',
    updatedAtUtc: '2026-01-16T14:00:00Z',
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    orgId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    title: 'Add dark mode',
    description: null,
    status: 'Pending',
    priority: null,
    type: null,
    dueDateUtc: null,
    estimatedEffort: null,
    createdAtUtc: '2026-01-17T08:00:00Z',
    updatedAtUtc: '2026-01-17T08:00:00Z',
  },
];

beforeEach(() => {
  vi.resetAllMocks();
});

describe('WorkItemsTable', () => {
  it('renders table with items showing title, status, and action links', () => {
    renderWithProviders(
      <WorkItemsTable items={mockItems} isLoading={false} onDelete={vi.fn()} />,
    );

    expect(screen.getByRole('table')).toBeInTheDocument();

    // Titles
    expect(screen.getByText('Fix login bug')).toBeInTheDocument();
    expect(screen.getByText('Add dark mode')).toBeInTheDocument();

    // Status badges
    expect(screen.getByText('Active')).toBeInTheDocument();
    expect(screen.getByText('Pending')).toBeInTheDocument();

    // Action links
    const viewLinks = screen.getAllByText('View');
    expect(viewLinks).toHaveLength(2);
    const editLinks = screen.getAllByText('Edit');
    expect(editLinks).toHaveLength(2);
    const deleteButtons = screen.getAllByText('Delete');
    expect(deleteButtons).toHaveLength(2);
  });

  it('renders column headers', () => {
    renderWithProviders(
      <WorkItemsTable items={mockItems} isLoading={false} onDelete={vi.fn()} />,
    );

    expect(screen.getByText('Title')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
    expect(screen.getByText('Priority')).toBeInTheDocument();
    expect(screen.getByText('Type')).toBeInTheDocument();
    expect(screen.getByText('Created')).toBeInTheDocument();
    expect(screen.getByText('Updated')).toBeInTheDocument();
    expect(screen.getByText('Actions')).toBeInTheDocument();
  });

  it('renders links with correct hrefs', () => {
    renderWithProviders(
      <WorkItemsTable items={mockItems} isLoading={false} onDelete={vi.fn()} />,
    );

    // Title links
    const titleLink = screen.getByText('Fix login bug').closest('a');
    expect(titleLink).toHaveAttribute('href', '/work-items/11111111-1111-1111-1111-111111111111');

    // View links
    const viewLinks = screen.getAllByText('View');
    expect(viewLinks[0].closest('a')).toHaveAttribute(
      'href',
      '/work-items/11111111-1111-1111-1111-111111111111',
    );

    // Edit links
    const editLinks = screen.getAllByText('Edit');
    expect(editLinks[0].closest('a')).toHaveAttribute(
      'href',
      '/work-items/11111111-1111-1111-1111-111111111111/edit',
    );
  });

  it('shows loading skeleton when isLoading is true', () => {
    renderWithProviders(
      <WorkItemsTable items={[]} isLoading={true} onDelete={vi.fn()} />,
    );

    // Should not show any item text
    expect(screen.queryByText('Fix login bug')).not.toBeInTheDocument();
    // Table should still be present (skeleton rows are inside tbody)
    expect(screen.getByRole('table')).toBeInTheDocument();
    // Should not show empty state
    expect(screen.queryByText('No work items found.')).not.toBeInTheDocument();
  });

  it('shows empty state when items is empty', () => {
    renderWithProviders(
      <WorkItemsTable items={[]} isLoading={false} onDelete={vi.fn()} />,
    );

    expect(screen.getByText('No work items found.')).toBeInTheDocument();
  });

  it('calls onDelete callback when delete button is clicked', async () => {
    const onDelete = vi.fn();
    const { user } = renderWithProviders(
      <WorkItemsTable items={mockItems} isLoading={false} onDelete={onDelete} />,
    );

    const deleteButtons = screen.getAllByText('Delete');
    await user.click(deleteButtons[0]);

    expect(onDelete).toHaveBeenCalledWith('11111111-1111-1111-1111-111111111111');
  });

  it('calls onDelete with correct id for second item', async () => {
    const onDelete = vi.fn();
    const { user } = renderWithProviders(
      <WorkItemsTable items={mockItems} isLoading={false} onDelete={onDelete} />,
    );

    const deleteButtons = screen.getAllByText('Delete');
    await user.click(deleteButtons[1]);

    expect(onDelete).toHaveBeenCalledWith('22222222-2222-2222-2222-222222222222');
  });
});
