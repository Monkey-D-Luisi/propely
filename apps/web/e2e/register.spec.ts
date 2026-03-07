import { test, expect } from './fixtures/auth.fixture';

test.describe('Registration flow', () => {
  test('registers a new user and redirects away from register page', async ({
    page,
    testUser,
  }) => {
    await page.goto('/en/register');

    await expect(page.getByRole('heading', { name: 'Propely' })).toBeVisible();

    await page.getByLabel('Name (optional)').fill(testUser.name);
    await page.getByLabel('Email').fill(testUser.email);
    await page.getByLabel('Password', { exact: true }).fill(testUser.password);
    await page.getByLabel('Confirm password').fill(testUser.password);

    await page.getByRole('button', { name: 'Create account' }).click();

    await page.waitForURL(/(?!.*\/register)/, { timeout: 15_000 });
    await expect(page).not.toHaveURL(/\/register/);
  });

  test('shows validation errors for empty required fields', async ({ page }) => {
    await page.goto('/en/register');

    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(page.getByText('Email is required.')).toBeVisible();
    await expect(page.getByText('Password is required.', { exact: true })).toBeVisible();
  });

  test('shows error when passwords do not match', async ({ page, testUser }) => {
    await page.goto('/en/register');

    await page.getByLabel('Email').fill(testUser.email);
    await page.getByLabel('Password', { exact: true }).fill(testUser.password);
    await page.getByLabel('Confirm password').fill('DifferentPassword999!');

    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(page.getByText('Passwords do not match.')).toBeVisible();
  });

  test('shows error when registering with an already-used email', async ({
    registeredPage,
  }) => {
    const { page, user } = registeredPage;

    // Clear auth state left by registeredPage so /en/register doesn't redirect
    await page.context().clearCookies();
    await page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });

    // Navigate back to register with the same email
    await page.goto('/en/register');
    await expect(page.getByRole('heading', { name: 'Propely' })).toBeVisible();

    await page.getByLabel('Email').fill(user.email);
    await page.getByLabel('Password', { exact: true }).fill(user.password);
    await page.getByLabel('Confirm password').fill(user.password);

    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(
      page.getByText('An account with this email already exists.')
    ).toBeVisible({ timeout: 10_000 });
  });
});
