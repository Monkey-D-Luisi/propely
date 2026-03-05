// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { renderWithProviders, screen } from '@test/utils';
import { AdvancedFilterPanel, type AdvancedFilters } from '../AdvancedFilterPanel';

const emptyFilters: AdvancedFilters = {
  types: [],
  operations: [],
  statuses: [],
};

describe('AdvancedFilterPanel', () => {
  const defaultProps = {
    filters: emptyFilters,
    onFiltersChange: vi.fn(),
    onClear: vi.fn(),
  };

  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('renders toggle button', () => {
    renderWithProviders(<AdvancedFilterPanel {...defaultProps} />);
    expect(screen.getByText('Advanced Filters')).toBeInTheDocument();
  });

  it('opens panel on toggle click', async () => {
    const { user } = renderWithProviders(<AdvancedFilterPanel {...defaultProps} />);
    await user.click(screen.getByText('Advanced Filters'));
    expect(screen.getByText('Price range')).toBeInTheDocument();
    expect(screen.getByText(/Area range/)).toBeInTheDocument();
  });

  it('shows active filter count badge', () => {
    const filtersWithActive: AdvancedFilters = {
      ...emptyFilters,
      types: ['Apartment'],
      minPrice: 100000,
    };
    renderWithProviders(<AdvancedFilterPanel {...defaultProps} filters={filtersWithActive} />);
    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('calls onClear when clear button is clicked', async () => {
    const filtersWithActive: AdvancedFilters = {
      ...emptyFilters,
      types: ['Apartment'],
    };
    const { user } = renderWithProviders(
      <AdvancedFilterPanel {...defaultProps} filters={filtersWithActive} />
    );
    await user.click(screen.getByText('Advanced Filters'));
    await user.click(screen.getByText('Clear all filters'));
    expect(defaultProps.onClear).toHaveBeenCalled();
  });

  it('toggles multi-select on type click', async () => {
    const { user } = renderWithProviders(<AdvancedFilterPanel {...defaultProps} />);
    await user.click(screen.getByText('Advanced Filters'));
    await user.click(screen.getByText('Apartment'));
    expect(defaultProps.onFiltersChange).toHaveBeenCalledWith(
      expect.objectContaining({ types: ['Apartment'] })
    );
  });
});
