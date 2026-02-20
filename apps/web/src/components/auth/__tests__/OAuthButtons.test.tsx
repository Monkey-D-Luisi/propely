// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { beforeEach, describe, expect, it, vi } from 'vitest';
import { OAuthButtons } from '@/components/auth/OAuthButtons';
import { renderWithProviders, screen } from '@test/utils';

describe('OAuthButtons', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('renders Google and GitHub buttons', () => {
    renderWithProviders(<OAuthButtons />);

    expect(screen.getByRole('button', { name: 'Google' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'GitHub' })).toBeInTheDocument();
  });

  it('redirects to Google OAuth endpoint with encoded next path', async () => {
    const assignSpy = vi.spyOn(window.location, 'assign').mockImplementation(() => {});
    const { user } = renderWithProviders(<OAuthButtons nextPath="/en/orgs/mine" />);

    await user.click(screen.getByRole('button', { name: 'Google' }));

    expect(assignSpy).toHaveBeenCalledWith(
      'http://localhost:5020/auth/oauth/google?next=%2Fen%2Forgs%2Fmine'
    );
  });

  it('redirects to GitHub OAuth endpoint with encoded next path', async () => {
    const assignSpy = vi.spyOn(window.location, 'assign').mockImplementation(() => {});
    const { user } = renderWithProviders(<OAuthButtons nextPath="/es/profile" />);

    await user.click(screen.getByRole('button', { name: 'GitHub' }));

    expect(assignSpy).toHaveBeenCalledWith(
      'http://localhost:5020/auth/oauth/github?next=%2Fes%2Fprofile'
    );
  });

  it('sanitizes non-relative next path to root', async () => {
    const assignSpy = vi.spyOn(window.location, 'assign').mockImplementation(() => {});
    const { user } = renderWithProviders(<OAuthButtons nextPath="https://malicious.example" />);

    await user.click(screen.getByRole('button', { name: 'Google' }));

    expect(assignSpy).toHaveBeenCalledWith(
      'http://localhost:5020/auth/oauth/google?next=%2F'
    );
  });

  it('disables both buttons after click', async () => {
    vi.spyOn(window.location, 'assign').mockImplementation(() => {});
    const { user } = renderWithProviders(<OAuthButtons />);

    const googleButton = screen.getByRole('button', { name: 'Google' });
    const gitHubButton = screen.getByRole('button', { name: 'GitHub' });

    await user.click(googleButton);

    expect(googleButton).toBeDisabled();
    expect(gitHubButton).toBeDisabled();
  });
});
