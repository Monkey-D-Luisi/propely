// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { renderWithProviders, screen } from '@test/utils';
import { PropertyTable } from '../PropertyTable';
import type { PropertyListItem } from '@/lib/schemas';

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [key: string]: unknown }) => <a href={href} {...rest}>{children}</a>,
}));

const mockItems: PropertyListItem[] = [
  {
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
  },
  {
    id: '00000000-0000-0000-0000-000000000002',
    title: 'Villa with Pool in Malaga',
    propertyType: 'Villa',
    operationType: 'Rent',
    status: 'Draft',
    price: 2500,
    city: 'Malaga',
    builtArea: 250,
    bedrooms: 5,
    bathrooms: 3,
    agentId: '00000000-0000-0000-0000-000000000099',
    createdAtUtc: '2026-02-01T08:00:00Z',
    updatedAtUtc: null,
  },
];

describe('PropertyTable', () => {
  const defaultProps = {
    items: mockItems,
    isLoading: false,
    onDelete: vi.fn(),
    onChangeStatus: vi.fn(),
  };

  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('renders table with correct headers', () => {
    renderWithProviders(<PropertyTable {...defaultProps} />);
    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getByText('Property')).toBeInTheDocument();
    expect(screen.getByText('Type')).toBeInTheDocument();
    expect(screen.getByText('Price')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
  });

  it('renders property rows with correct data', () => {
    renderWithProviders(<PropertyTable {...defaultProps} />);
    expect(screen.getByText('Modern Apartment in Madrid')).toBeInTheDocument();
    expect(screen.getByText('Villa with Pool in Malaga')).toBeInTheDocument();
    expect(screen.getByText('Madrid')).toBeInTheDocument();
    expect(screen.getByText('Malaga')).toBeInTheDocument();
  });

  it('renders status badges for each item', () => {
    renderWithProviders(<PropertyTable {...defaultProps} />);
    expect(screen.getByTestId('status-badge-Active')).toBeInTheDocument();
    expect(screen.getByTestId('status-badge-Draft')).toBeInTheDocument();
  });

  it('links property titles to detail page', () => {
    renderWithProviders(<PropertyTable {...defaultProps} />);
    const links = screen.getAllByRole('link');
    const detailLink = links.find(l => l.getAttribute('href')?.includes('/properties/00000000-0000-0000-0000-000000000001'));
    expect(detailLink).toBeInTheDocument();
  });

  it('renders skeleton when loading', () => {
    renderWithProviders(<PropertyTable {...defaultProps} isLoading={true} />);
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });
});
