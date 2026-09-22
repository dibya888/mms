import { API_BASE_URL } from '../config';
import { AuthResult } from './types';
import { clearRefreshToken, loadRefreshToken, saveRefreshToken } from './tokenStore';

export class ApiError extends Error {
  constructor(public status: number, public code: string, message: string) { super(message); }
}

let accessToken: string | null = null;
let refreshing: Promise<boolean> | null = null;
let onSignedOut: () => void = () => {};
export const setSignedOutHandler = (fn: () => void) => { onSignedOut = fn; };

export async function applyAuth(r: AuthResult) {
  accessToken = r.accessToken;
  await saveRefreshToken(r.refreshToken);
}
export async function clearAuth() { accessToken = null; await clearRefreshToken(); }

async function raw(path: string, init: RequestInit, auth: boolean): Promise<Response> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json', Accept: 'application/json' };
  if (auth && accessToken) headers.Authorization = `Bearer ${accessToken}`;
  return fetch(`${API_BASE_URL}${path}`, { ...init, headers });
}

async function tryRefresh(): Promise<boolean> {
  refreshing ??= (async () => {
    try {
      const token = await loadRefreshToken();
      if (!token) return false;
      const res = await raw('/auth/refresh', { method: 'POST', body: JSON.stringify({ refreshToken: token }) }, false);
      if (!res.ok) return false;
      await applyAuth((await res.json()) as AuthResult);
      return true;
    } catch { return false; } finally { refreshing = null; }
  })();
  return refreshing;
}

export async function api<T>(path: string, method = 'GET', body?: unknown, auth = true): Promise<T> {
  const init: RequestInit = { method, body: body === undefined ? undefined : JSON.stringify(body) };
  let res = await raw(path, init, auth);
  if (res.status === 401 && auth) {
    if (await tryRefresh()) res = await raw(path, init, auth);
    else { await clearAuth(); onSignedOut(); }
  }
  if (res.status === 204 || res.status === 202) return undefined as T;
  const text = await res.text();
  const data = text ? JSON.parse(text) : undefined;
  if (!res.ok) throw new ApiError(res.status, data?.title ?? 'error', data?.detail ?? 'Request failed');
  return data as T;
}

export async function restoreSession(): Promise<boolean> { return tryRefresh(); }

export const getAccessToken = () => accessToken;
