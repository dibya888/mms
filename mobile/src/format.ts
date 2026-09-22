import { Currency } from './api/types';

export function formatMoney(amount: number, code: string, currencies: Currency[] | undefined): string {
  const c = currencies?.find(x => x.code === code);
  const dp = c?.decimalPlaces ?? 2;
  const fixed = Math.abs(amount).toFixed(dp).replace(/\B(?=(\d{3})+(?!\d))/g, ',');
  return `${amount < 0 ? '-' : ''}${c?.symbol ?? code}${fixed}`;
}
export const AMOUNT_RE = /^\d{1,12}(\.\d{1,4})?$/;
