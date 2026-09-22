import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import React from 'react';
import { useTranslation } from 'react-i18next';
import AccountDetailScreen from '../screens/AccountDetailScreen';
import AccountsScreen from '../screens/AccountsScreen';
import AuthScreen from '../screens/AuthScreen';
import FamilyScreen from '../screens/FamilyScreen';
import NewAccountScreen from '../screens/NewAccountScreen';
import NewTransactionScreen from '../screens/NewTransactionScreen';
import SettingsScreen from '../screens/SettingsScreen';
import { useAuth } from '../auth/AuthContext';

const Tabs = createBottomTabNavigator();
const Stack = createNativeStackNavigator();

function Main() {
  const { t } = useTranslation();
  return (
    <Tabs.Navigator screenOptions={{ headerTitleAlign: 'center' }}>
      <Tabs.Screen name="Accounts" component={AccountsScreen} options={{ title: t('tabs.accounts') }} />
      <Tabs.Screen name="Family" component={FamilyScreen} options={{ title: t('tabs.family') }} />
      <Tabs.Screen name="Settings" component={SettingsScreen} options={{ title: t('tabs.settings') }} />
    </Tabs.Navigator>
  );
}

export default function RootNavigator() {
  const { t } = useTranslation();
  const { signedIn, ready } = useAuth();
  if (!ready) return null;
  return (
    <Stack.Navigator>
      {signedIn ? (
        <>
          <Stack.Screen name="Main" component={Main} options={{ headerShown: false }} />
          <Stack.Screen name="NewAccount" component={NewAccountScreen} options={{ title: t('accounts.new') }} />
          <Stack.Screen name="NewTransaction" component={NewTransactionScreen as any} options={{ title: t('tx.new') }} />
          <Stack.Screen name="AccountDetail" component={AccountDetailScreen as any} options={{ title: '' }} />
        </>
      ) : (
        <Stack.Screen name="Auth" component={AuthScreen} options={{ headerShown: false }} />
      )}
    </Stack.Navigator>
  );
}
