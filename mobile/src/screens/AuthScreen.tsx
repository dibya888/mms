import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, ErrorText, Field, H, Screen } from '../components/ui';
import { useAuth } from '../auth/AuthContext';
import { useAction } from '../hooks/useAction';

export default function AuthScreen() {
  const { t, i18n } = useTranslation();
  const auth = useAuth();
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [email, setEmail] = useState(''); const [password, setPassword] = useState(''); const [name, setName] = useState('');
  const submit = useAction(async () => {
    if (mode === 'login') await auth.login(email.trim(), password);
    else await auth.register(email.trim(), password, name.trim(), i18n.language);
  });
  return (
    <Screen>
      <H>{mode === 'login' ? t('auth.login') : t('auth.register')}</H>
      {mode === 'register' && <Field label={t('auth.displayName')} value={name} onChangeText={setName} maxLength={100} />}
      <Field label={t('auth.email')} value={email} onChangeText={setEmail} autoCapitalize="none" keyboardType="email-address" textContentType="emailAddress" maxLength={254} />
      <Field label={mode === 'register' ? `${t('auth.password')} (${t('auth.passwordHint')})` : t('auth.password')} value={password} onChangeText={setPassword} secureTextEntry textContentType={mode === 'login' ? 'password' : 'newPassword'} maxLength={128} />
      <ErrorText msg={submit.error} />
      <Button title={mode === 'login' ? t('auth.login') : t('auth.register')} onPress={() => submit.run()} busy={submit.busy} />
      <Button kind="ghost" title={mode === 'login' ? t('auth.noAccount') : t('auth.haveAccount')} onPress={() => setMode(mode === 'login' ? 'register' : 'login')} />
    </Screen>
  );
}
