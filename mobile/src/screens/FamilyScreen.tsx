import { useQuery, useQueryClient } from '@tanstack/react-query';
import React, { useState } from 'react';
import { Alert, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { FamilyApi } from '../api/endpoints';
import { Body, Button, Card, ErrorText, Field, H, Screen, Sub } from '../components/ui';
import { useFamily } from '../hooks/queries';
import { useAction } from '../hooks/useAction';

export default function FamilyScreen() {
  const { t } = useTranslation();
  const qc = useQueryClient();
  const family = useFamily();
  const refresh = () => qc.invalidateQueries({ queryKey: ['family'] });
  const [name, setName] = useState(''); const [fid, setFid] = useState('');
  const [found, setFound] = useState<{ publicId: string; name: string } | null>(null);

  const create = useAction(async () => { await FamilyApi.create(name.trim()); await refresh(); });
  const search = useAction(async () => { setFound(null); setFound(await FamilyApi.search(fid.trim())); });
  const join = useAction(async () => { await FamilyApi.join(found!.publicId); setFound(null); await refresh(); });

  const f = family.data;
  const active = f?.myStatus === 'Active';
  const members = useQuery({ queryKey: ['members'], queryFn: FamilyApi.members, enabled: active });
  const act = useAction(async (fn: () => Promise<unknown>) => { await fn(); await qc.invalidateQueries({ queryKey: ['members'] }); await qc.invalidateQueries({ queryKey: ['accounts'] }); });
  const confirmKick = (uid: string) => Alert.alert(t('family.kick'), t('family.kickConfirm'), [
    { text: t('common.cancel'), style: 'cancel' },
    { text: t('family.kick'), style: 'destructive', onPress: () => act.run(() => FamilyApi.kick(uid)) },
  ]);

  if (family.isLoading) return <Screen><Sub>{t('common.loading')}</Sub></Screen>;

  if (!f) return (
    <Screen>
      <H>{t('family.create')}</H>
      <Field label={t('family.name')} value={name} onChangeText={setName} maxLength={100} />
      <ErrorText msg={create.error} />
      <Button title={t('common.create')} onPress={() => create.run()} busy={create.busy} disabled={name.trim().length === 0} />
      <H>{t('family.findById')}</H>
      <Field label={t('family.familyId')} value={fid} onChangeText={setFid} autoCapitalize="characters" maxLength={10} />
      <ErrorText msg={search.error ?? join.error} />
      <Button kind="ghost" title={t('common.search')} onPress={() => search.run()} busy={search.busy} disabled={fid.trim().length < 6} />
      {found && <Card><Body bold>{found.name}</Body><Button title={t('family.requestJoin')} onPress={() => join.run()} busy={join.busy} /></Card>}
    </Screen>
  );

  if (!active) return <Screen><H>{f.name}</H><Sub>{t('family.pending')}</Sub></Screen>;

  const pending = members.data?.filter(m => m.status === 'Pending') ?? [];
  const current = members.data?.filter(m => m.status === 'Active') ?? [];
  return (
    <Screen>
      <H>{f.name}</H>
      <Sub>{t('family.yourId')}: {f.publicId}</Sub>
      <ErrorText msg={act.error} />
      {f.isOwner && pending.length > 0 && (
        <View style={{ gap: 12 }}>
          <H>{t('family.requests')}</H>
          {pending.map(m => (
            <Card key={m.userId}>
              <Body bold>{m.displayName}</Body>
              <Button title={t('family.approve')} onPress={() => act.run(() => FamilyApi.approve(m.userId))} />
              <Button kind="ghost" title={t('family.reject')} onPress={() => act.run(() => FamilyApi.reject(m.userId))} />
            </Card>
          ))}
        </View>
      )}
      <H>{t('family.members')}</H>
      {current.map(m => (
        <Card key={m.userId}>
          <Body bold>{m.displayName}{m.isOwner ? ` · ${t('family.owner')}` : ''}</Body>
          {f.isOwner && !m.isOwner && <Button kind="danger" title={t('family.kick')} onPress={() => confirmKick(m.userId)} />}
        </Card>
      ))}
    </Screen>
  );
}
