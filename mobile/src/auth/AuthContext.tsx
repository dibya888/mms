import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { applyAuth, clearAuth, getAccessToken, restoreSession, setSignedOutHandler } from '../api/client';
import { AuthApi } from '../api/endpoints';
import { loadRefreshToken } from '../api/tokenStore';
import { AuthResult } from '../api/types';
import { useQueryClient } from '@tanstack/react-query';

interface AuthState {
  ready: boolean; signedIn: boolean; userId: string | null;
  login(email: string, password: string): Promise<void>;
  register(email: string, password: string, name: string, lang: string): Promise<void>;
  logout(): Promise<void>;
}
const Ctx = createContext<AuthState>(null!);
export const useAuth = () => useContext(Ctx);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [ready, setReady] = useState(false);
  const [userId, setUserId] = useState<string | null>(null);
  const qc = useQueryClient();

  const signOutLocal = useCallback(() => { setUserId(null); qc.clear(); }, [qc]);
  useEffect(() => { setSignedOutHandler(signOutLocal); }, [signOutLocal]);
  useEffect(() => {
    (async () => {
      const ok = await restoreSession();
      if (ok) setUserId(parseSub(getAccessToken()));
      setReady(true);
    })();
  }, []);

  const finish = async (r: AuthResult) => { await applyAuth(r); setUserId(r.userId); };
  const value = useMemo<AuthState>(() => ({
    ready, signedIn: userId !== null, userId,
    login: async (e, p) => finish(await AuthApi.login(e, p)),
    register: async (e, p, n, l) => finish(await AuthApi.register(e, p, n, l)),
    logout: async () => {
      const t = await loadRefreshToken();
      if (t) { try { await AuthApi.logout(t); } catch { /* best effort */ } }
      await clearAuth(); signOutLocal();
    },
  }), [ready, userId, signOutLocal]);
  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}

// After a silent refresh the user id comes from the JWT subject (GUID, ASCII).
function parseSub(jwt: string | null): string | null {
  if (!jwt) return null;
  try {
    const payload = jwt.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse((globalThis as any).atob(payload)).sub ?? null;
  } catch { return null; }
}
