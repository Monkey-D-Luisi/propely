// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { WorkItemDetail } from '@/components/work-items/WorkItemDetail';

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

vi.mock('@/hooks/work-items', () => ({
  useWorkItem: vi.fn(),
  useDeleteWorkItem: vi.fn(),
}));

import { useWorkItem, useDeleteWorkItem } from '@/hooks/work-items';

const mockUseWorkItem = vi.mocked(useWorkItem);
const mockUseDeleteWorkItem = vi.mocked(useDeleteWorkItem);

const mockItem = {
  id: '11111111-1111-1111-1111-111111111111',
  orgId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  title: 'Fix login bug',
  description: 'Users cannot log in with special characters',
  status: 'Active' as const,
  priority: 'High',
  type: 'Bug',
  dueDateUtc: '2026-02-01T00:00:00Z',
  estimatedEffort: 'M',
  createdAtUtc: '2026-01-15T10:30:00Z',
  updatedAtUtc: '2026-01-16T14:00:00Z',
};

beforeEach(() => {
  vi.resetAllMocks();
  mockUseDeleteWorkItem.mockReturnValue(vi.fn().mockResolvedValue(undefined));
});

describe('WorkItemDetail', () => {
  it('shows loading skeleton when isLoading is true', () => {
    mockUseWorkItem.mockReturnValue({
      item: null,
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    });

    const { container } = renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(container.querySelector('.animate-pulse')).toBeInTheDocument();
    expect(screen.queryByText('Fix login bug')).not.toBeInTheDocument();
  });

  it('shows error state when error occurs', () => {
    mockUseWorkItem.mockReturnValue({
      item: null,
      isLoading: false,
      error: new Error('Not found'),
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(screen.getByText('Could not load work items.')).toBeInTheDocument();
  });

  it('shows error state when item is null', () => {
    mockUseWorkItem.mockReturnValue({
      item: null,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(screen.getByText('Could not load work items.')).toBeInTheDocument();
  });

  it('renders item details when loaded', () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    // Title appears in both breadcrumb and heading
    const heading = screen.getByRole('heading', { level: 1 });
    expect(heading).toHaveTextContent('Fix login bug');
    expect(screen.getByText('Users cannot log in with special characters')).toBeInTheDocument();
    expect(screen.getByText('Active')).toBeInTheDocument();
  });

  it('renders priority, type, and effort badges when fields are set', () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(screen.getByText('High')).toBeInTheDocument();
    expect(screen.getByText('Bug')).toBeInTheDocument();
    // Effort 'M' is translated to 'Medium' by the EffortBadge component
    expect(screen.getByText('Medium')).toBeInTheDocument();
  });

  it('does not render property badges when fields are null', () => {
    mockUseWorkItem.mockReturnValue({
      item: { ...mockItem, priority: null, type: null, dueDateUtc: null, estimatedEffort: null },
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    // The property badges section should not render
    expect(screen.queryByText('High')).not.toBeInTheDocument();
    expect(screen.queryByText('Bug')).not.toBeInTheDocument();
  });

  it('renders breadcrumbs with link to list', () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    const listLink = screen.getByText('Work Items');
    expect(listLink.closest('a')).toHaveAttribute('href', '/work-items');
  });

  it('renders edit and delete action buttons', () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(screen.getByText('Edit')).toBeInTheDocument();
    expect(screen.getByText('Delete')).toBeInTheDocument();
  });

  it('shows delete dialog when delete button is clicked', async () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    const { user } = renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    await user.click(screen.getByText('Delete'));

    await waitFor(() => {
      expect(screen.getByText('Delete Work Item')).toBeInTheDocument();
      expect(screen.getByText('Are you sure you want to delete this work item? This action cannot be undone.')).toBeInTheDocument();
    });
  });

  it('renders no description text when description is null', () => {
    mockUseWorkItem.mockReturnValue({
      item: { ...mockItem, description: null },
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(screen.getByText('No description provided.')).toBeInTheDocument();
  });

  it('renders back to list link', () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(screen.getByText('Back to Work Items')).toBeInTheDocument();
  });

  it('renders item ID in metadata section', () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(screen.getByText('11111111-1111-1111-1111-111111111111')).toBeInTheDocument();
  });

  it('calls deleteWorkItem when confirm button is clicked in dialog', async () => {
    const mockDeleteFn = vi.fn().mockResolvedValue(undefined);
    mockUseDeleteWorkItem.mockReturnValue(mockDeleteFn);
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    const { user } = renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    // Open the delete dialog
    await user.click(screen.getByText('Delete'));

    await waitFor(() => {
      expect(screen.getByText('Delete Work Item')).toBeInTheDocument();
    });

    // Find and click the confirm "Delete" button inside the dialog
    const dialogButtons = screen.getAllByRole('button');
    const confirmButton = dialogButtons.find(
      (btn) => btn.textContent === 'Delete' && btn.className.includes('bg-red-600'),
    );
    expect(confirmButton).toBeDefined();
    await user.click(confirmButton!);

    await waitFor(() => {
      expect(mockDeleteFn).toHaveBeenCalledWith('11111111-1111-1111-1111-111111111111');
    });
  });

  it('closes delete dialog when cancel button is clicked', async () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    const { user } = renderWithProviders(
      <WorkItemDetail id="11111111-1111-1111-1111-111111111111" />,
    );

    // Open the delete dialog
    await user.click(screen.getByText('Delete'));

    await waitFor(() => {
      expect(screen.getByText('Delete Work Item')).toBeInTheDocument();
    });

    // Click cancel
    await user.click(screen.getByText('Cancel'));

    await waitFor(() => {
      expect(screen.queryByText('Delete Work Item')).not.toBeInTheDocument();
    });
  });
});
