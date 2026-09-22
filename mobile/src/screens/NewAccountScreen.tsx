import { useNavigation } from '@react-navigation/native';
import { useQueryClient } from '@tanstack/react-query';
import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { AccountApi } from '../api/endpoints';
import { AccountKind } from '../api/types';
import { Body, Button, Chips, ErrorText, Field, Screen } from '../components/ui';
import { useAction } from '../hooks/useAction';
import { useCurrencies, useFamily } from '../hooks/queries';

export default function NewAccountScreen() {
  const { t } = useTranslation();
  const nav = useNavigation();
  const qc = useQueryClient();
  const currencies = useCurrencies(); const family = useFamily();
  const [kind, setKind] = useState<AccountKind>('Wallet');
  const [name, setName] = useState(''); const [cur, setCur] = useState('BDT');
  const canModule = family.data?.myStatus === 'Active';
  const kinds: AccountKind[] = ['Wallet', 'Source', 'CreditCard', ...(canModule ? (['Module'] as AccountKind[]) : [])];
  const save = useAction(async () => {
    await AccountApi.create(kind, name.trim(), cur);
    await qc.invalidateQueries({ queryKey: ['accounts'] });
    nav.goBack();
  });
  return (
    <Screen>
      <Body bold>{t('accounts.kind')}</Body>
      <Chips value={kind} onChange={setKind} options={kinds.map(k => ({ value: k, label: t(`accounts.${k}`) }))} />
      <Field label={t('common.name')} value={name} onChangeText={setName} maxLength={100} />
      <Body bold>{t('common.currency')}</Body>
      <Chips value={cur} onChange={setCur} options={(currencies.data ?? []).map(c => ({ value: c.code, label: `${c.symbol} ${c.code}` }))} />
      <ErrorText msg={save.error} />
      <Button title={t('common.create')} onPress={() => save.run()} busy={save.busy} disabled={name.trim().length === 0} />
    </Screen>
  );
}
