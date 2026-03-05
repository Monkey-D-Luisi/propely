// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { PropertyFormWizard } from '../PropertyFormWizard';

// Mock next/navigation
const mockPush = vi.fn();
vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
  usePathname: () => '/en/properties/new',
}));

// Mock i18n navigation
vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [key: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
}));

// Mock properties hooks
vi.mock('@/hooks/properties', () => ({
  useCreateProperty: () => vi.fn().mockResolvedValue({ id: 'test-id', status: 'Draft' }),
  useUpdateProperty: () => vi.fn().mockResolvedValue({ id: 'test-id', status: 'Active' }),
  useChangePropertyStatus: () => vi.fn().mockResolvedValue(undefined),
}));

beforeEach(() => {
  vi.resetAllMocks();
  localStorage.clear();
});

describe('PropertyFormWizard', () => {
  it('renders step indicator and navigation buttons', () => {
    renderWithProviders(<PropertyFormWizard />);

    // Step indicator with step labels
    expect(screen.getByRole('navigation', { name: 'Form steps' })).toBeInTheDocument();

    // Navigation buttons
    expect(screen.getByText('Previous')).toBeInTheDocument();
    expect(screen.getByText('Next')).toBeInTheDocument();
  });

  it('renders the first step (Basic Info) by default', () => {
    renderWithProviders(<PropertyFormWizard />);

    // BasicInfoStep renders a title input
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
  });

  it('disables Previous button on first step', () => {
    renderWithProviders(<PropertyFormWizard />);

    const prevBtn = screen.getByText('Previous');
    expect(prevBtn).toBeDisabled();
  });

  it('navigates to next step when clicking Next', async () => {
    const { user } = renderWithProviders(<PropertyFormWizard />);

    const nextBtn = screen.getByText('Next');
    await user.click(nextBtn);

    // Step 2 is LocationStep: should have street input
    expect(screen.getByLabelText(/street/i)).toBeInTheDocument();
  });

  it('navigates back when clicking Previous', async () => {
    const { user } = renderWithProviders(<PropertyFormWizard />);

    // Go to step 2
    await user.click(screen.getByText('Next'));
    expect(screen.getByLabelText(/street/i)).toBeInTheDocument();

    // Go back to step 1
    await user.click(screen.getByText('Previous'));
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
  });

  it('shows Save as Draft button in create mode', () => {
    renderWithProviders(<PropertyFormWizard />);

    expect(screen.getByText('Save as Draft')).toBeInTheDocument();
  });

  it('does not show Save as Draft button in edit mode', () => {
    const existingProperty = {
      id: '00000000-0000-0000-0000-000000000001',
      title: 'Test Property',
      propertyType: 'Apartment' as const,
      operationType: 'Sale' as const,
      status: 'Active' as const,
      tenantId: '00000000-0000-0000-0000-000000000002',
      agentId: '00000000-0000-0000-0000-000000000003',
      createdAtUtc: '2026-01-15T10:00:00Z',
    };

    renderWithProviders(<PropertyFormWizard existingProperty={existingProperty} />);

    expect(screen.queryByText('Save as Draft')).not.toBeInTheDocument();
  });

  it('shows Publish button on last step in create mode', async () => {
    const { user } = renderWithProviders(<PropertyFormWizard />);

    // Navigate through all steps to reach the last one (step 6 = index 5)
    for (let i = 0; i < 5; i++) {
      await user.click(screen.getByText('Next'));
    }

    expect(screen.getByText('Publish')).toBeInTheDocument();
    expect(screen.queryByText('Next')).not.toBeInTheDocument();
  });

  it('shows Save Changes button on last step in edit mode', async () => {
    const existingProperty = {
      id: '00000000-0000-0000-0000-000000000001',
      title: 'Test Property',
      propertyType: 'Apartment' as const,
      operationType: 'Sale' as const,
      status: 'Active' as const,
      tenantId: '00000000-0000-0000-0000-000000000002',
      agentId: '00000000-0000-0000-0000-000000000003',
      createdAtUtc: '2026-01-15T10:00:00Z',
    };

    const { user } = renderWithProviders(<PropertyFormWizard existingProperty={existingProperty} />);

    // Navigate to last step
    for (let i = 0; i < 5; i++) {
      await user.click(screen.getByText('Next'));
    }

    expect(screen.getByText('Save Changes')).toBeInTheDocument();
  });

  it('renders all 6 steps sequentially', async () => {
    const { user } = renderWithProviders(<PropertyFormWizard />);

    // Step 1: Basic Info - has title input
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();

    // Step 2: Location - has street input
    await user.click(screen.getByText('Next'));
    expect(screen.getByLabelText(/street/i)).toBeInTheDocument();

    // Step 3: Features - has bedrooms input
    await user.click(screen.getByText('Next'));
    expect(screen.getByLabelText(/bedrooms/i)).toBeInTheDocument();

    // Step 4: Financial - has price input
    await user.click(screen.getByText('Next'));
    expect(screen.getByLabelText(/price/i)).toBeInTheDocument();

    // Step 5: Descriptions - has language tab ES
    await user.click(screen.getByText('Next'));
    expect(screen.getByText('ES')).toBeInTheDocument();

    // Step 6: Media - has upload photos button
    await user.click(screen.getByText('Next'));
    expect(screen.getByText('Upload Photos')).toBeInTheDocument();
  });
});
