// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { test, expect } from '@playwright/test';
import { mockAuthenticatedSession } from './helpers/mock-session.helper';

// Mock contact data matching the ContactListItem interface
const mockContactListItem1 = {
  id: '00000000-0000-0000-0000-000000000200',
  firstName: 'Maria',
  lastName: 'Garcia',
  email: 'maria.garcia@example.com',
  phone: '+34 600 123 456',
  company: 'Garcia Inmobiliaria',
  roles: ['Buyer'],
  createdAtUtc: '2026-01-10T09:00:00Z',
};

const mockContactListItem2 = {
  id: '00000000-0000-0000-0000-000000000201',
  firstName: 'Carlos',
  lastName: 'Rodriguez',
  email: 'carlos.rodriguez@example.com',
  phone: '+34 600 789 012',
  company: null,
  roles: ['Seller', 'Landlord'],
  createdAtUtc: '2026-02-05T16:00:00Z',
};

const mockContactListItem3 = {
  id: '00000000-0000-0000-0000-000000000202',
  firstName: 'Elena',
  lastName: 'Martinez',
  email: 'elena.martinez@example.com',
  phone: null,
  company: 'Martinez & Partners',
  roles: ['Professional'],
  createdAtUtc: '2026-03-01T11:30:00Z',
};

// Full contact detail matching the Contact interface
const mockContactDetail = {
  id: '00000000-0000-0000-0000-000000000200',
  firstName: 'Maria',
  lastName: 'Garcia',
  email: 'maria.garcia@example.com',
  phone: '+34 600 123 456',
  secondaryPhone: null,
  company: 'Garcia Inmobiliaria',
  notes: 'Interested in apartments in central Madrid.',
  preferredLanguage: 'es',
  source: 'Website',
  assignedAgentId: '00000000-0000-0000-0000-000000000001',
  tenantId: '00000000-0000-0000-0000-000000000010',
  roles: ['Buyer'],
  propertyInterests: [],
  createdAtUtc: '2026-01-10T09:00:00Z',
  updatedAtUtc: '2026-02-15T14:00:00Z',
};

test.describe('Contacts list page (mocked API)', () => {
  test.beforeEach(async ({ page }) => {
    await mockAuthenticatedSession(page);
  });

  test('renders contacts list with mocked data', async ({ page }) => {
    // Mock contacts list endpoint
    await page.route(/\/api\/contacts\?/, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [mockContactListItem1, mockContactListItem2, mockContactListItem3],
          pageNumber: 1,
          totalPages: 1,
          totalCount: 3,
          hasPreviousPage: false,
          hasNextPage: false,
        }),
      });
    });

    await page.goto('/en/contacts');

    // Page heading should be visible
    await expect(page.getByRole('heading', { name: 'Contacts' })).toBeVisible({ timeout: 15_000 });

    // Contact names should be displayed
    await expect(page.getByText('Maria')).toBeVisible({ timeout: 10_000 });
    await expect(page.getByText('Carlos')).toBeVisible();
    await expect(page.getByText('Elena')).toBeVisible();
  });

  test('shows empty state when no contacts exist', async ({ page }) => {
    await page.route(/\/api\/contacts\?/, async (route) => {
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

    await page.goto('/en/contacts');

    // Wait for page to load
    await expect(page.getByRole('heading', { name: 'Contacts' })).toBeVisible({ timeout: 15_000 });

    // The "New contact" button should be visible
    await expect(page.getByRole('button', { name: /new contact/i })).toBeVisible({ timeout: 10_000 });
  });

  test('new contact button toggles the creation form', async ({ page }) => {
    await page.route(/\/api\/contacts\?/, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [mockContactListItem1],
          pageNumber: 1,
          totalPages: 1,
          totalCount: 1,
          hasPreviousPage: false,
          hasNextPage: false,
        }),
      });
    });

    await page.goto('/en/contacts');

    await expect(page.getByRole('heading', { name: 'Contacts' })).toBeVisible({ timeout: 15_000 });

    // Click "New contact" button to toggle the form
    const newContactBtn = page.getByRole('button', { name: /new contact/i });
    await expect(newContactBtn).toBeVisible({ timeout: 10_000 });
    await newContactBtn.click();

    // The form should now be visible (look for form fields)
    await expect(page.getByLabel(/first name/i)).toBeVisible({ timeout: 5_000 });
    await expect(page.getByLabel(/email/i)).toBeVisible();
  });
});

test.describe('Contact detail page (mocked API)', () => {
  test.beforeEach(async ({ page }) => {
    await mockAuthenticatedSession(page);
  });

  test('contact detail page renders with mocked data', async ({ page }) => {
    const contactId = mockContactDetail.id;

    // Mock single contact endpoint
    await page.route(new RegExp(`/api/contacts/${contactId}$`), async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockContactDetail),
      });
    });

    await page.goto(`/en/contacts/${contactId}`);

    // Contact name should appear (use testid to avoid matching breadcrumb)
    await expect(page.getByTestId('contact-name')).toContainText('Maria Garcia', { timeout: 15_000 });

    // Email should be visible
    await expect(page.getByText('maria.garcia@example.com')).toBeVisible();
  });

  test('contact detail page shows error for non-existent contact', async ({ page }) => {
    const fakeId = '00000000-0000-0000-0000-999999999999';

    // Mock the contact endpoint to return 404
    await page.route(new RegExp(`/api/contacts/${fakeId}$`), async (route) => {
      await route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({ title: 'Not Found', status: 404, detail: 'Contact not found' }),
      });
    });

    await page.goto(`/en/contacts/${fakeId}`);

    // Error state should be visible, or "Back to list" link
    await expect(
      page.getByText(/error|not found|back to/i).first()
    ).toBeVisible({ timeout: 15_000 });
  });
});
