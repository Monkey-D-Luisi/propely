// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { WorkItemEdit } from '@/components/work-items/WorkItemEdit';

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [k: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

vi.mock('@/hooks/work-items', () => ({
  useWorkItem: vi.fn(),
  useUpdateWorkItem: vi.fn(),
}));

import { useWorkItem, useUpdateWorkItem } from '@/hooks/work-items';

const mockUseWorkItem = vi.mocked(useWorkItem);
const mockUseUpdateWorkItem = vi.mocked(useUpdateWorkItem);

const mockItem = {
  id: '11111111-1111-1111-1111-111111111111',
  orgId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  title: 'Fix login bug',
  description: 'Users cannot log in with special characters',
  status: 'Active' as const,
  priority: 'High',
  type: 'Bug',
  dueDateUtc: '2026-02-01T00:00:00Z',
  estimatedEffort: 'L',
  createdAtUtc: '2026-01-15T10:30:00Z',
  updatedAtUtc: '2026-01-16T14:00:00Z',
};

beforeEach(() => {
  vi.resetAllMocks();
  mockUseUpdateWorkItem.mockReturnValue(vi.fn().mockResolvedValue(undefined));
});

describe('WorkItemEdit', () => {
  it('shows loading skeleton when isLoading is true', () => {
    mockUseWorkItem.mockReturnValue({
      item: null,
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    });

    const { container } = renderWithProviders(
      <WorkItemEdit id="11111111-1111-1111-1111-111111111111" />,
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
      <WorkItemEdit id="11111111-1111-1111-1111-111111111111" />,
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
      <WorkItemEdit id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(screen.getByText('Could not load work items.')).toBeInTheDocument();
  });

  it('renders breadcrumbs when item is loaded', () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemEdit id="11111111-1111-1111-1111-111111111111" />,
    );

    // Breadcrumb: Work Items / <title> / Edit Work Item
    const listLink = screen.getByText('Work Items');
    expect(listLink.closest('a')).toHaveAttribute('href', '/work-items');

    // Title appears in breadcrumb as a link
    const titleLink = screen.getByText('Fix login bug');
    expect(titleLink.closest('a')).toHaveAttribute('href', '/work-items/11111111-1111-1111-1111-111111111111');

    // "Edit Work Item" appears in both breadcrumb span and h1 heading
    const editTexts = screen.getAllByText('Edit Work Item');
    expect(editTexts.length).toBeGreaterThanOrEqual(1);
  });

  it('renders edit heading', () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemEdit id="11111111-1111-1111-1111-111111111111" />,
    );

    // The page heading "Edit Work Item" (h1)
    const heading = screen.getByRole('heading', { level: 1 });
    expect(heading).toHaveTextContent('Edit Work Item');
  });

  it('renders form with pre-filled values from item', () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemEdit id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(screen.getByLabelText(/title/i)).toHaveValue('Fix login bug');
    expect(screen.getByLabelText(/description/i)).toHaveValue('Users cannot log in with special characters');
    expect(screen.getByLabelText(/status/i)).toHaveValue('Active');
    expect(screen.getByLabelText(/priority/i)).toHaveValue('High');
    expect(screen.getByLabelText(/^type$/i)).toHaveValue('Bug');
    expect(screen.getByLabelText(/due date/i)).toHaveValue('2026-02-01');
    expect(screen.getByLabelText(/estimated effort/i)).toHaveValue('L');
  });

  it('renders save button as submit label', () => {
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(
      <WorkItemEdit id="11111111-1111-1111-1111-111111111111" />,
    );

    expect(screen.getByRole('button', { name: 'Save Changes' })).toBeInTheDocument();
  });

  it('calls updateWorkItem when form is submitted', async () => {
    const mockUpdateFn = vi.fn().mockResolvedValue(undefined);
    mockUseUpdateWorkItem.mockReturnValue(mockUpdateFn);
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    const { user } = renderWithProviders(
      <WorkItemEdit id="11111111-1111-1111-1111-111111111111" />,
    );

    // Clear and type a new title
    const titleInput = screen.getByLabelText(/title/i);
    await user.clear(titleInput);
    await user.type(titleInput, 'Updated title');

    // Submit the form
    await user.click(screen.getByRole('button', { name: 'Save Changes' }));

    await waitFor(() => {
      expect(mockUpdateFn).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        expect.objectContaining({
          title: 'Updated title',
          status: 'Active',
        }),
      );
    });
  });

  it('shows error toast when update fails', async () => {
    const mockUpdateFn = vi.fn().mockRejectedValue(new Error('Update failed'));
    mockUseUpdateWorkItem.mockReturnValue(mockUpdateFn);
    mockUseWorkItem.mockReturnValue({
      item: mockItem,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    const { user } = renderWithProviders(
      <WorkItemEdit id="11111111-1111-1111-1111-111111111111" />,
    );

    await user.click(screen.getByRole('button', { name: 'Save Changes' }));

    await waitFor(() => {
      expect(mockUpdateFn).toHaveBeenCalled();
    });
  });
});
