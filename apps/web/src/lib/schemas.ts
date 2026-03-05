// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { z } from "zod";

export const RoleSchema = z.enum(["owner", "admin", "agent", "viewer"]);
export type Role = z.infer<typeof RoleSchema>;

export const InviteRoleSchema = z.enum(["admin", "agent", "viewer"]);
export type InviteRole = z.infer<typeof InviteRoleSchema>;

export const OrgSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  description: z.string().nullable().optional(),
  role: RoleSchema.optional(),
});
export type Org = z.infer<typeof OrgSchema>;

export const MemberSchema = z.object({
  userId: z.string().uuid(),
  email: z.string().email(),
  name: z.string().nullable(),
  role: RoleSchema,
});
export type Member = z.infer<typeof MemberSchema>;

export const MeSchema = z.object({
  id: z.string().uuid(),
  email: z.string().email(),
  name: z.string().nullable().optional(),
  emailVerified: z.boolean(),
  isSystemAdmin: z.boolean().optional().default(false),
});
export type Me = z.infer<typeof MeSchema>;

export const MeResponseSchema = z.object({ user: MeSchema });

export function pagedResponseSchema<T extends z.ZodTypeAny>(itemSchema: T) {
  return z.object({
    items: z.array(itemSchema),
    pageNumber: z.number(),
    totalPages: z.number(),
    totalCount: z.number(),
    hasPreviousPage: z.boolean(),
    hasNextPage: z.boolean(),
  });
}
export type PagedResponse<T> = {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
};

export const MyOrgsResponseSchema = pagedResponseSchema(OrgSchema);
export const MembersResponseSchema = pagedResponseSchema(MemberSchema);

export const CsrfResponseSchema = z.object({
  csrfToken: z.string().min(1),
});
export type CsrfResponse = z.infer<typeof CsrfResponseSchema>;

export const InviteRequestSchema = z.object({
  email: z.string().email(),
  role: InviteRoleSchema,
});
export type InviteRequest = z.infer<typeof InviteRequestSchema>;

export const InviteResponseSchema = z.object({
  ok: z.boolean(),
});

// Auth form schemas — factory functions accept translated messages
export type LoginValidationMessages = {
  emailRequired: string;
  emailInvalid: string;
  passwordRequired: string;
};

export type RegisterValidationMessages = LoginValidationMessages & {
  passwordMinLength: string;
  confirmPasswordRequired: string;
  confirmPasswordMatch: string;
};

export type ForgotPasswordValidationMessages = {
  emailRequired: string;
  emailInvalid: string;
};

export type ResetPasswordValidationMessages = {
  newPasswordRequired: string;
  newPasswordMinLength: string;
  confirmPasswordRequired: string;
  confirmPasswordMatch: string;
};

export function createLoginFormSchema(messages: LoginValidationMessages) {
  return z.object({
    email: z.string().min(1, messages.emailRequired).email(messages.emailInvalid),
    password: z.string().min(1, messages.passwordRequired),
  });
}
export type LoginFormData = z.infer<ReturnType<typeof createLoginFormSchema>>;

export const PASSWORD_MIN_LENGTH = 8;

export function createRegisterFormSchema(messages: RegisterValidationMessages) {
  return z
    .object({
      name: z.string().optional(),
      email: z.string().min(1, messages.emailRequired).email(messages.emailInvalid),
      password: z.string().min(1, messages.passwordRequired).min(PASSWORD_MIN_LENGTH, messages.passwordMinLength),
      confirmPassword: z.string().min(1, messages.confirmPasswordRequired),
    })
    .refine((data) => data.password === data.confirmPassword, {
      path: ['confirmPassword'],
      message: messages.confirmPasswordMatch,
    });
}
export type RegisterFormData = z.infer<ReturnType<typeof createRegisterFormSchema>>;

export function createForgotPasswordFormSchema(messages: ForgotPasswordValidationMessages) {
  return z.object({
    email: z.string().min(1, messages.emailRequired).email(messages.emailInvalid),
  });
}
export type ForgotPasswordFormData = z.infer<ReturnType<typeof createForgotPasswordFormSchema>>;

export function createResetPasswordFormSchema(messages: ResetPasswordValidationMessages) {
  return z
    .object({
      newPassword: z
        .string()
        .min(1, messages.newPasswordRequired)
        .min(PASSWORD_MIN_LENGTH, messages.newPasswordMinLength),
      confirmPassword: z.string().min(1, messages.confirmPasswordRequired),
    })
    .refine((data) => data.newPassword === data.confirmPassword, {
      path: ['confirmPassword'],
      message: messages.confirmPasswordMatch,
    });
}
export type ResetPasswordFormData = z.infer<ReturnType<typeof createResetPasswordFormSchema>>;

// Invite form schema — factory function for i18n
export type InviteValidationMessages = {
  emailRequired: string;
  emailInvalid: string;
};

export function createInviteFormSchema(messages: InviteValidationMessages) {
  return z.object({
    email: z.string().min(1, messages.emailRequired).email(messages.emailInvalid),
    role: InviteRoleSchema,
  });
}
export type InviteFormData = z.infer<ReturnType<typeof createInviteFormSchema>>;

// Create org schema — factory function for i18n
export type CreateOrgValidationMessages = {
  nameRequired: string;
};

export function createOrgFormSchema(messages: CreateOrgValidationMessages) {
  return z.object({
    name: z.string().trim().min(1, messages.nameRequired),
  });
}
export type CreateOrgFormData = z.infer<ReturnType<typeof createOrgFormSchema>>;

// Update org schema — factory function for i18n
export type UpdateOrgValidationMessages = {
  nameRequired: string;
};

export function createUpdateOrgFormSchema(messages: UpdateOrgValidationMessages) {
  return z.object({
    name: z.string().trim().min(1, messages.nameRequired),
    description: z.string().optional(),
  });
}
export type UpdateOrgFormData = z.infer<ReturnType<typeof createUpdateOrgFormSchema>>;

// Update profile schema — factory function for i18n
export type UpdateProfileValidationMessages = {
  nameMaxLength: string;
};

export function createUpdateProfileFormSchema(messages: UpdateProfileValidationMessages) {
  return z.object({
    name: z.string().max(200, messages.nameMaxLength).optional(),
  });
}
export type UpdateProfileFormData = z.infer<ReturnType<typeof createUpdateProfileFormSchema>>;

// Change password schema — factory function for i18n
export type ChangePasswordValidationMessages = {
  currentPasswordRequired: string;
  newPasswordRequired: string;
  newPasswordMinLength: string;
};

export function createChangePasswordFormSchema(messages: ChangePasswordValidationMessages) {
  return z.object({
    currentPassword: z.string().min(1, messages.currentPasswordRequired),
    newPassword: z.string().min(1, messages.newPasswordRequired).min(PASSWORD_MIN_LENGTH, messages.newPasswordMinLength),
  });
}
export type ChangePasswordFormData = z.infer<ReturnType<typeof createChangePasswordFormSchema>>;

export const OrgDetailSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  description: z.string().nullable().optional(),
  role: RoleSchema,
});
export type OrgDetail = z.infer<typeof OrgDetailSchema>;

// Notification schemas
export const NotificationSchema = z.object({
  id: z.string().uuid(),
  type: z.string(),
  title: z.string(),
  body: z.string(),
  isRead: z.boolean(),
  createdAtUtc: z.string(),
  metadata: z.string().nullable().optional(),
});
export type Notification = z.infer<typeof NotificationSchema>;

export const NotificationsResponseSchema = z.object({
  items: z.array(NotificationSchema),
  pageNumber: z.number(),
  totalPages: z.number(),
  totalCount: z.number(),
  hasPreviousPage: z.boolean(),
  hasNextPage: z.boolean(),
  unreadCount: z.number(),
});
export type NotificationsResponse = z.infer<typeof NotificationsResponseSchema>;

// Feature flag schemas
export const FeatureFlagSchema = z.object({
  name: z.string(),
  isEnabled: z.boolean(),
  description: z.string().nullable().optional(),
  source: z.string(),
});
export type FeatureFlag = z.infer<typeof FeatureFlagSchema>;

export const FeatureFlagsResponseSchema = z.object({
  flags: z.array(FeatureFlagSchema),
});
export type FeatureFlagsResponse = z.infer<typeof FeatureFlagsResponseSchema>;

export const FeatureFlagCheckResponseSchema = z.object({
  name: z.string(),
  isEnabled: z.boolean(),
  source: z.string(),
});
export type FeatureFlagCheckResponse = z.infer<typeof FeatureFlagCheckResponseSchema>;

// Billing schemas
export const PlanSchema = z.object({
  id: z.string(),
  name: z.string(),
  features: z.array(z.string()),
});
export type Plan = z.infer<typeof PlanSchema>;
export const PlansResponseSchema = z.array(PlanSchema);

export const SubscriptionSchema = z.object({
  planId: z.string(),
  planName: z.string(),
  status: z.string(),
  currentPeriodEnd: z.string().nullable(),
  features: z.array(z.string()),
});
export type Subscription = z.infer<typeof SubscriptionSchema>;

export const PaymentSchema = z.object({
  id: z.string(),
  amount: z.number(),
  currency: z.string(),
  description: z.string(),
  status: z.string(),
  createdAtUtc: z.string(),
});
export type Payment = z.infer<typeof PaymentSchema>;
export const PaymentsResponseSchema = z.array(PaymentSchema);

// Audit log schemas
export const AuditLogSchema = z.object({
  id: z.string().uuid(),
  userId: z.string().uuid().nullable(),
  organizationId: z.string().uuid().nullable(),
  action: z.string(),
  entityType: z.string(),
  entityId: z.string(),
  changes: z.string().nullable(),
  correlationId: z.string().nullable(),
  createdAtUtc: z.string(),
});
export type AuditLog = z.infer<typeof AuditLogSchema>;
export const AuditLogsResponseSchema = pagedResponseSchema(AuditLogSchema);

// Work item schemas
export const WorkItemStatusEnum = z.enum(["Pending", "Active", "Deleted", "Deactivated", "Expired"]);
export type WorkItemStatusType = z.infer<typeof WorkItemStatusEnum>;

export const WorkItemPriorityEnum = z.enum(['Low', 'Medium', 'High', 'Critical']);
export type WorkItemPriorityType = z.infer<typeof WorkItemPriorityEnum>;

export const WorkItemTypeEnum = z.enum(['Task', 'Bug', 'Feature', 'Improvement']);
export type WorkItemTypeType = z.infer<typeof WorkItemTypeEnum>;

export const WorkItemEffortEnum = z.enum(['XS', 'S', 'M', 'L', 'XL']);
export type WorkItemEffortType = z.infer<typeof WorkItemEffortEnum>;

export const WorkItemSchema = z.object({
  id: z.string().guid(),
  orgId: z.string().guid(),
  title: z.string(),
  description: z.string().nullable(),
  status: WorkItemStatusEnum,
  priority: z.string().nullable().optional(),
  type: z.string().nullable().optional(),
  dueDateUtc: z.string().nullable().optional(),
  estimatedEffort: z.string().nullable().optional(),
  createdAtUtc: z.string(),
  updatedAtUtc: z.string(),
});
export type WorkItem = z.infer<typeof WorkItemSchema>;
export const WorkItemsResponseSchema = pagedResponseSchema(WorkItemSchema);

export const ParseWorkItemResponseSchema = z.object({
  title: z.string(),
  description: z.string().nullable(),
  status: z.string(),
  priority: z.string().nullable(),
  type: z.string().nullable(),
  dueDateUtc: z.string().nullable(),
  estimatedEffort: z.string().nullable(),
  confidence: z.number(),
});
export type ParseWorkItemResponse = z.infer<typeof ParseWorkItemResponseSchema>;

export type WorkItemFormValidationMessages = {
  titleRequired: string;
  titleMaxLength: string;
};

export function createWorkItemFormSchema(messages: WorkItemFormValidationMessages) {
  return z.object({
    title: z.string().trim().min(1, messages.titleRequired).max(200, messages.titleMaxLength),
    description: z.string().max(2000).optional().or(z.literal("")),
    status: WorkItemStatusEnum,
    priority: WorkItemPriorityEnum.optional().default('Medium'),
    type: WorkItemTypeEnum.optional().default('Task'),
    dueDate: z.string().optional().default(''),
    estimatedEffort: WorkItemEffortEnum.optional().default('M'),
  });
}
export type WorkItemFormData = z.input<ReturnType<typeof createWorkItemFormSchema>>;

// Version schemas
export const VersionResponseSchema = z.object({
  version: z.string(),
  buildDate: z.string().nullable().optional(),
});
export type VersionResponse = z.infer<typeof VersionResponseSchema>;

export const UpdateCheckResponseSchema = z.object({
  enabled: z.boolean(),
  current: z.string().optional(),
  latest: z.string().nullable().optional(),
  updateAvailable: z.boolean().optional(),
  releaseUrl: z.string().nullable().optional(),
  message: z.string().optional(),
});
export type UpdateCheckResponse = z.infer<typeof UpdateCheckResponseSchema>;

// Agency schemas
export const AgencyBranchSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  memberCount: z.number(),
  createdAtUtc: z.string(),
});
export type AgencyBranch = z.infer<typeof AgencyBranchSchema>;

export const AgencySchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  slug: z.string(),
  createdAtUtc: z.string(),
  branchCount: z.number(),
});
export type Agency = z.infer<typeof AgencySchema>;

export const AgenciesResponseSchema = z.array(AgencySchema);

export const AgencyDetailSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  slug: z.string(),
  createdByUserId: z.string().uuid(),
  createdAtUtc: z.string(),
  updatedAtUtc: z.string().nullable().optional(),
  branches: z.array(AgencyBranchSchema),
});
export type AgencyDetail = z.infer<typeof AgencyDetailSchema>;

export const CreateAgencyResponseSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  slug: z.string(),
});
export type CreateAgencyResponse = z.infer<typeof CreateAgencyResponseSchema>;

// Agency form schemas — factory functions for i18n
export type CreateAgencyValidationMessages = {
  nameRequired: string;
  nameMaxLength: string;
  slugRequired: string;
  slugMinLength: string;
  slugMaxLength: string;
  slugFormat: string;
};

export function createAgencyFormSchema(messages: CreateAgencyValidationMessages) {
  return z.object({
    name: z.string().trim().min(1, messages.nameRequired).max(200, messages.nameMaxLength),
    slug: z
      .string()
      .trim()
      .min(3, messages.slugMinLength)
      .max(50, messages.slugMaxLength)
      .regex(/^[a-z0-9]+(?:-[a-z0-9]+)*$/, messages.slugFormat),
  });
}
export type CreateAgencyFormData = z.infer<ReturnType<typeof createAgencyFormSchema>>;

export type UpdateAgencyValidationMessages = {
  nameRequired: string;
  nameMaxLength: string;
};

export function createUpdateAgencyFormSchema(messages: UpdateAgencyValidationMessages) {
  return z.object({
    name: z.string().trim().min(1, messages.nameRequired).max(200, messages.nameMaxLength),
  });
}
export type UpdateAgencyFormData = z.infer<ReturnType<typeof createUpdateAgencyFormSchema>>;
