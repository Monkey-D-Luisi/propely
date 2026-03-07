import { readFileSync, writeFileSync, renameSync } from "node:fs";
import { join } from "node:path";
import { homedir } from "node:os";

const OPENAI_AUTH_URL = "https://auth.openai.com/oauth/token";
const CODEX_CLIENT_ID = "app_EMoamEEZ73f0CkXaXp7hrann";

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  idToken: string;
  accountId: string;
  expiresAt: number; // unix seconds
  apiKey: string | null; // platform API key (from token exchange)
}

function log(msg: string): void {
  process.stderr.write(`[mcp-openai-oauth] ${msg}\n`);
}

function resolveAuthPath(): string {
  return (
    process.env.CODEX_AUTH_PATH || join(homedir(), ".codex", "auth.json")
  );
}

function loadTokens(authPath: string): AuthTokens {
  const raw = JSON.parse(readFileSync(authPath, "utf-8"));

  // Support Codex CLI format: { tokens: { access_token, ... }, last_refresh }
  // and flat format: { accessToken, refreshToken, ... }
  const tokens = raw.tokens ?? raw;
  const accessToken: string =
    tokens.accessToken ?? tokens.access_token;
  const refreshToken: string =
    tokens.refreshToken ?? tokens.refresh_token;
  const idToken: string =
    tokens.idToken ?? tokens.id_token ?? "";
  const accountId: string =
    tokens.accountId ?? tokens.account_id ?? "";

  // Platform API key (optional, from token exchange)
  const apiKey: string | null = raw.OPENAI_API_KEY ?? null;

  // Determine expiry
  let expiresAt: number = tokens.expiresAt ?? tokens.expires_at ?? 0;
  if (expiresAt === 0 && raw.last_refresh) {
    const lastRefresh = new Date(raw.last_refresh).getTime() / 1000;
    expiresAt = lastRefresh + 3600;
  }
  if (expiresAt > 1e12) {
    expiresAt = Math.floor(expiresAt / 1000);
  }

  if (!accessToken || !refreshToken) {
    throw new Error(
      `Invalid auth.json at ${authPath}: missing access_token or refresh_token. ` +
      `Found keys: ${JSON.stringify(Object.keys(raw))}`
    );
  }

  return { accessToken, refreshToken, idToken, accountId, expiresAt, apiKey };
}

async function refreshTokens(
  currentRefreshToken: string,
  authPath: string
): Promise<AuthTokens> {
  log("Refreshing OAuth tokens...");

  const resp = await fetch(OPENAI_AUTH_URL, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body: new URLSearchParams({
      grant_type: "refresh_token",
      refresh_token: currentRefreshToken,
      client_id: CODEX_CLIENT_ID,
    }),
  });

  if (!resp.ok) {
    const body = await resp.text();
    throw new Error(`Token refresh failed (${resp.status}): ${body}`);
  }

  const data = await resp.json();

  // Re-read existing file to get accountId and other fields
  const existing = JSON.parse(readFileSync(authPath, "utf-8"));
  const tokens = existing.tokens ?? existing;

  const newTokens: AuthTokens = {
    accessToken: data.access_token,
    refreshToken: data.refresh_token ?? currentRefreshToken,
    idToken: data.id_token ?? tokens.id_token ?? tokens.idToken ?? "",
    accountId: tokens.account_id ?? tokens.accountId ?? "",
    expiresAt: Math.floor(Date.now() / 1000) + (data.expires_in ?? 3600),
    apiKey: existing.OPENAI_API_KEY ?? null,
  };

  // Persist (preserve original format) — atomic write via temp+rename
  try {
    if (existing.tokens) {
      existing.tokens.access_token = newTokens.accessToken;
      existing.tokens.refresh_token = newTokens.refreshToken;
      if (newTokens.idToken) existing.tokens.id_token = newTokens.idToken;
      existing.last_refresh = new Date().toISOString();
    } else {
      existing.access_token = newTokens.accessToken;
      existing.refresh_token = newTokens.refreshToken;
      existing.expires_at = newTokens.expiresAt;
    }
    const tmpPath = authPath + ".tmp";
    writeFileSync(tmpPath, JSON.stringify(existing, null, 2), "utf-8");
    renameSync(tmpPath, authPath);
    log("Tokens refreshed and saved to auth.json");
  } catch {
    log("Warning: could not persist refreshed tokens to auth.json");
  }

  return newTokens;
}

/**
 * Attempt token exchange: trade id_token for a platform API key.
 * This allows calling api.openai.com/v1/* instead of chatgpt.com backend.
 */
async function tryTokenExchange(idToken: string): Promise<string | null> {
  if (!idToken) return null;

  log("Attempting token exchange for API key...");
  try {
    const resp = await fetch(OPENAI_AUTH_URL, {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: new URLSearchParams({
        grant_type: "urn:ietf:params:oauth:grant-type:token-exchange",
        client_id: CODEX_CLIENT_ID,
        requested_token: "openai-api-key",
        subject_token: idToken,
        subject_token_type: "urn:ietf:params:oauth:token-type:id_token",
      }),
    });

    if (!resp.ok) {
      log(`Token exchange failed (${resp.status}), will use ChatGPT backend`);
      return null;
    }

    const data = await resp.json();
    const apiKey = data.access_token ?? data.api_key ?? null;
    if (apiKey) {
      log("Token exchange succeeded — got platform API key");
    }
    return apiKey;
  } catch (err) {
    log(`Token exchange error: ${err}`);
    return null;
  }
}

let cachedTokens: AuthTokens | null = null;
let exchangedApiKey: string | null = null;
let exchangeAttempted = false;

export async function getAuth(): Promise<{
  accessToken: string;
  accountId: string;
  apiKey: string | null;
}> {
  const authPath = resolveAuthPath();

  if (!cachedTokens) {
    cachedTokens = loadTokens(authPath);
  }

  const nowSec = Math.floor(Date.now() / 1000);
  const bufferSec = 60;

  if (cachedTokens.expiresAt > 0 && nowSec >= cachedTokens.expiresAt - bufferSec) {
    cachedTokens = await refreshTokens(cachedTokens.refreshToken, authPath);
    exchangeAttempted = false; // retry exchange after refresh
  }

  // Try token exchange once to get a platform API key
  if (!exchangeAttempted && !cachedTokens.apiKey) {
    exchangeAttempted = true;
    exchangedApiKey = await tryTokenExchange(cachedTokens.idToken);
  }

  return {
    accessToken: cachedTokens.accessToken,
    accountId: cachedTokens.accountId,
    apiKey: cachedTokens.apiKey ?? exchangedApiKey,
  };
}

export function clearTokenCache(): void {
  cachedTokens = null;
  exchangeAttempted = false;
  exchangedApiKey = null;
}

/**
 * Force a token refresh regardless of expiry status.
 * Use on 401 when the token may have been revoked or expiresAt is unknown.
 */
export async function forceRefresh(): Promise<void> {
  const authPath = resolveAuthPath();
  if (!cachedTokens) {
    cachedTokens = loadTokens(authPath);
  }
  cachedTokens = await refreshTokens(cachedTokens.refreshToken, authPath);
  exchangeAttempted = false;
  exchangedApiKey = null;
}
