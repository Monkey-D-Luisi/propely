// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { test, expect } from '@playwright/test';

test.describe('Marketing pages', () => {
  test('home page loads and shows hero content', async ({ page }) => {
    await page.goto('/en');

    // The hero heading should be visible (contains "Propely" or brand text)
    await expect(page.locator('h1')).toBeVisible({ timeout: 10_000 });

    // CTA buttons should be present
    await expect(page.getByRole('link', { name: /get started/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /demo|pricing/i })).toBeVisible();

    // Feature cards section exists
    await expect(page.locator('h3').first()).toBeVisible();
  });

  test('privacy page loads and has content', async ({ page }) => {
    await page.goto('/en/privacy');

    // Page heading
    await expect(page.locator('h1')).toBeVisible({ timeout: 10_000 });

    // Content sections should be present
    await expect(page.locator('h2').first()).toBeVisible();

    // Contact email at the bottom
    await expect(page.locator('section').last()).toBeVisible();
  });

  test('terms page loads and has content', async ({ page }) => {
    await page.goto('/en/terms');

    // Page heading
    await expect(page.locator('h1')).toBeVisible({ timeout: 10_000 });

    // Multiple sections should be present
    const sections = page.locator('h2');
    await expect(sections.first()).toBeVisible();
    expect(await sections.count()).toBeGreaterThan(3);
  });
});

test.describe('Auth-protected navigation', () => {
  test('unauthenticated user accessing properties is redirected to login', async ({ page }) => {
    // Clear any cookies/state
    await page.context().clearCookies();

    // Try to access the properties page (which is auth-protected)
    await page.goto('/en/properties');

    // Should redirect to login page
    await page.waitForURL(/\/login/, { timeout: 15_000 });
    await expect(page).toHaveURL(/\/login/);
  });

  test('unauthenticated user accessing contacts is redirected to login', async ({ page }) => {
    await page.context().clearCookies();

    await page.goto('/en/contacts');

    await page.waitForURL(/\/login/, { timeout: 15_000 });
    await expect(page).toHaveURL(/\/login/);
  });

  test('unauthenticated user accessing appointments is redirected to login', async ({ page }) => {
    await page.context().clearCookies();

    await page.goto('/en/appointments');

    await page.waitForURL(/\/login/, { timeout: 15_000 });
    await expect(page).toHaveURL(/\/login/);
  });

  test('unauthenticated user accessing orgs is redirected to login', async ({ page }) => {
    await page.context().clearCookies();

    await page.goto('/en/orgs/mine');

    await page.waitForURL(/\/login/, { timeout: 15_000 });
    await expect(page).toHaveURL(/\/login/);
  });
});
