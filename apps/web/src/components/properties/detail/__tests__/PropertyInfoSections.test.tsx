// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import { PropertyInfoSections } from '../PropertyInfoSections';
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
      latitude: 40.4200,
      longitude: -3.7025,
    },
    features: {
      bedrooms: 3,
      bathrooms: 2,
      builtArea: 120,
      usableArea: 105,
      plotArea: null,
      floor: 5,
      yearBuilt: 2020,
      orientation: 'SE',
      energyRating: 'B',
      hasPool: true,
      hasGarden: false,
      hasGarage: true,
      hasElevator: true,
      hasTerrace: false,
      airConditioning: true,
      heating: true,
      furnished: false,
    },
    financials: {
      price: 450000,
      communityFees: 150,
      ibiTax: 1200,
      catastroReference: 'REF-12345',
    },
    pricePerSqm: 3750,
    description: {
      es: 'Un apartamento moderno en el centro de Madrid.',
      en: 'A modern apartment in the center of Madrid.',
      pt: null,
      fr: null,
      de: null,
      nl: null,
    },
    ...overrides,
  };
}

describe('PropertyInfoSections', () => {
  it('renders all five sections', () => {
    const property = makeProperty();
    renderWithProviders(<PropertyInfoSections property={property} />);

    expect(screen.getByTestId('section-basic-info')).toBeInTheDocument();
    expect(screen.getByTestId('section-location')).toBeInTheDocument();
    expect(screen.getByTestId('section-features')).toBeInTheDocument();
    expect(screen.getByTestId('section-financial')).toBeInTheDocument();
    expect(screen.getByTestId('section-descriptions')).toBeInTheDocument();
  });

  it('renders basic information correctly', () => {
    const property = makeProperty();
    renderWithProviders(<PropertyInfoSections property={property} />);

    expect(screen.getByText('Basic Information')).toBeInTheDocument();
    expect(screen.getByText('Apartment')).toBeInTheDocument();
    expect(screen.getByText('Sale')).toBeInTheDocument();
    expect(screen.getByText('Modern Apartment in Madrid')).toBeInTheDocument();
  });

  it('renders location information', () => {
    const property = makeProperty();
    renderWithProviders(<PropertyInfoSections property={property} />);

    expect(screen.getByText('Location')).toBeInTheDocument();
    expect(screen.getByText('Calle Gran Via 42')).toBeInTheDocument();
    // City and province both "Madrid" so use getAllByText
    const madridElements = screen.getAllByText('Madrid');
    expect(madridElements.length).toBeGreaterThanOrEqual(2);
    expect(screen.getByText('28013')).toBeInTheDocument();
    expect(screen.getByText('Spain')).toBeInTheDocument();
  });

  it('renders map placeholder when lat/lng available', () => {
    const property = makeProperty();
    renderWithProviders(<PropertyInfoSections property={property} />);
    expect(screen.getByTestId('map-placeholder')).toBeInTheDocument();
  });

  it('renders features with correct values', () => {
    const property = makeProperty();
    renderWithProviders(<PropertyInfoSections property={property} />);

    expect(screen.getByText('Features')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument(); // bedrooms
    expect(screen.getByText('2')).toBeInTheDocument(); // bathrooms
    expect(screen.getByText('120 m\u00b2')).toBeInTheDocument(); // builtArea
    expect(screen.getByText('105 m\u00b2')).toBeInTheDocument(); // usableArea
    expect(screen.getByText('5')).toBeInTheDocument(); // floor
    expect(screen.getByText('2020')).toBeInTheDocument(); // yearBuilt
    expect(screen.getByText('SE')).toBeInTheDocument(); // orientation
    expect(screen.getByText('B')).toBeInTheDocument(); // energyRating
  });

  it('renders only active amenity badges', () => {
    const property = makeProperty();
    renderWithProviders(<PropertyInfoSections property={property} />);

    // Active amenities
    expect(screen.getByText('Pool')).toBeInTheDocument();
    expect(screen.getByText('Garage')).toBeInTheDocument();
    expect(screen.getByText('Elevator')).toBeInTheDocument();
    expect(screen.getByText('Air Conditioning')).toBeInTheDocument();
    expect(screen.getByText('Heating')).toBeInTheDocument();

    // Inactive amenities should NOT be rendered
    expect(screen.queryByText('Garden')).not.toBeInTheDocument();
    expect(screen.queryByText('Terrace')).not.toBeInTheDocument();
    expect(screen.queryByText('Furnished')).not.toBeInTheDocument();
  });

  it('formats price correctly as EUR', () => {
    const property = makeProperty();
    renderWithProviders(<PropertyInfoSections property={property} />);

    const priceDisplay = screen.getByTestId('price-display');
    // Should be formatted as Spanish EUR (e.g., "450.000 EUR" or similar)
    expect(priceDisplay).toBeInTheDocument();
    expect(priceDisplay.textContent).toContain('450');
  });

  it('renders price per sqm when available', () => {
    const property = makeProperty();
    renderWithProviders(<PropertyInfoSections property={property} />);

    const pricePerSqm = screen.getByTestId('price-per-sqm');
    expect(pricePerSqm).toBeInTheDocument();
    expect(pricePerSqm.textContent).toContain('3');
  });

  it('renders financial details', () => {
    const property = makeProperty();
    renderWithProviders(<PropertyInfoSections property={property} />);

    expect(screen.getByText('Financial')).toBeInTheDocument();
    expect(screen.getByText('REF-12345')).toBeInTheDocument();
  });

  it('renders language tabs for descriptions with content', () => {
    const property = makeProperty();
    renderWithProviders(<PropertyInfoSections property={property} />);

    expect(screen.getByText('Descriptions')).toBeInTheDocument();
    // Only es and en have content
    expect(screen.getByTestId('lang-tab-es')).toBeInTheDocument();
    expect(screen.getByTestId('lang-tab-en')).toBeInTheDocument();
    // pt has null, so no tab
    expect(screen.queryByTestId('lang-tab-pt')).not.toBeInTheDocument();
    expect(screen.queryByTestId('lang-tab-fr')).not.toBeInTheDocument();
  });

  it('shows "No description available" when no descriptions exist', () => {
    const property = makeProperty({ description: null });
    renderWithProviders(<PropertyInfoSections property={property} />);

    expect(screen.getByText('No description available')).toBeInTheDocument();
  });

  it('switches description tab when clicked', async () => {
    const property = makeProperty();
    const { user } = renderWithProviders(<PropertyInfoSections property={property} />);

    // Default tab should be es (first with content)
    const content = screen.getByTestId('description-content');
    expect(content.textContent).toContain('Un apartamento moderno');

    // Click English tab
    await user.click(screen.getByTestId('lang-tab-en'));
    expect(content.textContent).toContain('A modern apartment in the center of Madrid.');
  });

  it('renders without features section data gracefully', () => {
    const property = makeProperty({ features: null });
    renderWithProviders(<PropertyInfoSections property={property} />);

    // Features section should still exist but with no info items
    expect(screen.getByTestId('section-features')).toBeInTheDocument();
    // No amenities section rendered
    expect(screen.queryByTestId('amenities-section')).not.toBeInTheDocument();
  });
});
