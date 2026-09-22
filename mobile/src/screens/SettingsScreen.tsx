import React from 'react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../auth/AuthContext';
import { Body, Button, Chips, H, Screen } from '../components/ui';
import { LANGUAGES, setLanguage } from '../i18n';

export default function SettingsScreen() {
  const { t, i18n } = useTranslation();
  const { logout } = useAuth();
  return (
    <Screen>
      <H>{t('settings.language')}</H>
      <Chips value={i18n.language} onChange={c => setLanguage(c)} options={LANGUAGES.map(l => ({ value: l.code, label: l.label }))} />
      <Body> </Body>
      <Button kind="ghost" title={t('common.logout')} onPress={() => logout()} />
    </Screen>
  );
}
