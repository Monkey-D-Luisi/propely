// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import createIntlMiddleware from 'next-intl/middleware';
import { routing } from './i18n/routing';
import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

const intlMiddleware = createIntlMiddleware(routing);

export default function proxy(request: NextRequest) {
  // Let intl middleware handle locale redirects first
  const intlResponse = intlMiddleware(request);

  // If intl middleware triggers a redirect, return it directly
  if (intlResponse.headers.get('location')) {
    return intlResponse;
  }

  // Generate a per-request nonce for CSP (Edge-compatible, no Buffer)
  const nonce = btoa(crypto.randomUUID());
  const orgsApiUrl = process.env.NEXT_PUBLIC_ORGS_API_URL ?? 'http://localhost:5020';
  const aiApiUrl = process.env.NEXT_PUBLIC_AI_API_URL ?? 'http://localhost:5010';

  const cspHeader = [
    `default-src 'self'`,
    `script-src 'self' 'nonce-${nonce}' 'strict-dynamic'`,
    `style-src 'self' 'unsafe-inline' https://fonts.googleapis.com`,
    `img-src 'self' data: blob:`,
    `font-src 'self' https://fonts.gstatic.com`,
    `connect-src 'self' ${orgsApiUrl} ${aiApiUrl}`,
    `frame-src 'self' https://checkout.stripe.com https://billing.stripe.com`,
    `object-src 'none'`,
    `base-uri 'self'`,
    `form-action 'self'`,
    `frame-ancestors 'none'`,
  ].join('; ');

  // Pass nonce to server components via cloned request headers
  const requestHeaders = new Headers(request.headers);
  requestHeaders.set('x-nonce', nonce);

  const response = NextResponse.next({
    request: { headers: requestHeaders },
  });

  // Preserve cookies set by intl middleware (e.g. NEXT_LOCALE)
  for (const cookie of intlResponse.cookies.getAll()) {
    response.cookies.set(cookie);
  }

  // Set security headers on the response
  response.headers.set('Content-Security-Policy', cspHeader);
  response.headers.set('X-Content-Type-Options', 'nosniff');
  response.headers.set('Referrer-Policy', 'strict-origin-when-cross-origin');
  response.headers.set('X-Frame-Options', 'DENY');

  return response;
}

export const config = {
  matcher: '/((?!api|trpc|_next|_vercel|.*\\..*).*)',
};
