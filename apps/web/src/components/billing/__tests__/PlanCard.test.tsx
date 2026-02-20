// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PlanCard } from '@/components/billing/PlanCard';
import { renderWithProviders, screen } from '@test/utils';

beforeEach(() => {
  vi.resetAllMocks();
});

const basePlan = {
  id: 'pro',
  name: 'Pro',
  features: ['Feature A', 'Feature B'],
};

describe('PlanCard', () => {
  it('renders plan name and features', () => {
    renderWithProviders(<PlanCard plan={basePlan} />);

    expect(screen.getByText('Pro')).toBeInTheDocument();
    expect(screen.getByText('Feature A')).toBeInTheDocument();
    expect(screen.getByText('Feature B')).toBeInTheDocument();
  });

  it('renders recommended badge when isRecommended', () => {
    renderWithProviders(<PlanCard plan={basePlan} isRecommended />);

    expect(screen.getByText(/recommended/i)).toBeInTheDocument();
  });

  it('renders current plan label when isCurrent', () => {
    renderWithProviders(<PlanCard plan={basePlan} isCurrent />);

    expect(screen.getByText(/current plan/i)).toBeInTheDocument();
  });

  it('renders price when provided', () => {
    renderWithProviders(<PlanCard plan={basePlan} price="$29" />);

    expect(screen.getByText('$29')).toBeInTheDocument();
  });

  it('calls onSelect when CTA button is clicked', async () => {
    const handleSelect = vi.fn();
    const { user } = renderWithProviders(
      <PlanCard plan={basePlan} onSelect={handleSelect} ctaLabel="Subscribe" />,
    );

    await user.click(screen.getByRole('button', { name: 'Subscribe' }));

    expect(handleSelect).toHaveBeenCalledTimes(1);
  });

  it('renders disabled features with reduced opacity', () => {
    renderWithProviders(
      <PlanCard plan={basePlan} disabledFeatures={['No custom domains']} />,
    );

    expect(screen.getByText('No custom domains')).toBeInTheDocument();
  });
});
