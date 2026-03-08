// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { test, expect } from '@playwright/test';

test.describe('Login page smoke tests', () => {
  test('renders login page with heading and form fields', async ({ page }) => {
    await page.goto('/en/login');

    // Brand heading is visible
    await expect(page.getByRole('heading', { name: 'Propely' })).toBeVisible();

    // Email and password fields are present
    await expect(page.getByLabel('Email')).toBeVisible();
    await expect(page.getByLabel('Password')).toBeVisible();

    // Sign in button is present
    await expect(page.getByRole('button', { name: 'Sign in' })).toBeVisible();
  });

  test('shows validation errors for empty fields', async ({ page }) => {
    await page.goto('/en/login');

    // Submit without filling any fields
    await page.getByRole('button', { name: 'Sign in' }).click();

    // Both validation errors should appear
    await expect(page.getByText('Email is required.')).toBeVisible();
    await expect(page.getByText('Password is required.')).toBeVisible();
  });

  test('shows error message for invalid credentials', async ({ page }) => {
    await page.goto('/en/login');

    await page.getByLabel('Email').fill('nonexistent@test.local');
    await page.getByLabel('Password').fill('WrongPassword123!');
    await page.getByRole('button', { name: 'Sign in' }).click();

    // Error message should be displayed
    await expect(
      page.getByText('We could not find a user with that email and password.')
    ).toBeVisible({ timeout: 15_000 });
  });

  test('has a link to the register page', async ({ page }) => {
    await page.goto('/en/login');

    const registerLink = page.getByRole('link', { name: /create|sign up|register/i });
    await expect(registerLink).toBeVisible();
  });
});

test.describe('Register page smoke tests', () => {
  test('renders register page with heading and form fields', async ({ page }) => {
    await page.goto('/en/register');

    // Brand heading is visible
    await expect(page.getByRole('heading', { name: 'Propely' })).toBeVisible();

    // All form fields are present
    await expect(page.getByLabel('Name (optional)')).toBeVisible();
    await expect(page.getByLabel('Email')).toBeVisible();
    await expect(page.getByLabel('Password', { exact: true })).toBeVisible();
    await expect(page.getByLabel('Confirm password')).toBeVisible();

    // Create account button is present
    await expect(page.getByRole('button', { name: 'Create account' })).toBeVisible();
  });

  test('shows validation errors for empty required fields', async ({ page }) => {
    await page.goto('/en/register');

    // Submit without filling any fields
    await page.getByRole('button', { name: 'Create account' }).click();

    // Required field validation errors
    await expect(page.getByText('Email is required.')).toBeVisible();
    await expect(page.getByText('Password is required.', { exact: true })).toBeVisible();
  });

  test('shows error when passwords do not match', async ({ page }) => {
    await page.goto('/en/register');

    await page.getByLabel('Email').fill('test@example.com');
    await page.getByLabel('Password', { exact: true }).fill('StrongPassword123!');
    await page.getByLabel('Confirm password').fill('DifferentPassword999!');

    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(page.getByText('Passwords do not match.')).toBeVisible();
  });

  test('has a link to the login page', async ({ page }) => {
    await page.goto('/en/register');

    const loginLink = page.getByRole('link', { name: /sign in|log in|login/i });
    await expect(loginLink).toBeVisible();
  });
});
