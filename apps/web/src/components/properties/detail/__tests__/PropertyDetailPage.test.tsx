// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { PropertyDetailPage } from '../PropertyDetailPage';
import type { Property, PropertyMedia } from '@/lib/schemas';

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href, ...rest }: { children: React.ReactNode; href: string; [key: string]: unknown }) => (
    <a href={href} {...rest}>{children}</a>
  ),
  useRouter: () => ({ push: vi.fn() }),
}));

const mockProperty: Property = {
  id: '00000000-0000-0000-0000-000000000001',
  title: 'Modern Apartment in Madrid',
  propertyType: 'Apartment',
  operationType: 'Sale',
  status: 'Active',
  tenantId: '00000000-0000-0000-0000-000000000099',
  agentId: '00000000-0000-0000-0000-000000000099',
  createdAtUtc: '2026-01-15T10:00:00Z',
  address: {
    street: 'Calle Gran Via 42',
    city: 'Madrid',
    province: 'Madrid',
    postalCode: '28013',
    country: 'Spain',
  },
  features: {
    bedrooms: 3,
    bathrooms: 2,
    builtArea: 120,
  },
  financials: {
    price: 450000,
  },
  description: {
    es: 'Apartamento moderno',
    en: 'Modern apartment',
  },
};

const mockMedia: PropertyMedia[] = [
  {
    id: '00000000-0000-0000-0000-000000000010',
    propertyId: '00000000-0000-0000-0000-000000000001',
    mediaType: 'Photo',
    storagePath: '/photos/1.jpg',
    fileName: 'living-room.jpg',
    contentType: 'image/jpeg',
    sizeBytes: 500000,
    displayOrder: 0,
    uploadedAtUtc: '2026-01-15T10:00:00Z',
    url: 'https://example.com/photos/1.jpg',
    thumbnailUrl: 'https://example.com/photos/1_thumb.jpg',
  },
];

const mockUseProperty = vi.fn();
const mockUsePropertyMedia = vi.fn();
const mockUseChangePropertyStatus = vi.fn();

vi.mock('@/hooks/properties', () => ({
  useProperty: (...args: unknown[]) => mockUseProperty(...args),
  usePropertyMedia: (...args: unknown[]) => mockUsePropertyMedia(...args),
  useChangePropertyStatus: () => mockUseChangePropertyStatus(),
}));

describe('PropertyDetailPage', () => {
  beforeEach(() => {
    vi.resetAllMocks();
    mockUseChangePropertyStatus.mockReturnValue(vi.fn());
  });

  it('renders loading skeleton while data is being fetched', () => {
    mockUseProperty.mockReturnValue({
      property: null,
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    });
    mockUsePropertyMedia.mockReturnValue({
      media: [],
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PropertyDetailPage id="00000000-0000-0000-0000-000000000001" />);
    expect(screen.getByTestId('detail-loading')).toBeInTheDocument();
  });

  it('renders error state when fetch fails', () => {
    mockUseProperty.mockReturnValue({
      property: null,
      isLoading: false,
      error: new Error('Network error'),
      refetch: vi.fn(),
    });
    mockUsePropertyMedia.mockReturnValue({
      media: [],
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PropertyDetailPage id="00000000-0000-0000-0000-000000000001" />);
    expect(screen.getByTestId('detail-error')).toBeInTheDocument();
    expect(screen.getByText('Back to Properties')).toBeInTheDocument();
  });

  it('renders property detail with title and status badge', () => {
    mockUseProperty.mockReturnValue({
      property: mockProperty,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });
    mockUsePropertyMedia.mockReturnValue({
      media: mockMedia,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PropertyDetailPage id="00000000-0000-0000-0000-000000000001" />);
    expect(screen.getByTestId('property-detail')).toBeInTheDocument();
    expect(screen.getByTestId('property-title')).toHaveTextContent('Modern Apartment in Madrid');
    expect(screen.getByTestId('status-badge-Active')).toBeInTheDocument();
  });

  it('renders back link to properties list', () => {
    mockUseProperty.mockReturnValue({
      property: mockProperty,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });
    mockUsePropertyMedia.mockReturnValue({
      media: mockMedia,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PropertyDetailPage id="00000000-0000-0000-0000-000000000001" />);
    expect(screen.getByTestId('back-link')).toBeInTheDocument();
    expect(screen.getByTestId('back-link')).toHaveAttribute('href', '/properties');
  });

  it('renders edit link pointing to edit page', () => {
    mockUseProperty.mockReturnValue({
      property: mockProperty,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });
    mockUsePropertyMedia.mockReturnValue({
      media: mockMedia,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PropertyDetailPage id="00000000-0000-0000-0000-000000000001" />);
    const editLink = screen.getByTestId('edit-link');
    expect(editLink).toHaveAttribute('href', '/properties/00000000-0000-0000-0000-000000000001/edit');
  });

  it('renders status action buttons for active property', () => {
    mockUseProperty.mockReturnValue({
      property: mockProperty,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });
    mockUsePropertyMedia.mockReturnValue({
      media: mockMedia,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PropertyDetailPage id="00000000-0000-0000-0000-000000000001" />);
    expect(screen.getByTestId('status-actions')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Reserve' })).toBeInTheDocument();
  });

  it('renders photo gallery section', () => {
    mockUseProperty.mockReturnValue({
      property: mockProperty,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });
    mockUsePropertyMedia.mockReturnValue({
      media: mockMedia,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PropertyDetailPage id="00000000-0000-0000-0000-000000000001" />);
    expect(screen.getByTestId('hero-photo')).toBeInTheDocument();
  });

  it('renders info sections', () => {
    mockUseProperty.mockReturnValue({
      property: mockProperty,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });
    mockUsePropertyMedia.mockReturnValue({
      media: mockMedia,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    });

    renderWithProviders(<PropertyDetailPage id="00000000-0000-0000-0000-000000000001" />);
    expect(screen.getByTestId('property-info-sections')).toBeInTheDocument();
    expect(screen.getByTestId('section-basic-info')).toBeInTheDocument();
  });
});
