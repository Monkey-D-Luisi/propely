import { test, expect } from '@playwright/test';

const BASE = 'http://localhost:3000';

test.describe('Dashboard Zero-State', () => {
  test.setTimeout(30_000);

  test('shows get-started onboarding when no data', async ({ page }) => {
    await page.goto(`${BASE}/en/dashboard`, { waitUntil: 'domcontentloaded' });

    // The zero-state should show (no backend = no data = isEmpty)
    await expect(page.locator('text=Welcome to Propely!')).toBeVisible({ timeout: 10_000 });
    await expect(page.locator('text=Add your first property')).toBeVisible();
    await expect(page.locator('text=Try AI Assistant')).toBeVisible();

    // Numbered steps visible
    await expect(page.locator('text=Add a property')).toBeVisible();
    await expect(page.locator('text=Create a lead')).toBeVisible();
    await expect(page.locator('text=Schedule an appointment')).toBeVisible();

    await page.screenshot({ path: 'test-results/dashboard-zero-state.png', fullPage: false });
  });

  test('sidebar and FAB are visible on dashboard', async ({ page }) => {
    await page.goto(`${BASE}/en/dashboard`, { waitUntil: 'domcontentloaded' });

    // Sidebar AI entry
    const aiButton = page.locator('button', { hasText: 'AI Assistant' });
    await expect(aiButton).toBeVisible();

    // FAB
    const fab = page.locator('button[aria-label="AI Assistant"]');
    await expect(fab).toBeVisible();

    await page.screenshot({ path: 'test-results/dashboard-full-layout.png', fullPage: false });
  });
});
