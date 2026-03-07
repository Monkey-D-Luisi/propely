// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import type { Page } from '@playwright/test';

// Default mock user returned by /auth/me
export const mockUser = {
  id: '00000000-0000-0000-0000-000000000001',
  email: 'e2e-mock@test.local',
  name: 'E2E Mock User',
  emailVerified: true,
  isSystemAdmin: false,
};

// Default mock org returned by /orgs/mine
export const mockOrg = {
  id: '00000000-0000-0000-0000-000000000010',
  name: 'E2E Test Agency',
  description: 'Mock org for E2E tests',
  role: 'Owner',
};

/**
 * Sets up route interception for a fully authenticated user session.
 * Mocks the /auth/me, /auth/csrf, and /orgs/mine endpoints so that
 * dashboard pages render without needing a real backend.
 *
 * The orgs-api base URL defaults to localhost:5020.
 */
export async function mockAuthenticatedSession(page: Page): Promise<void> {
  const orgsApiBase = process.env.NEXT_PUBLIC_ORGS_API_URL ?? 'http://localhost:5020';

  // Mock /auth/me -> return authenticated user
  await page.route(`${orgsApiBase}/auth/me`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ user: mockUser }),
    });
  });

  // Mock /auth/csrf -> return a dummy CSRF token
  await page.route(`${orgsApiBase}/auth/csrf`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ csrfToken: 'mock-csrf-token-for-e2e' }),
    });
  });

  // Mock /orgs/mine -> return a single org
  await page.route(`${orgsApiBase}/orgs/mine*`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [mockOrg],
        pageNumber: 1,
        totalPages: 1,
        totalCount: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      }),
    });
  });
}
