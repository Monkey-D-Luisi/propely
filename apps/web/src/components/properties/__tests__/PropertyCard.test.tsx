// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { renderWithProviders, screen } from '@test/utils';
import { PropertyCard } from '../PropertyCard';
import type { PropertyListItem } from '@/lib/schemas';

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [key: string]: unknown }) => <a href={href} {...rest}>{children}</a>,
}));

const mockItem: PropertyListItem = {
  id: '00000000-0000-0000-0000-000000000001',
  title: 'Modern Apartment in Madrid',
  propertyType: 'Apartment',
  operationType: 'Sale',
  status: 'Active',
  price: 350000,
  city: 'Madrid',
  builtArea: 95,
  bedrooms: 3,
  bathrooms: 2,
  agentId: '00000000-0000-0000-0000-000000000099',
  createdAtUtc: '2026-01-15T10:00:00Z',
  updatedAtUtc: '2026-02-20T14:30:00Z',
};

describe('PropertyCard', () => {
  it('renders property title', () => {
    renderWithProviders(<PropertyCard item={mockItem} />);
    expect(screen.getByText('Modern Apartment in Madrid')).toBeInTheDocument();
  });

  it('renders city', () => {
    renderWithProviders(<PropertyCard item={mockItem} />);
    expect(screen.getByText('Madrid')).toBeInTheDocument();
  });

  it('renders price formatted', () => {
    renderWithProviders(<PropertyCard item={mockItem} />);
    // Price formatting may vary by locale in test, check it exists
    const priceEl = screen.getByText(/350/);
    expect(priceEl).toBeInTheDocument();
  });

  it('renders status badge', () => {
    renderWithProviders(<PropertyCard item={mockItem} />);
    expect(screen.getByTestId('status-badge-Active')).toBeInTheDocument();
  });

  it('renders bedrooms, bathrooms, and area', () => {
    renderWithProviders(<PropertyCard item={mockItem} />);
    expect(screen.getByText('3')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText(/95/)).toBeInTheDocument();
  });

  it('links to property detail page', () => {
    renderWithProviders(<PropertyCard item={mockItem} />);
    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', expect.stringContaining('/properties/00000000-0000-0000-0000-000000000001'));
  });

  it('renders without optional fields', () => {
    const minimal: PropertyListItem = {
      ...mockItem,
      city: null,
      builtArea: null,
      bedrooms: null,
      bathrooms: null,
      price: null,
    };
    renderWithProviders(<PropertyCard item={minimal} />);
    expect(screen.getByText('Modern Apartment in Madrid')).toBeInTheDocument();
  });
});
