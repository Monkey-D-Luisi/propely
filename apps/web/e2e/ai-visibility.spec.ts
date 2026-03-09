import { test, expect, type Page } from '@playwright/test';

const BASE = 'http://localhost:3000';

/**
 * Login helper — requires orgs-api running on port 5020 for CSRF token.
 * If backend is down, the submit button stays disabled and login will time out.
 */
async function login(page: Page) {
  await page.goto(`${BASE}/en/login`, { waitUntil: 'domcontentloaded' });
  await page.fill('input[name="email"]', 'alice@propely.test');
  await page.fill('input[name="password"]', 'Demo123!');
  const submitBtn = page.locator('button[type="submit"]');
  await expect(submitBtn).toBeEnabled({ timeout: 30_000 });
  await submitBtn.click();
  await page.waitForURL('**/dashboard', { timeout: 30_000 });
  await expect(page.locator('aside')).toBeVisible({ timeout: 15_000 });
}

test.describe.configure({ mode: 'serial' });

test.describe('AI Visibility - No Auth Required', () => {
  test.setTimeout(60_000);

  test('login page renders correctly', async ({ page }) => {
    await page.goto(`${BASE}/en/login`, { waitUntil: 'domcontentloaded' });
    await expect(page.locator('input[name="email"]')).toBeVisible();
    await expect(page.locator('input[name="password"]')).toBeVisible();
    await page.screenshot({ path: 'test-results/login-page.png' });
  });
});

test.describe('AI Visibility - Authenticated', () => {
  test.setTimeout(120_000);

  test.beforeAll(async () => {
    // Skip all tests in this group if orgs-api is not running
    try {
      const res = await fetch('http://localhost:5020/health/live', { signal: AbortSignal.timeout(5_000) });
      if (!res.ok) test.skip();
    } catch {
      test.skip();
    }
  });

  test('sidebar shows AI Assistant entry and FAB after login', async ({ page }) => {
    await login(page);

    const aiButton = page.locator('button', { hasText: 'AI Assistant' });
    await expect(aiButton).toBeVisible();

    const fab = page.locator('button[aria-label="AI Assistant"]');
    await expect(fab).toBeVisible();

    await expect(page.locator('main.bg-surface')).toBeVisible();

    await page.screenshot({ path: 'test-results/ai-sidebar-and-fab.png', fullPage: false });
  });

  test('clicking AI Assistant opens command bar', async ({ page }) => {
    await login(page);

    await page.locator('button', { hasText: 'AI Assistant' }).click();

    const dialog = page.locator('[role="dialog"]');
    await expect(dialog).toBeVisible({ timeout: 5_000 });

    await page.screenshot({ path: 'test-results/ai-command-bar-open.png', fullPage: false });
  });
});
