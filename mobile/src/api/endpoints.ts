import { api } from './client';
import { Account, AccountKind, AuthResult, Category, Currency, Family, Member, PermissionEntry, Tx } from './types';

export const AuthApi = {
  register: (email: string, password: string, displayName: string, languageCode: string) =>
    api<AuthResult>('/auth/register', 'POST', { email, password, displayName, languageCode }, false),
  login: (email: string, password: string) => api<AuthResult>('/auth/login', 'POST', { email, password }, false),
  logout: (refreshToken: string) => api<void>('/auth/logout', 'POST', { refreshToken }, false),
};
export const CurrencyApi = { list: () => api<Currency[]>('/currencies') };
export const CategoryApi = {
  list: () => api<Category[]>('/categories'),
  create: (name: string) => api<Category>('/categories', 'POST', { name }),
};
export const AccountApi = {
  list: () => api<Account[]>('/accounts'),
  create: (kind: AccountKind, name: string, currencyCode: string) => api<Account>('/accounts', 'POST', { kind, name, currencyCode }),
  rename: (id: string, name: string) => api<Account>(`/accounts/${id}/name`, 'PUT', { name }),
  history: (id: string, page = 1) => api<Tx[]>(`/accounts/${id}/transactions?page=${page}&pageSize=50`),
  permissions: (id: string) => api<PermissionEntry[]>(`/accounts/${id}/permissions`),
  setPermissions: (id: string, userId: string, permissions: string) => api<void>(`/accounts/${id}/permissions/${userId}`, 'PUT', { permissions }),
};
export const TxApi = {
  post: (b: { fromAccountId?: string; toAccountId?: string; categoryId?: string; amount: number; note?: string }) => api<Tx>('/transactions', 'POST', b),
};
export const FamilyApi = {
  mine: () => api<Family | undefined>('/families/me'),
  create: (name: string) => api<Family>('/families', 'POST', { name }),
  search: (publicId: string) => api<{ publicId: string; name: string }>(`/families/search/${encodeURIComponent(publicId)}`),
  join: (publicId: string) => api<void>(`/families/join/${encodeURIComponent(publicId)}`, 'POST'),
  members: () => api<Member[]>('/families/me/members'),
  approve: (userId: string) => api<void>(`/families/me/requests/${userId}/approve`, 'POST'),
  reject: (userId: string) => api<void>(`/families/me/requests/${userId}/reject`, 'POST'),
  kick: (userId: string) => api<void>(`/families/me/members/${userId}`, 'DELETE'),
};
