export type AccountKind = 'Wallet' | 'Module' | 'Source' | 'CreditCard';
export type MembershipStatus = 'Pending' | 'Active';
export type TransactionType = 'Income' | 'Transfer' | 'Expense' | 'CreditCardPayment';

// Flags serialised by the API as a comma-separated string, e.g. "View, Take".
export const Perm = { View: 1, Take: 2, Deposit: 4, Manage: 8 } as const;
export type PermName = keyof typeof Perm;

export interface AuthResult { userId: string; displayName: string; accessToken: string; accessExpiresAt: string; refreshToken: string }
export interface Currency { code: string; name: string; symbol: string; decimalPlaces: number }
export interface Account {
  id: string; kind: AccountKind; name: string; currencyCode: string; balance: number;
  ownerUserId: string; myPermissions: string;
}
export interface Category { id: string; key: string | null; name: string | null; isCustom: boolean }
export interface Tx {
  id: string; type: TransactionType; fromAccountId: string | null; toAccountId: string | null; categoryId: string | null;
  amount: number; currencyCode: string; note: string | null; createdByUserId: string; createdAt: string;
}
export interface Family { id: string; publicId: string; name: string; isOwner: boolean; myStatus: MembershipStatus }
export interface Member { userId: string; displayName: string; status: MembershipStatus; isOwner: boolean }
export interface PermissionEntry { userId: string; displayName: string; permissions: string }

export const has = (flags: string, p: PermName) => flags.split(',').map(s => s.trim()).includes(p);
export const toFlags = (names: PermName[]) => names.join(', ') || 'None';
