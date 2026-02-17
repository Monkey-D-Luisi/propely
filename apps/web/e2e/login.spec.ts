import { test, expect } from './fixtures/auth.fixture';

test.describe('Login flow', () => {
  test('logs in with valid credentials after registration', async ({ registeredPage }) => {
    const { page, user } = registeredPage;

    // Clear session so we can test the actual login flow
    await page.context().clearCookies();
    await page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });
    await page.goto('about:blank');

    await page.goto('/en/login');
    await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible();

    await page.getByLabel('Email').fill(user.email);
    await page.getByLabel('Password').fill(user.password);
    await page.getByRole('button', { name: 'Sign in' }).click();

    await page.waitForURL(/(?!.*\/login)/, { timeout: 15_000 });
    await expect(page).not.toHaveURL(/\/login/);
  });

  test('shows error for invalid credentials', async ({ page }) => {
    await page.goto('/en/login');

    await page.getByLabel('Email').fill('nonexistent@test.local');
    await page.getByLabel('Password').fill('WrongPassword123!');
    await page.getByRole('button', { name: 'Sign in' }).click();

    await expect(
      page.getByText('We could not find a user with that email and password.')
    ).toBeVisible({ timeout: 10_000 });
  });

  test('shows validation errors for empty form', async ({ page }) => {
    await page.goto('/en/login');

    await page.getByRole('button', { name: 'Sign in' }).click();

    await expect(page.getByText('Email is required.')).toBeVisible();
    await expect(page.getByText('Password is required.')).toBeVisible();
  });
});
