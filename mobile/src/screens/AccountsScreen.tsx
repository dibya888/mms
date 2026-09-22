import { useNavigation } from '@react-navigation/native';
import { useQueryClient } from '@tanstack/react-query';
import React from 'react';
import { Pressable } from 'react-native';
import { useTranslation } from 'react-i18next';
import { Body, Button, Card, ErrorText, H, Screen, Sub } from '../components/ui';
import { formatMoney } from '../format';
import { useAccounts, useCurrencies } from '../hooks/queries';

export default function AccountsScreen() {
  const { t } = useTranslation();
  const nav = useNavigation<any>();
  const qc = useQueryClient();
  const accounts = useAccounts(); const currencies = useCurrencies();
  return (
    <Screen>
      <H>{t('accounts.title')}</H>
      <Button title={t('accounts.new')} onPress={() => nav.navigate('NewAccount')} />
      <Button kind="ghost" title={t('tx.new')} onPress={() => nav.navigate('NewTransaction')} />
      {accounts.isError && <ErrorText msg={t('common.error')} />}
      {accounts.data?.length === 0 && <Sub>{t('accounts.empty')}</Sub>}
      {accounts.data?.map(a => (
        <Pressable key={a.id} accessibilityRole="button" onPress={() => nav.navigate('AccountDetail', { id: a.id })}>
          <Card>
            <Body bold>{a.name}</Body>
            <Sub>{t(`accounts.${a.kind}`)}{a.kind === 'CreditCard' ? ` · ${t('accounts.due')}` : ''}</Sub>
            {a.kind !== 'Source' && <Body>{formatMoney(a.balance, a.currencyCode, currencies.data)}</Body>}
          </Card>
        </Pressable>
      ))}
      <Button kind="ghost" title={t('common.retry')} onPress={() => qc.invalidateQueries({ queryKey: ['accounts'] })} />
    </Screen>
  );
}
