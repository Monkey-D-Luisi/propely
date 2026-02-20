// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { AuditLogFilters } from '@/components/admin/AuditLogFilters';
import type { AuditLogFilters as FilterState } from '@/hooks/audit-logs';

const mockOnFiltersChange = vi.fn();

beforeEach(() => {
  vi.resetAllMocks();
});

describe('AuditLogFilters', () => {
  it('renders all six filter inputs', () => {
    renderWithProviders(
      <AuditLogFilters filters={{}} onFiltersChange={mockOnFiltersChange} />,
    );

    expect(screen.getByLabelText('From date')).toBeInTheDocument();
    expect(screen.getByLabelText('To date')).toBeInTheDocument();
    expect(screen.getByLabelText('Action')).toBeInTheDocument();
    expect(screen.getByLabelText('Entity type')).toBeInTheDocument();
    expect(screen.getByLabelText('Entity ID')).toBeInTheDocument();
    expect(screen.getByLabelText('User ID')).toBeInTheDocument();
  });

  it('does not show clear button when no filters are active', () => {
    renderWithProviders(
      <AuditLogFilters filters={{}} onFiltersChange={mockOnFiltersChange} />,
    );

    expect(screen.queryByText('Clear filters')).not.toBeInTheDocument();
  });

  it('shows clear button when filters are active', () => {
    const filters: FilterState = { action: 'Added' };
    renderWithProviders(
      <AuditLogFilters filters={filters} onFiltersChange={mockOnFiltersChange} />,
    );

    expect(screen.getByText('Clear filters')).toBeInTheDocument();
  });

  it('calls onFiltersChange with empty object when clear is clicked', async () => {
    const filters: FilterState = { action: 'Added' };
    const { user } = renderWithProviders(
      <AuditLogFilters filters={filters} onFiltersChange={mockOnFiltersChange} />,
    );

    await user.click(screen.getByText('Clear filters'));

    expect(mockOnFiltersChange).toHaveBeenCalledWith({});
  });

  it('calls onFiltersChange when action input changes', async () => {
    const { user } = renderWithProviders(
      <AuditLogFilters filters={{}} onFiltersChange={mockOnFiltersChange} />,
    );

    const actionInput = screen.getByLabelText('Action');
    await user.type(actionInput, 'A');

    expect(mockOnFiltersChange).toHaveBeenCalledWith({ action: 'A' });
  });

  it('displays current filter values in inputs', () => {
    const filters: FilterState = {
      action: 'Modified',
      entityType: 'Organization',
    };
    renderWithProviders(
      <AuditLogFilters filters={filters} onFiltersChange={mockOnFiltersChange} />,
    );

    expect(screen.getByLabelText('Action')).toHaveValue('Modified');
    expect(screen.getByLabelText('Entity type')).toHaveValue('Organization');
  });

  it('renders placeholders for text inputs', () => {
    renderWithProviders(
      <AuditLogFilters filters={{}} onFiltersChange={mockOnFiltersChange} />,
    );

    expect(screen.getByPlaceholderText('e.g. Added')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('e.g. Organization')).toBeInTheDocument();
  });
});
