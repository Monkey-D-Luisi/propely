// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { renderWithProviders, screen } from '@test/utils';
import { ActiveFilterBadges } from '../ActiveFilterBadges';
import type { PropertyFilters } from '@/hooks/properties';

describe('ActiveFilterBadges', () => {
  const defaultProps = {
    filters: {} as PropertyFilters,
    onRemove: vi.fn(),
    onClearAll: vi.fn(),
  };

  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('renders nothing when no active filters', () => {
    const { container } = renderWithProviders(<ActiveFilterBadges {...defaultProps} />);
    expect(container.querySelector('[data-testid="active-filter-badges"]')).not.toBeInTheDocument();
  });

  it('renders type badge', () => {
    renderWithProviders(
      <ActiveFilterBadges {...defaultProps} filters={{ type: 'Apartment' }} />
    );
    expect(screen.getByText(/Apartment/)).toBeInTheDocument();
  });

  it('renders price range badge with both min and max', () => {
    renderWithProviders(
      <ActiveFilterBadges {...defaultProps} filters={{ minPrice: 100000, maxPrice: 300000 }} />
    );
    expect(screen.getByText(/100k.*300k/)).toBeInTheDocument();
  });

  it('renders bedrooms badge', () => {
    renderWithProviders(
      <ActiveFilterBadges {...defaultProps} filters={{ minBedrooms: 3 }} />
    );
    expect(screen.getByText(/3\+/)).toBeInTheDocument();
  });

  it('renders amenity badges', () => {
    renderWithProviders(
      <ActiveFilterBadges {...defaultProps} filters={{ hasPool: true, hasGarage: true }} />
    );
    expect(screen.getByText('Pool')).toBeInTheDocument();
    expect(screen.getByText('Garage')).toBeInTheDocument();
  });

  it('calls onRemove when X button clicked', async () => {
    const { user } = renderWithProviders(
      <ActiveFilterBadges {...defaultProps} filters={{ type: 'Apartment' }} />
    );
    const removeBtn = screen.getByLabelText(/Remove/);
    await user.click(removeBtn);
    expect(defaultProps.onRemove).toHaveBeenCalledWith('type');
  });

  it('calls onClearAll when clear all button clicked', async () => {
    const { user } = renderWithProviders(
      <ActiveFilterBadges {...defaultProps} filters={{ type: 'Apartment' }} />
    );
    await user.click(screen.getByText('Clear all filters'));
    expect(defaultProps.onClearAll).toHaveBeenCalled();
  });

  it('renders clear all button when badges present', () => {
    renderWithProviders(
      <ActiveFilterBadges {...defaultProps} filters={{ status: 'Active' }} />
    );
    expect(screen.getByText('Clear all filters')).toBeInTheDocument();
  });
});
