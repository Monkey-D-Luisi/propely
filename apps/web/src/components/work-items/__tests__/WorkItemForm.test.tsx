// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen, waitFor } from '@test/utils';
import { WorkItemForm } from '@/components/work-items/WorkItemForm';

beforeEach(() => {
  vi.resetAllMocks();
});

describe('WorkItemForm', () => {
  const defaultProps = {
    onSubmit: vi.fn().mockResolvedValue(undefined),
    onCancel: vi.fn(),
    submitLabel: 'Create Work Item',
  };

  it('renders form with all fields (title, description, status, priority, type, dueDate, effort)', () => {
    renderWithProviders(<WorkItemForm {...defaultProps} />);

    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/status/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/priority/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^type$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/due date/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/estimated effort/i)).toBeInTheDocument();
  });

  it('renders submit and cancel buttons', () => {
    renderWithProviders(<WorkItemForm {...defaultProps} />);

    expect(screen.getByRole('button', { name: 'Create Work Item' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  it('renders status select with all options', () => {
    renderWithProviders(<WorkItemForm {...defaultProps} />);

    const select = screen.getByLabelText(/status/i);
    expect(select).toBeInTheDocument();
    expect(screen.getByText('Pending')).toBeInTheDocument();
    expect(screen.getByText('Active')).toBeInTheDocument();
    expect(screen.getByText('Deactivated')).toBeInTheDocument();
    expect(screen.getByText('Expired')).toBeInTheDocument();
  });

  it('shows validation error when title is empty on submit', async () => {
    const onSubmit = vi.fn();
    const { user } = renderWithProviders(
      <WorkItemForm {...defaultProps} onSubmit={onSubmit} />,
    );

    const submitButton = screen.getByRole('button', { name: 'Create Work Item' });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Title is required')).toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('calls onSubmit with form data when valid', async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    const { user } = renderWithProviders(
      <WorkItemForm {...defaultProps} onSubmit={onSubmit} />,
    );

    await user.type(screen.getByLabelText(/title/i), 'My new work item');
    await user.type(screen.getByLabelText(/description/i), 'A description');

    const submitButton = screen.getByRole('button', { name: 'Create Work Item' });
    await user.click(submitButton);

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          title: 'My new work item',
          description: 'A description',
          status: 'Pending',
          priority: 'Medium',
          type: 'Task',
          dueDate: '',
          estimatedEffort: 'M',
        }),
        expect.anything(),
      );
    });
  });

  it('calls onCancel when cancel button is clicked', async () => {
    const onCancel = vi.fn();
    const { user } = renderWithProviders(
      <WorkItemForm {...defaultProps} onCancel={onCancel} />,
    );

    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(onCancel).toHaveBeenCalled();
  });

  it('pre-fills form with defaultValues', () => {
    renderWithProviders(
      <WorkItemForm
        {...defaultProps}
        defaultValues={{
          title: 'Existing title',
          description: 'Existing description',
          status: 'Active',
          priority: 'High',
          type: 'Bug',
          dueDate: '2026-03-01',
          estimatedEffort: 'L',
        }}
      />,
    );

    expect(screen.getByLabelText(/title/i)).toHaveValue('Existing title');
    expect(screen.getByLabelText(/description/i)).toHaveValue('Existing description');
    expect(screen.getByLabelText(/status/i)).toHaveValue('Active');
    expect(screen.getByLabelText(/priority/i)).toHaveValue('High');
    expect(screen.getByLabelText(/^type$/i)).toHaveValue('Bug');
    expect(screen.getByLabelText(/due date/i)).toHaveValue('2026-03-01');
    expect(screen.getByLabelText(/estimated effort/i)).toHaveValue('L');
  });

  it('shows loading indicator when isSubmitting is true', () => {
    renderWithProviders(
      <WorkItemForm {...defaultProps} isSubmitting={true} />,
    );

    const submitButton = screen.getByRole('button', { name: '...' });
    expect(submitButton).toBeDisabled();
  });

  it('defaults status to Pending and new fields to defaults when no defaultValues', () => {
    renderWithProviders(<WorkItemForm {...defaultProps} />);

    expect(screen.getByLabelText(/status/i)).toHaveValue('Pending');
    expect(screen.getByLabelText(/priority/i)).toHaveValue('Medium');
    expect(screen.getByLabelText(/^type$/i)).toHaveValue('Task');
    expect(screen.getByLabelText(/due date/i)).toHaveValue('');
    expect(screen.getByLabelText(/estimated effort/i)).toHaveValue('M');
  });
});
