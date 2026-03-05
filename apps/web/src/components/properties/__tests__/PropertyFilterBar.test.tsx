// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { renderWithProviders, screen } from '@test/utils';
import { PropertyFilterBar } from '../PropertyFilterBar';

describe('PropertyFilterBar', () => {
  const defaultProps = {
    search: '',
    onSearchChange: vi.fn(),
    type: '',
    onTypeChange: vi.fn(),
    operation: '',
    onOperationChange: vi.fn(),
    status: '',
    onStatusChange: vi.fn(),
    sortBy: '',
    onSortByChange: vi.fn(),
  };

  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('renders search input', () => {
    renderWithProviders(<PropertyFilterBar {...defaultProps} />);
    expect(screen.getByPlaceholderText('Search properties...')).toBeInTheDocument();
  });

  it('renders type dropdown with all options', () => {
    renderWithProviders(<PropertyFilterBar {...defaultProps} />);
    const typeSelect = screen.getByLabelText('All types');
    expect(typeSelect).toBeInTheDocument();
    expect(typeSelect.querySelectorAll('option').length).toBeGreaterThan(1);
  });

  it('renders operation dropdown', () => {
    renderWithProviders(<PropertyFilterBar {...defaultProps} />);
    expect(screen.getByLabelText('All operations')).toBeInTheDocument();
  });

  it('renders status dropdown', () => {
    renderWithProviders(<PropertyFilterBar {...defaultProps} />);
    expect(screen.getByLabelText('All statuses')).toBeInTheDocument();
  });

  it('renders sort dropdown', () => {
    renderWithProviders(<PropertyFilterBar {...defaultProps} />);
    expect(screen.getByLabelText('Sort by')).toBeInTheDocument();
  });

  it('calls onSearchChange when search input changes', async () => {
    const { user } = renderWithProviders(<PropertyFilterBar {...defaultProps} />);
    const input = screen.getByPlaceholderText('Search properties...');
    await user.type(input, 'a');
    expect(defaultProps.onSearchChange).toHaveBeenCalled();
  });
});
