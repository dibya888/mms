import { useCallback, useState } from 'react';
import { ApiError } from '../api/client';

export function useAction<A extends unknown[]>(fn: (...a: A) => Promise<unknown>) {
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const run = useCallback(async (...a: A) => {
    setBusy(true); setError(null);
    try { await fn(...a); return true; }
    catch (e) { setError(e instanceof ApiError ? e.message : 'Network error'); return false; }
    finally { setBusy(false); }
  }, [fn]);
  return { run, busy, error };
}
