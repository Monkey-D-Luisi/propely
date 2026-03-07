import { test, expect } from './fixtures/auth.fixture';
import { createOrg } from './helpers/org.helper';

test.describe('Invite member flow', () => {
  test('invites a member to an organization', async ({ authenticatedPage }) => {
    // This test spans multiple pages and API calls; CI Docker can be slow.
    test.setTimeout(90_000);

    const { page } = authenticatedPage;
    const rnd = Math.random().toString(36).substring(2, 8);
    const orgName = `Invite Org ${Date.now()}-${rnd}`;
    const inviteeEmail = `invitee-${Date.now()}-${rnd}@test.local`;

    // Step 1: Create an org
    await createOrg(page, orgName);

    // Step 2: Navigate to the org's members page
    await page.getByText(orgName).click();
    await page.waitForURL(/\/orgs\/[^/]+\/members/, { timeout: 15_000 });
    await page.waitForLoadState('networkidle');
    await expect(page.locator('h1', { hasText: 'Members' })).toBeVisible({
      timeout: 30_000,
    });

    // Step 3: Fill the invite form (scoped to the invite section)
    const inviteSection = page.locator('section').filter({
      has: page.getByText('Invite a member'),
    });
    await inviteSection.getByLabel('Email').fill(inviteeEmail);
    await inviteSection.getByRole('button', { name: 'Send invitation' }).click();

    // Step 4: Verify success toast
    await expect(page.getByText('Invitation sent')).toBeVisible({ timeout: 10_000 });
  });
});
