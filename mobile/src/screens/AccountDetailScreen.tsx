import { useNavigation } from '@react-navigation/native';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { AccountApi } from '../api/endpoints';
import { has, PermName, toFlags } from '../api/types';
import { Body, Button, Card, Chips, ErrorText, H, Screen, Sub } from '../components/ui';
import { formatMoney } from '../format';
import { useAccounts, useCategories, useCurrencies } from '../hooks/queries';
import { useAction } from '../hooks/useAction';
import { View } from 'react-native';

const PERMS: PermName[] = ['View', 'Take', 'Deposit', 'Manage'];

export default function AccountDetailScreen({ route }: { route: { params: { id: string } } }) {
  const { id } = route.params;
  const { t } = useTranslation();
  const nav = useNavigation<any>();
  const qc = useQueryClient();
  const accounts = useAccounts(); const currencies = useCurrencies(); const categories = useCategories();
  const acc = accounts.data?.find(a => a.id === id);
  const history = useQuery({ queryKey: ['history', id], queryFn: () => AccountApi.history(id), enabled: !!acc });
  const isModuleOwnerish = acc?.kind === 'Module' && has(acc.myPermissions, 'Manage');
  const perms = useQuery({ queryKey: ['perms', id], queryFn: () => AccountApi.permissions(id), enabled: !!isModuleOwnerish, retry: false });
  const setPerm = useAction(async (uid: string, flags: string) => {
    await AccountApi.setPermissions(id, uid, flags);
    await qc.invalidateQueries({ queryKey: ['perms', id] });
  });
  if (!acc) return <Screen><Sub>{t('common.loading')}</Sub></Screen>;
  const money = (n: number) => formatMoney(n, acc.currencyCode, currencies.data);
  const catName = (cid: string | null) => { const c = categories.data?.find(x => x.id === cid); return c ? (c.key ? t(`cat.${c.key}`) : c.name) : ''; };

  return (
    <Screen>
      <H>{acc.name}</H>
      {acc.kind !== 'Source' && <Body bold>{money(acc.balance)}{acc.kind === 'CreditCard' ? ` (${t('accounts.due')})` : ''}</Body>}
      <Button title={t('tx.new')} onPress={() => nav.navigate('NewTransaction', { fromId: acc.id })} />

      {perms.data && (
        <View style={{ gap: 12 }}>
          <H>{t('accounts.permissions')}</H>
          <ErrorText msg={setPerm.error} />
          {perms.data.map(p => {
            const names = PERMS.filter(n => has(p.permissions, n));
            return (
              <Card key={p.userId}>
                <Body bold>{p.displayName}</Body>
                <Chips<PermName> value={null} onChange={n => setPerm.run(p.userId, toFlags(names.includes(n) ? names.filter(x => x !== n) : [...names, n]))}
                  options={PERMS.map(n => ({ value: n, label: `${names.includes(n) ? '✓ ' : ''}${t(`perm.${n}`)}` }))} />
              </Card>
            );
          })}
        </View>
      )}

      <H>{t('accounts.history')}</H>
      {history.data?.length === 0 && <Sub>{t('accounts.noTx')}</Sub>}
      {history.data?.map(x => (
        <Card key={x.id}>
          <Body bold>{t(`tx.${x.type}`)} · {money(x.amount)}</Body>
          <Sub>{new Date(x.createdAt).toLocaleString()}{x.categoryId ? ` · ${catName(x.categoryId)}` : ''}</Sub>
          {x.note ? <Sub>{x.note}</Sub> : null}
        </Card>
      ))}
    </Screen>
  );
}
