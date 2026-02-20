// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { FeatureFlagTable } from '@/components/admin/FeatureFlagTable';
import type { FeatureFlag } from '@/lib/schemas';

const mockRefetch = vi.fn();
const mockToggle = vi.fn();

vi.mock('@/hooks/feature-flags', () => ({
  useFeatureFlags: vi.fn(),
  useToggleFeatureFlag: vi.fn(() => mockToggle),
}));

import { useFeatureFlags } from '@/hooks/feature-flags';

const mockUseFeatureFlags = vi.mocked(useFeatureFlags);

const mockFlags: FeatureFlag[] = [
  { name: 'DarkMode', isEnabled: true, description: 'Enable dark mode', source: 'Database' },
  { name: 'BetaFeatures', isEnabled: false, description: null, source: 'Configuration' },
];

beforeEach(() => {
  vi.resetAllMocks();
  mockRefetch.mockResolvedValue(undefined);
  mockToggle.mockResolvedValue(undefined);
});

describe('FeatureFlagTable', () => {
  it('shows loading skeleton while flags are loading', () => {
    mockUseFeatureFlags.mockReturnValue({
      flags: [],
      isLoading: true,
      error: null,
      refetch: mockRefetch,
    });

    renderWithProviders(<FeatureFlagTable />);

    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  it('shows error state when loading fails', () => {
    mockUseFeatureFlags.mockReturnValue({
      flags: [],
      isLoading: false,
      error: new Error('Network error'),
      refetch: mockRefetch,
    });

    renderWithProviders(<FeatureFlagTable />);

    expect(screen.getByText('Could not load feature flags.')).toBeInTheDocument();
    expect(screen.getByText('Try again')).toBeInTheDocument();
  });

  it('shows empty state when no flags exist', () => {
    mockUseFeatureFlags.mockReturnValue({
      flags: [],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderWithProviders(<FeatureFlagTable />);

    expect(screen.getByText('No feature flags configured.')).toBeInTheDocument();
  });

  it('renders flag table with all flags', () => {
    mockUseFeatureFlags.mockReturnValue({
      flags: mockFlags,
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderWithProviders(<FeatureFlagTable />);

    expect(screen.getByText('DarkMode')).toBeInTheDocument();
    expect(screen.getAllByText('Enable dark mode').length).toBeGreaterThan(0);
    expect(screen.getByText('BetaFeatures')).toBeInTheDocument();
    expect(screen.getByText('Database')).toBeInTheDocument();
    expect(screen.getByText('Configuration')).toBeInTheDocument();
  });

  it('renders toggle switches with correct initial state', () => {
    mockUseFeatureFlags.mockReturnValue({
      flags: mockFlags,
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderWithProviders(<FeatureFlagTable />);

    const switches = screen.getAllByRole('switch');
    expect(switches).toHaveLength(2);
    expect(switches[0]).toHaveAttribute('aria-checked', 'true');
    expect(switches[1]).toHaveAttribute('aria-checked', 'false');
  });

  it('calls toggle and refetch when switch is clicked', async () => {
    mockUseFeatureFlags.mockReturnValue({
      flags: mockFlags,
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    const { user } = renderWithProviders(<FeatureFlagTable />);

    const switches = screen.getAllByRole('switch');
    await user.click(switches[0]);

    await waitFor(() => {
      expect(mockToggle).toHaveBeenCalledWith('DarkMode', false);
    });

    await waitFor(() => {
      expect(mockRefetch).toHaveBeenCalled();
    });
  });

  it('shows no description text for flags without description', () => {
    mockUseFeatureFlags.mockReturnValue({
      flags: [{ name: 'Test', isEnabled: true, description: null, source: 'Configuration' }],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });

    renderWithProviders(<FeatureFlagTable />);

    expect(screen.getAllByText('No description').length).toBeGreaterThan(0);
  });

  it('calls refetch when try again button is clicked in error state', async () => {
    mockUseFeatureFlags.mockReturnValue({
      flags: [],
      isLoading: false,
      error: new Error('fail'),
      refetch: mockRefetch,
    });

    const { user } = renderWithProviders(<FeatureFlagTable />);

    await user.click(screen.getByText('Try again'));

    expect(mockRefetch).toHaveBeenCalled();
  });
});
