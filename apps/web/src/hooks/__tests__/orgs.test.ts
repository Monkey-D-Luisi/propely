// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import {
  useCurrentUser,
  useMyOrgs,
  useMembers,
  useCreateOrg,
  useInviteMember,
  useUpdateRole,
  useLeaveOrg,
  useDeleteOrg,
  useRemoveMember,
} from '@/hooks/orgs';
import type { Me, Org, Member } from '@/lib/schemas';
import { AUTH_USER_UPDATED_EVENT } from '@/lib/auth-events';

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return {
    ...actual,
    apiFetch: vi.fn(),
  };
});

vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn().mockResolvedValue('mock-csrf-token'),
}));

import { apiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';

const mockApiFetch = vi.mocked(apiFetch);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);

// ── Test data ──────────────────────────────────────────────

const mockUser: Me = {
  id: '00000000-0000-0000-0000-000000000001',
  email: 'test@example.com',
  name: 'Test User',
  emailVerified: true,
};

const mockOrgs: Org[] = [
  { id: '00000000-0000-0000-0000-000000000010', name: 'Org A', role: 'owner' },
  { id: '00000000-0000-0000-0000-000000000020', name: 'Org B', role: 'agent' },
];

const mockMembers: Member[] = [
  { userId: '00000000-0000-0000-0000-000000000001', email: 'alice@example.com', name: 'Alice', role: 'owner' },
  { userId: '00000000-0000-0000-0000-000000000002', email: 'bob@example.com', name: 'Bob', role: 'agent' },
];

const ORG_ID = '00000000-0000-0000-0000-000000000010';

function pagedResponse<T>(items: T[], totalCount?: number) {
  const count = totalCount ?? items.length;
  return {
    items,
    pageNumber: 1,
    totalPages: 1,
    totalCount: count,
    hasPreviousPage: false,
    hasNextPage: false,
  };
}

beforeEach(() => {
  vi.resetAllMocks();
  mockEnsureCsrfToken.mockResolvedValue('mock-csrf-token');
});

// ── useCurrentUser ─────────────────────────────────────────

describe('useCurrentUser', () => {
  it('starts in loading state', () => {
    mockApiFetch.mockReturnValue(new Promise(() => {})); // never resolves
    const { result } = renderHook(() => useCurrentUser());

    expect(result.current.isLoading).toBe(true);
    expect(result.current.user).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it('returns user on success', async () => {
    mockApiFetch.mockResolvedValueOnce({ user: mockUser });
    const { result } = renderHook(() => useCurrentUser());

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.user).toEqual(mockUser);
    expect(result.current.error).toBeNull();
    expect(mockApiFetch).toHaveBeenCalledWith('/auth/me', expect.objectContaining({ method: 'GET' }), expect.anything());
  });

  it('sets error on failure', async () => {
    const error = new Error('Network error');
    mockApiFetch.mockRejectedValueOnce(error);
    const { result } = renderHook(() => useCurrentUser());

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.user).toBeNull();
    expect(result.current.error).toBe(error);
  });

  it('refetches user when auth user updated event is dispatched', async () => {
    const unverifiedUser: Me = { ...mockUser, emailVerified: false };
    const verifiedUser: Me = { ...mockUser, emailVerified: true };
    mockApiFetch
      .mockResolvedValueOnce({ user: unverifiedUser })
      .mockResolvedValueOnce({ user: verifiedUser });

    const { result } = renderHook(() => useCurrentUser());

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.user).toEqual(unverifiedUser);

    act(() => {
      window.dispatchEvent(new Event(AUTH_USER_UPDATED_EVENT));
    });

    await waitFor(() => {
      expect(mockApiFetch).toHaveBeenCalledTimes(2);
    });
    await waitFor(() => {
      expect(result.current.user).toEqual(verifiedUser);
    });
  });
});

// ── useMyOrgs ──────────────────────────────────────────────

describe('useMyOrgs', () => {
  it('starts in loading state', () => {
    mockApiFetch.mockReturnValue(new Promise(() => {}));
    const { result } = renderHook(() => useMyOrgs());

    expect(result.current.isLoading).toBe(true);
    expect(result.current.orgs).toEqual([]);
  });

  it('returns orgs on success', async () => {
    mockApiFetch.mockResolvedValueOnce(pagedResponse(mockOrgs));
    const { result } = renderHook(() => useMyOrgs());

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.orgs).toEqual(mockOrgs);
    expect(result.current.error).toBeNull();
    expect(mockApiFetch).toHaveBeenCalledWith(
      '/orgs/mine?page=1&pageSize=20',
      expect.objectContaining({ method: 'GET' }),
      expect.anything(),
    );
  });

  it('sets error on failure', async () => {
    const error = new Error('Failed');
    mockApiFetch.mockRejectedValueOnce(error);
    const { result } = renderHook(() => useMyOrgs());

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.orgs).toEqual([]);
    expect(result.current.error).toBe(error);
  });

  it('refetch reloads data', async () => {
    mockApiFetch.mockResolvedValueOnce(pagedResponse(mockOrgs));
    const { result } = renderHook(() => useMyOrgs());

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.orgs).toEqual(mockOrgs);

    const updatedOrgs = [mockOrgs[0]];
    mockApiFetch.mockResolvedValueOnce(pagedResponse(updatedOrgs));

    await act(async () => {
      await result.current.refetch();
    });

    expect(result.current.orgs).toEqual(updatedOrgs);
    expect(mockApiFetch).toHaveBeenCalledTimes(2);
  });
});

// ── useMembers ─────────────────────────────────────────────

describe('useMembers', () => {
  it('starts in loading state', () => {
    mockApiFetch.mockReturnValue(new Promise(() => {}));
    const { result } = renderHook(() => useMembers(ORG_ID));

    expect(result.current.isLoading).toBe(true);
    expect(result.current.members).toEqual([]);
  });

  it('returns members on success', async () => {
    mockApiFetch.mockResolvedValueOnce(pagedResponse(mockMembers));
    const { result } = renderHook(() => useMembers(ORG_ID));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.members).toEqual(mockMembers);
    expect(result.current.error).toBeNull();
    expect(mockApiFetch).toHaveBeenCalledWith(
      `/orgs/${ORG_ID}/members?page=1&pageSize=20`,
      expect.objectContaining({ method: 'GET' }),
      expect.anything(),
    );
  });

  it('sets error on failure', async () => {
    const error = new Error('Failed');
    mockApiFetch.mockRejectedValueOnce(error);
    const { result } = renderHook(() => useMembers(ORG_ID));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.members).toEqual([]);
    expect(result.current.error).toBe(error);
  });

  it('refetch reloads data', async () => {
    mockApiFetch.mockResolvedValueOnce(pagedResponse(mockMembers));
    const { result } = renderHook(() => useMembers(ORG_ID));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    const updatedMembers = [mockMembers[0]];
    mockApiFetch.mockResolvedValueOnce(pagedResponse(updatedMembers));

    await act(async () => {
      await result.current.refetch();
    });

    expect(result.current.members).toEqual(updatedMembers);
  });

  it('setMembers allows optimistic updates', async () => {
    mockApiFetch.mockResolvedValueOnce(pagedResponse(mockMembers));
    const { result } = renderHook(() => useMembers(ORG_ID));

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    const modified: Member[] = mockMembers.map((m) =>
      m.userId === mockMembers[1].userId ? { ...m, role: 'admin' as const } : m,
    );

    act(() => {
      result.current.setMembers(() => modified);
    });

    expect(result.current.members).toEqual(modified);
  });
});

// ── useCreateOrg ───────────────────────────────────────────

describe('useCreateOrg', () => {
  it('calls apiFetch with POST and org name', async () => {
    const response = { id: 'new-id', name: 'New Org' };
    mockApiFetch.mockResolvedValueOnce(response);

    const { result } = renderHook(() => useCreateOrg());
    const createOrg = result.current;

    const res = await createOrg('New Org');

    expect(res).toEqual(response);
    expect(mockApiFetch).toHaveBeenCalledWith(
      '/orgs',
      { method: 'POST', headers: { 'x-csrf-token': 'mock-csrf-token' }, body: JSON.stringify({ name: 'New Org' }) },
    );
  });

  it('propagates errors', async () => {
    const error = new Error('Creation failed');
    mockApiFetch.mockRejectedValueOnce(error);

    const { result } = renderHook(() => useCreateOrg());

    await expect(result.current('Bad Org')).rejects.toThrow('Creation failed');
  });
});

// ── useInviteMember ────────────────────────────────────────

describe('useInviteMember', () => {
  it('calls apiFetch with POST and invite payload', async () => {
    const response = { ok: true };
    mockApiFetch.mockResolvedValueOnce(response);

    const { result } = renderHook(() => useInviteMember(ORG_ID));
    const invite = result.current;

    const payload = { email: 'new@example.com', role: 'agent' as const };
    const res = await invite(payload);

    expect(res).toEqual(response);
    expect(mockApiFetch).toHaveBeenCalledWith(
      `/orgs/${ORG_ID}/invitations`,
      { method: 'POST', headers: { 'x-csrf-token': 'mock-csrf-token' }, body: JSON.stringify(payload) },
      expect.anything(),
    );
  });

  it('propagates errors', async () => {
    mockApiFetch.mockRejectedValueOnce(new Error('Invite failed'));

    const { result } = renderHook(() => useInviteMember(ORG_ID));

    await expect(
      result.current({ email: 'bad@example.com', role: 'agent' }),
    ).rejects.toThrow('Invite failed');
  });
});

// ── useUpdateRole ──────────────────────────────────────────

describe('useUpdateRole', () => {
  it('calls apiFetch with PUT and new role', async () => {
    mockApiFetch.mockResolvedValueOnce(undefined);

    const { result } = renderHook(() => useUpdateRole(ORG_ID));
    const userId = mockMembers[1].userId;

    await result.current(userId, 'admin');

    expect(mockApiFetch).toHaveBeenCalledWith(
      `/orgs/${ORG_ID}/members/${userId}`,
      { method: 'PUT', headers: { 'x-csrf-token': 'mock-csrf-token' }, body: JSON.stringify({ role: 'admin' }) },
    );
  });

  it('propagates errors', async () => {
    mockApiFetch.mockRejectedValueOnce(new Error('Forbidden'));

    const { result } = renderHook(() => useUpdateRole(ORG_ID));

    await expect(
      result.current(mockMembers[1].userId, 'admin'),
    ).rejects.toThrow('Forbidden');
  });
});

// ── useLeaveOrg ────────────────────────────────────────────

describe('useLeaveOrg', () => {
  it('returns { ok: true } when currentMember is null', async () => {
    const { result } = renderHook(() => useLeaveOrg(ORG_ID));

    const res = await result.current({ members: mockMembers, currentMember: null });

    expect(res).toEqual({ ok: true });
    expect(mockApiFetch).not.toHaveBeenCalled();
  });

  it('blocks when user is the last owner', async () => {
    const singleOwnerMembers: Member[] = [
      { userId: 'u1', email: 'owner@example.com', name: 'Owner', role: 'owner' },
      { userId: 'u2', email: 'member@example.com', name: 'Member', role: 'agent' },
    ];

    const { result } = renderHook(() => useLeaveOrg(ORG_ID));

    const res = await result.current({
      members: singleOwnerMembers,
      currentMember: singleOwnerMembers[0],
    });

    expect(res).toEqual({ ok: false, reason: 'LAST_OWNER' });
    expect(mockApiFetch).not.toHaveBeenCalled();
  });

  it('calls DELETE endpoint when owner is not the last one', async () => {
    mockApiFetch.mockResolvedValueOnce(undefined);
    const twoOwnerMembers: Member[] = [
      { userId: 'u1', email: 'owner1@example.com', name: 'Owner1', role: 'owner' },
      { userId: 'u2', email: 'owner2@example.com', name: 'Owner2', role: 'owner' },
    ];

    const { result } = renderHook(() => useLeaveOrg(ORG_ID));

    const res = await result.current({
      members: twoOwnerMembers,
      currentMember: twoOwnerMembers[0],
    });

    expect(res).toEqual({ ok: true });
    expect(mockApiFetch).toHaveBeenCalledWith(
      `/orgs/${ORG_ID}/members/me`,
      { method: 'DELETE', headers: { 'x-csrf-token': 'mock-csrf-token' } },
    );
  });

  it('calls DELETE endpoint for non-owner member', async () => {
    mockApiFetch.mockResolvedValueOnce(undefined);

    const { result } = renderHook(() => useLeaveOrg(ORG_ID));

    const res = await result.current({
      members: mockMembers,
      currentMember: mockMembers[1], // role: 'agent'
    });

    expect(res).toEqual({ ok: true });
    expect(mockApiFetch).toHaveBeenCalledWith(
      `/orgs/${ORG_ID}/members/me`,
      { method: 'DELETE', headers: { 'x-csrf-token': 'mock-csrf-token' } },
    );
  });

  it('calls DELETE endpoint for viewer member', async () => {
    mockApiFetch.mockResolvedValueOnce(undefined);
    const viewerMember: Member = {
      userId: 'u3',
      email: 'viewer@example.com',
      name: 'Viewer',
      role: 'viewer',
    };

    const { result } = renderHook(() => useLeaveOrg(ORG_ID));

    const res = await result.current({
      members: [...mockMembers, viewerMember],
      currentMember: viewerMember,
    });

    expect(res).toEqual({ ok: true });
    expect(mockApiFetch).toHaveBeenCalledWith(
      `/orgs/${ORG_ID}/members/me`,
      { method: 'DELETE', headers: { 'x-csrf-token': 'mock-csrf-token' } },
    );
  });

  it('propagates API errors during leave', async () => {
    mockApiFetch.mockRejectedValueOnce(new Error('Server error'));

    const { result } = renderHook(() => useLeaveOrg(ORG_ID));

    await expect(
      result.current({
        members: mockMembers,
        currentMember: mockMembers[1],
      }),
    ).rejects.toThrow('Server error');
  });
});

// ── useDeleteOrg ────────────────────────────────────────────

describe('useDeleteOrg', () => {
  it('calls DELETE endpoint for org', async () => {
    mockApiFetch.mockResolvedValueOnce(undefined);

    const { result } = renderHook(() => useDeleteOrg(ORG_ID));

    await result.current();

    expect(mockApiFetch).toHaveBeenCalledWith(
      `/orgs/${ORG_ID}`,
      { method: 'DELETE', headers: { 'x-csrf-token': 'mock-csrf-token' } },
    );
  });

  it('propagates API errors', async () => {
    mockApiFetch.mockRejectedValueOnce(new Error('Forbidden'));

    const { result } = renderHook(() => useDeleteOrg(ORG_ID));

    await expect(result.current()).rejects.toThrow('Forbidden');
  });
});

// ── useRemoveMember ────────────────────────────────────────────

describe('useRemoveMember', () => {
  it('calls DELETE endpoint for member', async () => {
    mockApiFetch.mockResolvedValueOnce(undefined);

    const { result } = renderHook(() => useRemoveMember(ORG_ID));

    await result.current('user-123');

    expect(mockApiFetch).toHaveBeenCalledWith(
      `/orgs/${ORG_ID}/members/user-123`,
      { method: 'DELETE', headers: { 'x-csrf-token': 'mock-csrf-token' } },
    );
  });

  it('propagates API errors', async () => {
    mockApiFetch.mockRejectedValueOnce(new Error('Forbidden'));

    const { result } = renderHook(() => useRemoveMember(ORG_ID));

    await expect(result.current('user-123')).rejects.toThrow('Forbidden');
  });
});
