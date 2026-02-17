import type { NextConfig } from 'next';
import createNextIntlPlugin from 'next-intl/plugin';

// CSP headers are now set in middleware.ts with per-request nonces.
// Security headers (X-Content-Type-Options, Referrer-Policy, X-Frame-Options)
// are also set there to keep all security headers in one place.

const nextConfig: NextConfig = {
  output: 'standalone',
};

const withNextIntl = createNextIntlPlugin('./src/i18n/request.ts');
export default withNextIntl(nextConfig);
