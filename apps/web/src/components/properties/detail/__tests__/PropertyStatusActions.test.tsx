// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { PropertyStatusActions } from '../PropertyStatusActions';
import type { Property } from '@/lib/schemas';

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [key: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
  useRouter: () => ({ push: vi.fn() }),
}));

function makeProperty(overrides: Partial<Property> = {}): Property {
  return {
    id: '00000000-0000-0000-0000-000000000001',
    title: 'Test Property',
    propertyType: 'Apartment',
    operationType: 'Sale',
    status: 'Draft',
    tenantId: '00000000-0000-0000-0000-000000000099',
    agentId: '00000000-0000-0000-0000-000000000099',
    createdAtUtc: '2026-01-15T10:00:00Z',
    ...overrides,
  };
}

const testCases = [
  { status: 'Draft' as const, expectedButtons: ['Activate', 'Archive'] },
  { status: 'Active' as const, expectedButtons: ['Reserve', 'Mark as Sold', 'Mark as Rented', 'Archive'] },
  { status: 'Reserved' as const, expectedButtons: ['Release reservation', 'Mark as Sold', 'Mark as Rented', 'Archive'] },
  { status: 'Sold' as const, expectedButtons: [] },
  { status: 'Rented' as const, expectedButtons: [] },
  { status: 'Archived' as const, expectedButtons: ['Activate'] },
];

describe('PropertyStatusActions', () => {
  let onStatusChange: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    onStatusChange = vi.fn();
  });

  it.each(testCases)(
    'renders correct buttons for $status status',
    ({ status, expectedButtons }) => {
      const property = makeProperty({ status });
      renderWithProviders(
        <PropertyStatusActions property={property} onStatusChange={onStatusChange} />,
      );

      if (expectedButtons.length === 0) {
        expect(screen.queryByTestId('status-actions')).not.toBeInTheDocument();
      } else {
        const container = screen.getByTestId('status-actions');
        expect(container).toBeInTheDocument();
        expectedButtons.forEach((label) => {
          expect(screen.getByRole('button', { name: label })).toBeInTheDocument();
        });
        // Verify no extra buttons
        const buttons = screen.getAllByRole('button');
        expect(buttons).toHaveLength(expectedButtons.length);
      }
    },
  );

  it('calls onStatusChange with correct target when button clicked', async () => {
    const property = makeProperty({ status: 'Draft' });
    const { user } = renderWithProviders(
      <PropertyStatusActions property={property} onStatusChange={onStatusChange} />,
    );
    await user.click(screen.getByRole('button', { name: 'Activate' }));
    expect(onStatusChange).toHaveBeenCalledWith('Active');
  });

  it('calls onStatusChange with Archive when Archive button clicked', async () => {
    const property = makeProperty({ status: 'Active' });
    const { user } = renderWithProviders(
      <PropertyStatusActions property={property} onStatusChange={onStatusChange} />,
    );
    await user.click(screen.getByRole('button', { name: 'Archive' }));
    expect(onStatusChange).toHaveBeenCalledWith('Archived');
  });

  it('shows "Release reservation" for Reserved -> Active transition', () => {
    const property = makeProperty({ status: 'Reserved' });
    renderWithProviders(
      <PropertyStatusActions property={property} onStatusChange={onStatusChange} />,
    );
    expect(screen.getByRole('button', { name: 'Release reservation' })).toBeInTheDocument();
  });
});
