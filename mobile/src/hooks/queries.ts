import { useQuery } from '@tanstack/react-query';
import { AccountApi, CategoryApi, CurrencyApi, FamilyApi } from '../api/endpoints';

export const useCurrencies = () => useQuery({ queryKey: ['currencies'], queryFn: CurrencyApi.list, staleTime: Infinity });
export const useAccounts = () => useQuery({ queryKey: ['accounts'], queryFn: AccountApi.list });
export const useCategories = () => useQuery({ queryKey: ['categories'], queryFn: CategoryApi.list });
export const useFamily = () => useQuery({ queryKey: ['family'], queryFn: async () => (await FamilyApi.mine()) ?? null });
