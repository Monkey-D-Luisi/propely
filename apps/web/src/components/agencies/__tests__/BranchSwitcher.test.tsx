// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BranchSwitcher } from '@/components/agencies/BranchSwitcher';
import { renderWithProviders, screen, waitFor } from '@test/utils';

vi.mock('@/hooks/agencies', () => ({
  useAgencies: vi.fn(),
}));
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, onClick, ...rest }: { children: React.ReactNode; href: string; onClick?: () => void; [k: string]: unknown }) => (
    <a href={href} onClick={onClick} {...rest}>{children}</a>
  ),
}));

import { useAgencies } from '@/hooks/agencies';

const mockUseAgencies = vi.mocked(useAgencies);

const mockAgencies = [
  {
    id: '00000000-0000-0000-0000-000000000001',
    name: 'Agency One',
    slug: 'agency-one',
    createdAtUtc: '2026-01-01T00:00:00Z',
    branchCount: 3,
  },
  {
    id: '00000000-0000-0000-0000-000000000002',
    name: 'Agency Two',
    slug: 'agency-two',
    createdAtUtc: '2026-02-01T00:00:00Z',
    branchCount: 1,
  },
];

beforeEach(() => {
  vi.resetAllMocks();
  mockUseAgencies.mockReturnValue({
    agencies: mockAgencies,
    isLoading: false,
    error: null,
    refetch: vi.fn(),
  });
});

describe('BranchSwitcher', () => {
  it('renders nothing while loading', () => {
    mockUseAgencies.mockReturnValue({
      agencies: [],
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<BranchSwitcher />);
    expect(screen.queryByRole('button', { name: 'Switch agency' })).not.toBeInTheDocument();
  });

  it('renders nothing when no agencies', () => {
    mockUseAgencies.mockReturnValue({
      agencies: [],
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<BranchSwitcher />);
    expect(screen.queryByRole('button', { name: 'Switch agency' })).not.toBeInTheDocument();
  });

  it('renders agencies button when agencies exist', () => {
    renderWithProviders(<BranchSwitcher />);

    expect(screen.getByRole('button', { name: 'Switch agency' })).toBeInTheDocument();
  });

  it('shows dropdown with agencies when clicked', async () => {
    const { user } = renderWithProviders(<BranchSwitcher />);

    await user.click(screen.getByRole('button', { name: 'Switch agency' }));

    await waitFor(() => {
      expect(screen.getByText('Agency One')).toBeInTheDocument();
      expect(screen.getByText('Agency Two')).toBeInTheDocument();
    });
  });

  it('shows create agency link in dropdown', async () => {
    const { user } = renderWithProviders(<BranchSwitcher />);

    await user.click(screen.getByRole('button', { name: 'Switch agency' }));

    await waitFor(() => {
      expect(screen.getByText('Create agency')).toBeInTheDocument();
    });
  });

  it('shows branch counts in dropdown', async () => {
    const { user } = renderWithProviders(<BranchSwitcher />);

    await user.click(screen.getByRole('button', { name: 'Switch agency' }));

    await waitFor(() => {
      expect(screen.getByText('3 branches')).toBeInTheDocument();
      expect(screen.getByText('1 branch')).toBeInTheDocument();
    });
  });

  it('closes dropdown on Escape key', async () => {
    const { user } = renderWithProviders(<BranchSwitcher />);

    await user.click(screen.getByRole('button', { name: 'Switch agency' }));
    expect(screen.getByText('Agency One')).toBeInTheDocument();

    await user.keyboard('{Escape}');

    await waitFor(() => {
      expect(screen.queryByText('Agency One')).not.toBeInTheDocument();
    });
  });
});
