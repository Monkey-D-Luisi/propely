// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it } from 'vitest';
import {
  RoleSchema,
  InviteRoleSchema,
  OrgSchema,
  MemberSchema,
  MeSchema,
  MyOrgsResponseSchema,
  MembersResponseSchema,
  MeResponseSchema,
  CsrfResponseSchema,
  InviteRequestSchema,
  InviteResponseSchema,
  createLoginFormSchema,
  createRegisterFormSchema,
  createInviteFormSchema,
  createOrgFormSchema,
  PASSWORD_MIN_LENGTH,
} from '@/lib/schemas';

// ---------------------------------------------------------------------------
// RoleSchema
// ---------------------------------------------------------------------------
describe('RoleSchema', () => {
  it.each(['owner', 'admin', 'member', 'viewer'])('accepts "%s"', (role) => {
    expect(RoleSchema.parse(role)).toBe(role);
  });

  it('rejects an invalid role', () => {
    expect(() => RoleSchema.parse('superadmin')).toThrow();
  });
});

// ---------------------------------------------------------------------------
// InviteRoleSchema
// ---------------------------------------------------------------------------
describe('InviteRoleSchema', () => {
  it.each(['admin', 'member', 'viewer'])('accepts "%s"', (role) => {
    expect(InviteRoleSchema.parse(role)).toBe(role);
  });

  it('rejects "owner" since owners cannot be invited', () => {
    expect(() => InviteRoleSchema.parse('owner')).toThrow();
  });
});

// ---------------------------------------------------------------------------
// OrgSchema
// ---------------------------------------------------------------------------
describe('OrgSchema', () => {
  const validOrg = { id: '550e8400-e29b-41d4-a716-446655440000', name: 'Acme' };

  it('parses a valid org', () => {
    expect(OrgSchema.parse(validOrg)).toEqual(validOrg);
  });

  it('accepts an optional role', () => {
    const org = { ...validOrg, role: 'admin' };
    expect(OrgSchema.parse(org)).toEqual(org);
  });

  it('rejects a non-uuid id', () => {
    expect(() => OrgSchema.parse({ ...validOrg, id: 'not-a-uuid' })).toThrow();
  });

  it('rejects missing name', () => {
    expect(() => OrgSchema.parse({ id: validOrg.id })).toThrow();
  });
});

// ---------------------------------------------------------------------------
// MemberSchema
// ---------------------------------------------------------------------------
describe('MemberSchema', () => {
  const validMember = {
    userId: '550e8400-e29b-41d4-a716-446655440000',
    email: 'user@example.com',
    name: 'Alice',
    role: 'member',
  };

  it('parses a valid member', () => {
    expect(MemberSchema.parse(validMember)).toEqual(validMember);
  });

  it('accepts null name', () => {
    const member = { ...validMember, name: null };
    expect(MemberSchema.parse(member)).toEqual(member);
  });

  it('rejects an invalid email', () => {
    expect(() => MemberSchema.parse({ ...validMember, email: 'not-email' })).toThrow();
  });

  it('rejects a non-uuid userId', () => {
    expect(() => MemberSchema.parse({ ...validMember, userId: '123' })).toThrow();
  });
});

// ---------------------------------------------------------------------------
// MeSchema
// ---------------------------------------------------------------------------
describe('MeSchema', () => {
  const validMe = {
    id: '550e8400-e29b-41d4-a716-446655440000',
    email: 'me@example.com',
    emailVerified: false,
  };

  it('parses minimal valid input', () => {
    expect(MeSchema.parse(validMe)).toEqual({ ...validMe, isSystemAdmin: false });
  });

  it('accepts optional name and emailVerified', () => {
    const me = { ...validMe, name: 'Bob', emailVerified: true };
    expect(MeSchema.parse(me)).toEqual({ ...me, isSystemAdmin: false });
  });

  it('accepts null name', () => {
    const me = { ...validMe, name: null };
    expect(MeSchema.parse(me)).toEqual({ ...me, isSystemAdmin: false });
  });

  it('parses isSystemAdmin when provided', () => {
    const me = { ...validMe, isSystemAdmin: true };
    expect(MeSchema.parse(me)).toEqual(me);
  });
});

// ---------------------------------------------------------------------------
// Response schemas
// ---------------------------------------------------------------------------
describe('MyOrgsResponseSchema', () => {
  it('parses valid paginated response', () => {
    const data = {
      items: [{ id: '550e8400-e29b-41d4-a716-446655440000', name: 'Org1' }],
      pageNumber: 1,
      totalPages: 1,
      totalCount: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    };
    expect(MyOrgsResponseSchema.parse(data).items).toHaveLength(1);
  });

  it('parses empty items array', () => {
    const data = {
      items: [],
      pageNumber: 1,
      totalPages: 0,
      totalCount: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };
    expect(MyOrgsResponseSchema.parse(data).items).toHaveLength(0);
  });

  it('rejects missing items key', () => {
    expect(() => MyOrgsResponseSchema.parse({})).toThrow();
  });
});

describe('MembersResponseSchema', () => {
  it('parses valid paginated response', () => {
    const data = {
      items: [
        {
          userId: '550e8400-e29b-41d4-a716-446655440000',
          email: 'a@b.com',
          name: null,
          role: 'viewer',
        },
      ],
      pageNumber: 1,
      totalPages: 1,
      totalCount: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    };
    expect(MembersResponseSchema.parse(data).items).toHaveLength(1);
  });
});

describe('MeResponseSchema', () => {
  it('parses valid response', () => {
    const data = {
      user: {
        id: '550e8400-e29b-41d4-a716-446655440000',
        email: 'me@b.com',
        emailVerified: true,
      },
    };
    expect(MeResponseSchema.parse(data).user.email).toBe('me@b.com');
  });
});

describe('CsrfResponseSchema', () => {
  it('parses valid token', () => {
    expect(CsrfResponseSchema.parse({ csrfToken: 'abc123' }).csrfToken).toBe('abc123');
  });

  it('rejects empty token', () => {
    expect(() => CsrfResponseSchema.parse({ csrfToken: '' })).toThrow();
  });
});

describe('InviteRequestSchema', () => {
  it('parses valid invite request', () => {
    const data = { email: 'invite@example.com', role: 'member' };
    expect(InviteRequestSchema.parse(data)).toEqual(data);
  });

  it('rejects invalid email', () => {
    expect(() => InviteRequestSchema.parse({ email: 'bad', role: 'member' })).toThrow();
  });

  it('rejects owner role in invite', () => {
    expect(() =>
      InviteRequestSchema.parse({ email: 'a@b.com', role: 'owner' }),
    ).toThrow();
  });
});

describe('InviteResponseSchema', () => {
  it('parses response with ok', () => {
    expect(InviteResponseSchema.parse({ ok: true })).toEqual({ ok: true });
  });

  it('strips unknown fields', () => {
    const data = { ok: true, inviteUrl: 'https://example.com/invite/abc' };
    expect(InviteResponseSchema.parse(data)).toEqual({ ok: true });
  });
});

// ---------------------------------------------------------------------------
// Form schema factories
// ---------------------------------------------------------------------------
const loginMessages = {
  emailRequired: 'Email is required.',
  emailInvalid: 'Enter a valid email.',
  passwordRequired: 'Password is required.',
};

const registerMessages = {
  ...loginMessages,
  passwordMinLength: 'Password too short.',
  confirmPasswordRequired: 'Confirm password is required.',
  confirmPasswordMatch: 'Passwords do not match.',
};

const inviteMessages = {
  emailRequired: 'Email is required.',
  emailInvalid: 'Enter a valid email.',
};

const orgMessages = {
  nameRequired: 'Name is required.',
};

describe('createLoginFormSchema', () => {
  const schema = createLoginFormSchema(loginMessages);

  it('accepts valid login data', () => {
    const data = { email: 'user@example.com', password: 'secret123' };
    expect(schema.parse(data)).toEqual(data);
  });

  it('rejects empty email', () => {
    const result = schema.safeParse({ email: '', password: 'secret123' });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toBe(loginMessages.emailRequired);
    }
  });

  it('rejects invalid email format', () => {
    const result = schema.safeParse({ email: 'not-email', password: 'secret123' });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toBe(loginMessages.emailInvalid);
    }
  });

  it('rejects empty password', () => {
    const result = schema.safeParse({ email: 'user@example.com', password: '' });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toBe(loginMessages.passwordRequired);
    }
  });
});

describe('createRegisterFormSchema', () => {
  const schema = createRegisterFormSchema(registerMessages);

  it('accepts valid registration with optional name', () => {
    const data = { email: 'user@example.com', password: 'longpassword', confirmPassword: 'longpassword' };
    expect(schema.parse(data)).toEqual(data);
  });

  it('accepts registration with name', () => {
    const data = {
      name: 'Alice',
      email: 'user@example.com',
      password: 'longpassword',
      confirmPassword: 'longpassword',
    };
    expect(schema.parse(data)).toEqual(data);
  });

  it('rejects password shorter than PASSWORD_MIN_LENGTH', () => {
    const result = schema.safeParse({
      email: 'user@example.com',
      password: 'a'.repeat(PASSWORD_MIN_LENGTH - 1),
      confirmPassword: 'a'.repeat(PASSWORD_MIN_LENGTH - 1),
    });
    expect(result.success).toBe(false);
    if (!result.success) {
      const messages = result.error.issues.map((i) => i.message);
      expect(messages).toContain(registerMessages.passwordMinLength);
    }
  });

  it('accepts password of exactly PASSWORD_MIN_LENGTH', () => {
    const result = schema.safeParse({
      email: 'user@example.com',
      password: 'a'.repeat(PASSWORD_MIN_LENGTH),
      confirmPassword: 'a'.repeat(PASSWORD_MIN_LENGTH),
    });
    expect(result.success).toBe(true);
  });

  it('rejects when confirmPassword is empty', () => {
    const result = schema.safeParse({
      email: 'user@example.com',
      password: 'longpassword',
      confirmPassword: '',
    });
    expect(result.success).toBe(false);
    if (!result.success) {
      const messages = result.error.issues.map((i) => i.message);
      expect(messages).toContain(registerMessages.confirmPasswordRequired);
    }
  });

  it('rejects when password and confirmPassword do not match', () => {
    const result = schema.safeParse({
      email: 'user@example.com',
      password: 'longpassword',
      confirmPassword: 'differentpassword',
    });
    expect(result.success).toBe(false);
    if (!result.success) {
      const messages = result.error.issues.map((i) => i.message);
      expect(messages).toContain(registerMessages.confirmPasswordMatch);
    }
  });
});

describe('createInviteFormSchema', () => {
  const schema = createInviteFormSchema(inviteMessages);

  it('accepts valid invite data', () => {
    const data = { email: 'invite@example.com', role: 'member' as const };
    expect(schema.parse(data)).toEqual(data);
  });

  it('rejects empty email with custom message', () => {
    const result = schema.safeParse({ email: '', role: 'member' });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toBe(inviteMessages.emailRequired);
    }
  });

  it('rejects owner role', () => {
    expect(() =>
      schema.parse({ email: 'a@b.com', role: 'owner' }),
    ).toThrow();
  });
});

describe('createOrgFormSchema', () => {
  const schema = createOrgFormSchema(orgMessages);

  it('accepts valid org name', () => {
    expect(schema.parse({ name: 'Acme Corp' })).toEqual({ name: 'Acme Corp' });
  });

  it('trims whitespace from name', () => {
    expect(schema.parse({ name: '  Acme  ' })).toEqual({ name: 'Acme' });
  });

  it('rejects empty name', () => {
    const result = schema.safeParse({ name: '' });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toBe(orgMessages.nameRequired);
    }
  });

  it('rejects whitespace-only name', () => {
    const result = schema.safeParse({ name: '   ' });
    expect(result.success).toBe(false);
  });
});

describe('PASSWORD_MIN_LENGTH', () => {
  it('is 8', () => {
    expect(PASSWORD_MIN_LENGTH).toBe(8);
  });
});
