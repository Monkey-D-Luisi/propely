// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { test, expect } from '@playwright/test';
import { mockAuthenticatedSession, mockOrg } from './helpers/mock-session.helper';

const PROPERTIES_API_BASE = process.env.NEXT_PUBLIC_PROPERTIES_API_URL ?? 'http://localhost:5030';

// Mock property data matching the PropertyListItem schema
const mockPropertyListItem = {
  id: '00000000-0000-0000-0000-000000000100',
  title: 'Modern Apartment in Madrid',
  propertyType: 'Apartment',
  operationType: 'Sale',
  status: 'Active',
  price: 350000,
  city: 'Madrid',
  builtArea: 120,
  bedrooms: 3,
  bathrooms: 2,
  agentId: '00000000-0000-0000-0000-000000000001',
  createdAtUtc: '2026-01-15T10:00:00Z',
  updatedAtUtc: null,
};

const mockPropertyListItem2 = {
  id: '00000000-0000-0000-0000-000000000101',
  title: 'Beach House in Valencia',
  propertyType: 'House',
  operationType: 'Rent',
  status: 'Draft',
  price: 1500,
  city: 'Valencia',
  builtArea: 200,
  bedrooms: 4,
  bathrooms: 3,
  agentId: '00000000-0000-0000-0000-000000000001',
  createdAtUtc: '2026-02-20T14:30:00Z',
  updatedAtUtc: null,
};

// Full property detail matching the Property schema
const mockPropertyDetail = {
  id: '00000000-0000-0000-0000-000000000100',
  title: 'Modern Apartment in Madrid',
  propertyType: 'Apartment',
  operationType: 'Sale',
  status: 'Active',
  tenantId: mockOrg.id,
  agentId: '00000000-0000-0000-0000-000000000001',
  agencyId: null,
  description: { en: 'A beautiful modern apartment in the heart of Madrid.', es: 'Un hermoso apartamento moderno en el coraz\u00f3n de Madrid.' },
  address: {
    street: 'Calle Gran Via 42',
    city: 'Madrid',
    state: 'Madrid',
    postalCode: '28013',
    country: 'Spain',
    latitude: 40.4200,
    longitude: -3.7025,
  },
  features: {
    builtArea: 120,
    usableArea: 100,
    bedrooms: 3,
    bathrooms: 2,
    parkingSpaces: 1,
    yearBuilt: 2020,
    floor: 5,
    hasElevator: true,
    hasTerrace: true,
    hasPool: false,
    hasGarden: false,
    hasGarage: true,
    energyRating: 'B',
  },
  financials: {
    price: 350000,
    currency: 'EUR',
    communityFees: 150,
    propertyTax: 1200,
  },
  virtualTourUrl: null,
  videoUrl: null,
  pricePerSqm: 2916.67,
  createdAtUtc: '2026-01-15T10:00:00Z',
  updatedAtUtc: null,
  publishedAtUtc: '2026-01-16T08:00:00Z',
};

test.describe('Properties list page (mocked API)', () => {
  test.beforeEach(async ({ page }) => {
    await mockAuthenticatedSession(page);
  });

  test('renders properties list with mocked data', async ({ page }) => {
    // Mock properties list endpoint
    await page.route(`${PROPERTIES_API_BASE}/api/properties?*`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [mockPropertyListItem, mockPropertyListItem2],
          pageNumber: 1,
          totalPages: 1,
          totalCount: 2,
          hasPreviousPage: false,
          hasNextPage: false,
        }),
      });
    });

    await page.goto('/en/properties');

    // Page heading should be visible
    await expect(page.getByRole('heading', { name: 'Properties' })).toBeVisible({ timeout: 15_000 });

    // Property data should appear in the table/cards
    await expect(page.getByText('Modern Apartment in Madrid')).toBeVisible({ timeout: 10_000 });
    await expect(page.getByText('Beach House in Valencia')).toBeVisible();
  });

  test('shows empty state when no properties exist', async ({ page }) => {
    await page.route(`${PROPERTIES_API_BASE}/api/properties?*`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [],
          pageNumber: 1,
          totalPages: 0,
          totalCount: 0,
          hasPreviousPage: false,
          hasNextPage: false,
        }),
      });
    });

    await page.goto('/en/properties');

    // Wait for page to load and show empty state
    await expect(page.getByRole('heading', { name: 'Properties' })).toBeVisible({ timeout: 15_000 });

    // The "New property" button should still be visible
    await expect(page.getByRole('link', { name: /new property/i })).toBeVisible({ timeout: 10_000 });
  });

  test('new property link navigates to creation form', async ({ page }) => {
    await page.route(`${PROPERTIES_API_BASE}/api/properties?*`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [mockPropertyListItem],
          pageNumber: 1,
          totalPages: 1,
          totalCount: 1,
          hasPreviousPage: false,
          hasNextPage: false,
        }),
      });
    });

    await page.goto('/en/properties');

    await expect(page.getByRole('heading', { name: 'Properties' })).toBeVisible({ timeout: 15_000 });

    // Click "New property" link
    const newPropertyLink = page.getByRole('link', { name: /new property/i });
    await expect(newPropertyLink).toBeVisible({ timeout: 10_000 });
    await newPropertyLink.click();

    await page.waitForURL(/\/properties\/new/, { timeout: 10_000 });
    await expect(page).toHaveURL(/\/properties\/new/);
  });
});

test.describe('Property creation form (mocked API)', () => {
  test.beforeEach(async ({ page }) => {
    await mockAuthenticatedSession(page);
  });

  test('property creation form renders', async ({ page }) => {
    await page.goto('/en/properties/new');

    // Breadcrumb should show "Properties" link
    await expect(page.getByRole('link', { name: 'Properties' })).toBeVisible({ timeout: 15_000 });

    // Page heading for new property form
    await expect(page.getByRole('heading', { name: /create property/i })).toBeVisible();
  });
});

test.describe('Property detail page (mocked API)', () => {
  test.beforeEach(async ({ page }) => {
    await mockAuthenticatedSession(page);
  });

  test('property detail page renders with mocked data', async ({ page }) => {
    const propertyId = mockPropertyDetail.id;

    // Mock single property endpoint
    await page.route(`${PROPERTIES_API_BASE}/api/properties/${propertyId}`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockPropertyDetail),
      });
    });

    // Mock media endpoint (empty)
    await page.route(`${PROPERTIES_API_BASE}/api/properties/${propertyId}/media`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await page.goto(`/en/properties/${propertyId}`);

    // Property title should appear (use testid to avoid matching breadcrumb)
    await expect(page.getByTestId('property-title')).toContainText('Modern Apartment in Madrid', { timeout: 15_000 });

    // Address info should be visible
    await expect(page.getByTestId('property-detail')).toContainText('Madrid');
  });
});
