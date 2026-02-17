// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { FeatureGate } from '@/components/common/FeatureGate';
import { renderWithProviders, screen } from '@test/utils';

vi.mock('@/hooks/feature-flags', () => ({
  useFeatureFlag: vi.fn(),
}));

import { useFeatureFlag } from '@/hooks/feature-flags';

const mockUseFeatureFlag = vi.mocked(useFeatureFlag);

beforeEach(() => {
  vi.resetAllMocks();
});

describe('FeatureGate', () => {
  it('renders children when flag is enabled', () => {
    mockUseFeatureFlag.mockReturnValue({
      isEnabled: true,
      isLoading: false,
      error: null,
    });

    renderWithProviders(
      <FeatureGate flag="DarkMode">
        <span>Dark mode content</span>
      </FeatureGate>,
    );

    expect(screen.getByText('Dark mode content')).toBeInTheDocument();
  });

  it('does not render children when flag is disabled', () => {
    mockUseFeatureFlag.mockReturnValue({
      isEnabled: false,
      isLoading: false,
      error: null,
    });

    renderWithProviders(
      <FeatureGate flag="DarkMode">
        <span>Dark mode content</span>
      </FeatureGate>,
    );

    expect(screen.queryByText('Dark mode content')).not.toBeInTheDocument();
  });

  it('renders fallback when flag is disabled', () => {
    mockUseFeatureFlag.mockReturnValue({
      isEnabled: false,
      isLoading: false,
      error: null,
    });

    renderWithProviders(
      <FeatureGate flag="DarkMode" fallback={<span>Coming soon</span>}>
        <span>Dark mode content</span>
      </FeatureGate>,
    );

    expect(screen.queryByText('Dark mode content')).not.toBeInTheDocument();
    expect(screen.getByText('Coming soon')).toBeInTheDocument();
  });

  it('does not render children while loading', () => {
    mockUseFeatureFlag.mockReturnValue({
      isEnabled: false,
      isLoading: true,
      error: null,
    });

    renderWithProviders(
      <FeatureGate flag="DarkMode">
        <span>Dark mode content</span>
      </FeatureGate>,
    );

    expect(screen.queryByText('Dark mode content')).not.toBeInTheDocument();
  });

  it('passes the flag name to useFeatureFlag', () => {
    mockUseFeatureFlag.mockReturnValue({
      isEnabled: false,
      isLoading: false,
      error: null,
    });

    renderWithProviders(
      <FeatureGate flag="BetaFeatures">
        <span>Beta</span>
      </FeatureGate>,
    );

    expect(mockUseFeatureFlag).toHaveBeenCalledWith('BetaFeatures');
  });
});
