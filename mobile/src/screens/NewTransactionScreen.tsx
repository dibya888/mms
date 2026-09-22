import { useNavigation } from '@react-navigation/native';
import { useQueryClient } from '@tanstack/react-query';
import React, { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { CategoryApi, TxApi } from '../api/endpoints';
import { Account, Category, has, TransactionType } from '../api/types';
import { Body, Button, Chips, ErrorText, Field, Screen } from '../components/ui';
import { AMOUNT_RE } from '../format';
import { useAccounts, useCategories } from '../hooks/queries';
import { useAction } from '../hooks/useAction';

const TYPES: TransactionType[] = ['Income', 'Transfer', 'Expense', 'CreditCardPayment'];
const pool = (a: Account) => a.kind === 'Wallet' || a.kind === 'Module';
const canDebit = (a: Account) => (a.kind === 'Module' ? has(a.myPermissions, 'Take') : true);
const canCredit = (a: Account) => (a.kind === 'Module' ? has(a.myPermissions, 'Deposit') : true);

export default function NewTransactionScreen({ route }: { route?: { params?: { fromId?: string } } }) {
  const { t } = useTranslation();
  const nav = useNavigation();
  const qc = useQueryClient();
  const accounts = useAccounts(); const categories = useCategories();
  const [type, setType] = useState<TransactionType>('Transfer');
  const [from, setFrom] = useState<string | null>(route?.params?.fromId ?? null);
  const [to, setTo] = useState<string | null>(null);
  const [cat, setCat] = useState<string | null>(null);
  const [amount, setAmount] = useState(''); const [note, setNote] = useState(''); const [custom, setCustom] = useState('');

  const list = useMemo(() => accounts.data ?? [], [accounts.data]);
  const fromOpts = useMemo(() => list.filter(a =>
    type === 'Income' ? a.kind === 'Source' : type === 'Expense' ? (pool(a) || a.kind === 'CreditCard') && canDebit(a) : pool(a) && canDebit(a)), [list, type]);
  const toOpts = useMemo(() => list.filter(a =>
    type === 'CreditCardPayment' ? a.kind === 'CreditCard' : pool(a) && canCredit(a) && a.id !== from), [list, type, from]);
  const needsTo = type !== 'Expense';
  const fromAcc = list.find(a => a.id === from);
  const toAcc = list.find(a => a.id === to);
  const sameCurrency = !needsTo || !fromAcc || !toAcc || fromAcc.currencyCode === toAcc.currencyCode;
  const valid = AMOUNT_RE.test(amount) && Number(amount) > 0 && !!from && (!needsTo || !!to) && (type !== 'Expense' || !!cat) && sameCurrency;

  const addCat = useAction(async () => {
    const c = await CategoryApi.create(custom.trim());
    await qc.invalidateQueries({ queryKey: ['categories'] });
    setCat(c.id); setCustom('');
  });
  const submit = useAction(async () => {
    await TxApi.post({ fromAccountId: from!, toAccountId: needsTo ? to! : undefined, categoryId: type === 'Expense' ? cat! : undefined, amount: Number(amount), note: note.trim() || undefined });
    await qc.invalidateQueries({ queryKey: ['accounts'] });
    nav.goBack();
  });
  const catLabel = (c: Category) => (c.key ? t(`cat.${c.key}`) : c.name ?? '');

  return (
    <Screen>
      <Chips value={type} onChange={v => { setType(v); setFrom(null); setTo(null); setCat(null); }} options={TYPES.map(x => ({ value: x, label: t(`tx.${x}`) }))} />
      <Body bold>{t('tx.from')}</Body>
      <Chips value={from} onChange={setFrom} options={fromOpts.map(a => ({ value: a.id, label: `${a.name} (${a.currencyCode})` }))} />
      {needsTo && <>
        <Body bold>{t('tx.to')}</Body>
        <Chips value={to} onChange={setTo} options={toOpts.filter(a => !fromAcc || a.currencyCode === fromAcc.currencyCode).map(a => ({ value: a.id, label: `${a.name} (${a.currencyCode})` }))} />
      </>}
      {type === 'Expense' && <>
        <Body bold>{t('tx.category')}</Body>
        <Chips value={cat} onChange={setCat} options={(categories.data ?? []).map(c => ({ value: c.id, label: catLabel(c) }))} />
        <Field label={t('tx.customCategory')} value={custom} onChangeText={setCustom} maxLength={60} />
        <Button kind="ghost" title={t('tx.addCategory')} onPress={() => addCat.run()} busy={addCat.busy} disabled={custom.trim().length === 0} />
        <ErrorText msg={addCat.error} />
      </>}
      <Field label={t('common.amount')} value={amount} onChangeText={setAmount} keyboardType="decimal-pad" maxLength={17} />
      <Field label={`${t('common.note')} (${t('common.optional')})`} value={note} onChangeText={setNote} maxLength={500} />
      <ErrorText msg={submit.error} />
      <Button title={t('common.confirm')} onPress={() => submit.run()} busy={submit.busy} disabled={!valid} />
    </Screen>
  );
}
