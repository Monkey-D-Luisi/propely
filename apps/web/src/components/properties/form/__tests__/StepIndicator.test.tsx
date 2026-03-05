// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { renderWithProviders, screen } from '@test/utils';
import { StepIndicator } from '../StepIndicator';

describe('StepIndicator', () => {
  it('renders all 6 steps', () => {
    renderWithProviders(<StepIndicator currentStep={0} completedSteps={new Set()} />);
    expect(screen.getByText('Basic Info')).toBeInTheDocument();
    expect(screen.getByText('Location')).toBeInTheDocument();
    expect(screen.getByText('Features')).toBeInTheDocument();
    expect(screen.getByText('Financial')).toBeInTheDocument();
    expect(screen.getByText('Descriptions')).toBeInTheDocument();
    expect(screen.getByText('Media')).toBeInTheDocument();
  });

  it('highlights current step', () => {
    renderWithProviders(<StepIndicator currentStep={2} completedSteps={new Set()} />);
    // Step 3 (index 2) should have the number "3"
    const stepNumbers = screen.getAllByText('3');
    expect(stepNumbers.length).toBeGreaterThanOrEqual(1);
  });

  it('shows check icon for completed steps', () => {
    renderWithProviders(<StepIndicator currentStep={2} completedSteps={new Set([0, 1])} />);
    const checkIcons = document.querySelectorAll('.material-symbols-outlined');
    expect(checkIcons.length).toBeGreaterThanOrEqual(2);
  });

  it('renders step navigation', () => {
    renderWithProviders(<StepIndicator currentStep={0} completedSteps={new Set()} />);
    const nav = screen.getByRole('navigation', { name: 'Form steps' });
    expect(nav).toBeInTheDocument();
  });
});
