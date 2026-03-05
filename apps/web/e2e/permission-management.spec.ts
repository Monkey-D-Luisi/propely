import { test, expect } from './fixtures/auth.fixture';
import { createOrg } from './helpers/org.helper';

test.describe('Permission management flow', () => {
  test('navigates to permission management from members page', async ({ authenticatedPage }) => {
    test.setTimeout(90_000);

    const { page } = authenticatedPage;
    const rnd = Math.random().toString(36).substring(2, 8);
    const orgName = `Perm Org ${Date.now()}-${rnd}`;

    // Step 1: Create an org
    await createOrg(page, orgName);

    // Step 2: Navigate to the org's members page
    await page.getByText(orgName).click();
    await page.waitForURL(/\/orgs\/[^/]+\/members/, { timeout: 15_000 });
    await page.waitForLoadState('networkidle');
    await expect(page.getByRole('heading', { name: 'Members' })).toBeVisible({
      timeout: 30_000,
    });

    // Step 3: Click the Permissions link in the members page
    await page.getByRole('link', { name: 'Permissions' }).click();
    await page.waitForURL(/\/orgs\/[^/]+\/permissions/, { timeout: 15_000 });
    await page.waitForLoadState('networkidle');

    // Step 4: Verify the permission management page renders
    await expect(page.getByText('Permission Management')).toBeVisible({
      timeout: 30_000,
    });
    await expect(page.getByText('Manage fine-grained permissions for branch members')).toBeVisible();

    // Step 5: Verify the owner (current user) appears in the members table
    await expect(page.getByText('Owner')).toBeVisible();
    await expect(page.getByText('Manage').first()).toBeVisible();
  });

  test('navigates to permission detail for a member', async ({ authenticatedPage }) => {
    test.setTimeout(90_000);

    const { page } = authenticatedPage;
    const rnd = Math.random().toString(36).substring(2, 8);
    const orgName = `PermDetail Org ${Date.now()}-${rnd}`;

    // Step 1: Create an org
    await createOrg(page, orgName);

    // Step 2: Navigate to members, then permissions
    await page.getByText(orgName).click();
    await page.waitForURL(/\/orgs\/[^/]+\/members/, { timeout: 15_000 });
    await page.waitForLoadState('networkidle');
    await page.getByRole('link', { name: 'Permissions' }).click();
    await page.waitForURL(/\/orgs\/[^/]+\/permissions/, { timeout: 15_000 });
    await page.waitForLoadState('networkidle');

    // Step 3: Click Manage for the first (and only) member
    await page.getByRole('link', { name: 'Manage' }).first().click();
    await page.waitForURL(/\/orgs\/[^/]+\/permissions\/[^/]+/, { timeout: 15_000 });
    await page.waitForLoadState('networkidle');

    // Step 4: Verify the permission detail page renders with categories
    await expect(page.getByText('Properties')).toBeVisible({ timeout: 30_000 });
    await expect(page.getByText('Contacts')).toBeVisible();
    await expect(page.getByText('Appointments')).toBeVisible();
    await expect(page.getByText('Leads')).toBeVisible();
    await expect(page.getByText('Reports')).toBeVisible();

    // Step 5: Verify owner note is shown
    await expect(
      page.getByText('Owners always have full access'),
    ).toBeVisible();
  });
});
