import AsyncStorage from '@react-native-async-storage/async-storage';
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import { getLocales } from 'react-native-localize';
import bn from './locales/bn';
import en from './locales/en';
import es from './locales/es';
import fr from './locales/fr';
import hi from './locales/hi';
import zh from './locales/zh';

// Add a language: create locales/<code>.ts (typed against en) and one line here.
export const LANGUAGES: { code: string; label: string }[] = [
  { code: 'en', label: 'English' }, { code: 'bn', label: 'বাংলা' }, { code: 'hi', label: 'हिन्दी' },
  { code: 'es', label: 'Español' }, { code: 'fr', label: 'Français' }, { code: 'zh', label: '中文' },
];
const resources = { en, bn, hi, es, fr, zh };
const KEY = 'moneyapp.lang';

export async function initI18n() {
  const saved = await AsyncStorage.getItem(KEY);
  const device = getLocales()[0]?.languageCode;
  const lng = [saved, device].find(c => c && c in resources) ?? 'en';
  await i18n.use(initReactI18next).init({
    resources: Object.fromEntries(Object.entries(resources).map(([k, v]) => [k, { translation: v }])),
    lng, fallbackLng: 'en', interpolation: { escapeValue: false }, returnNull: false,
  });
}
export async function setLanguage(code: string) { await AsyncStorage.setItem(KEY, code); await i18n.changeLanguage(code); }
export default i18n;
