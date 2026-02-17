import { expect, type Page } from '@playwright/test';

export async function createOrg(page: Page, orgName: string): Promise<void> {
  await page.goto('/en/orgs/mine');
  await page.waitForLoadState('networkidle');
  await expect(page.getByRole('heading', { name: 'My organizations' })).toBeVisible({
    timeout: 15_000,
  });
  await page.getByRole('button', { name: 'New organization' }).click();
  await page.getByLabel('Organization name').fill(orgName);
  // Register both response listeners BEFORE clicking to avoid race conditions.
  // The GET refetch may fire immediately after the POST completes, so we must
  // be listening for it before the click triggers the POST.
  const postDone = page.waitForResponse(
    (resp) => resp.url().includes('/orgs') && resp.request().method() === 'POST' && resp.ok(),
  );
  const getDone = page.waitForResponse(
    (resp) => resp.url().includes('/orgs/mine') && resp.request().method() === 'GET' && resp.ok(),
  );
  await page.getByRole('button', { name: 'Create' }).click();
  await postDone;
  await getDone;
  await expect(page.getByText(orgName)).toBeVisible({ timeout: 15_000 });
}
