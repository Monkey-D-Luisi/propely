// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { AppointmentForm } from '../AppointmentForm';

const baseProps = {
  onSubmit: vi.fn(),
  onCancel: vi.fn(),
  submitLabel: 'Create Appointment',
  isSubmitting: false,
};

describe('AppointmentForm', () => {
  it('renders all form fields', () => {
    renderWithProviders(<AppointmentForm {...baseProps} />);

    expect(screen.getByTestId('appointment-form')).toBeInTheDocument();

    // Text inputs
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/type/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/all day/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/location/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/property/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/contact/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/notes/i)).toBeInTheDocument();
  });

  it('renders start and end date fields', () => {
    renderWithProviders(<AppointmentForm {...baseProps} />);

    expect(screen.getByLabelText(/start date/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/end date/i)).toBeInTheDocument();
  });

  it('renders submit and cancel buttons', () => {
    renderWithProviders(<AppointmentForm {...baseProps} />);

    expect(screen.getByRole('button', { name: /create appointment/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
  });

  it('calls onCancel when cancel button is clicked', async () => {
    const { user } = renderWithProviders(<AppointmentForm {...baseProps} />);

    await user.click(screen.getByRole('button', { name: /cancel/i }));
    expect(baseProps.onCancel).toHaveBeenCalledTimes(1);
  });

  it('disables submit button when isSubmitting', () => {
    renderWithProviders(<AppointmentForm {...baseProps} isSubmitting={true} />);

    const submitBtn = screen.getByRole('button', { name: '...' });
    expect(submitBtn).toBeDisabled();
  });
});
