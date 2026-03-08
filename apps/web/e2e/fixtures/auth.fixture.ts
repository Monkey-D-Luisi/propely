import { test as base, expect, type Page } from '@playwright/test';

export interface TestUser {
  name: string;
  email: string;
  password: string;
}

interface AuthFixtures {
  testUser: TestUser;
  registeredPage: { page: Page; user: TestUser };
  authenticatedPage: { page: Page; user: TestUser };
}

async function registerUser(page: Page, user: TestUser): Promise<void> {
  await page.goto('/en/register');
  await page.getByLabel('Name (optional)').fill(user.name);
  await page.getByLabel('Email').fill(user.email);
  await page.getByLabel('Password', { exact: true }).fill(user.password);
  await page.getByLabel('Confirm password').fill(user.password);
  await page.getByRole('button', { name: 'Create account' }).click();
  await page.waitForURL((url) => !url.pathname.includes('/register'), { timeout: 15_000 });
  await page.waitForLoadState('networkidle');
}

export const test = base.extend<AuthFixtures>({
  testUser: async ({}, use) => {
    const ts = Date.now();
    const rnd = Math.random().toString(36).substring(2, 8);
    await use({
      name: `E2E User ${ts}`,
      email: `e2e-${ts}-${rnd}@test.local`,
      password: 'TestPassword123!',
    });
  },

  registeredPage: async ({ page, testUser }, use) => {
    await registerUser(page, testUser);
    await use({ page, user: testUser });
  },

  authenticatedPage: async ({ registeredPage }, use) => {
    await use(registeredPage);
  },
});

export { expect };
