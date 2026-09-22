import * as Keychain from 'react-native-keychain';

const SERVICE = 'moneyapp.refresh';

// Refresh token lives in the OS keystore (Keychain / Keystore); access token stays in memory only.
export async function saveRefreshToken(token: string) {
  await Keychain.setGenericPassword('refresh', token, { service: SERVICE, accessible: Keychain.ACCESSIBLE.WHEN_UNLOCKED_THIS_DEVICE_ONLY });
}
export async function loadRefreshToken(): Promise<string | null> {
  const c = await Keychain.getGenericPassword({ service: SERVICE });
  return c ? c.password : null;
}
export async function clearRefreshToken() {
  await Keychain.resetGenericPassword({ service: SERVICE });
}
