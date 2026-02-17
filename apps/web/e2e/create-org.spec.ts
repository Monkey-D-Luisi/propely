import { test, expect } from './fixtures/auth.fixture';
import { createOrg } from './helpers/org.helper';

test.describe('Create organization flow', () => {
  test('creates a new org and sees it in the list', async ({ authenticatedPage }) => {
    const { page } = authenticatedPage;
    const orgName = `E2E Org ${Date.now()}-${Math.random().toString(36).substring(2, 8)}`;

    await createOrg(page, orgName);

    await expect(page.getByText(orgName)).toBeVisible({ timeout: 10_000 });
  });
});
