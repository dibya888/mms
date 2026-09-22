import type { Resources } from './en';
const bn: Resources = {
  common: { cancel: 'বাতিল', save: 'সংরক্ষণ', create: 'তৈরি করুন', name: 'নাম', amount: 'পরিমাণ', note: 'নোট', currency: 'মুদ্রা', loading: 'লোড হচ্ছে…', error: 'কিছু ভুল হয়েছে', retry: 'আবার চেষ্টা করুন', logout: 'লগ আউট', confirm: 'নিশ্চিত করুন', search: 'খুঁজুন', optional: 'ঐচ্ছিক' },
  auth: { login: 'লগ ইন', register: 'অ্যাকাউন্ট তৈরি করুন', email: 'ইমেইল', password: 'পাসওয়ার্ড', displayName: 'আপনার নাম', noAccount: 'অ্যাকাউন্ট নেই? নিবন্ধন করুন', haveAccount: 'অ্যাকাউন্ট আছে? লগ ইন করুন', passwordHint: 'কমপক্ষে ১০ অক্ষর' },
  tabs: { accounts: 'টাকা', family: 'পরিবার', settings: 'সেটিংস' },
  accounts: { title: 'আমার টাকা', empty: 'এখনও কিছু নেই', new: 'নতুন অ্যাকাউন্ট', kind: 'ধরন', balance: 'ব্যালেন্স', due: 'বকেয়া', history: 'ইতিহাস', noTx: 'কোনো লেনদেন নেই', permissions: 'অ্যাক্সেস', Wallet: 'ওয়ালেট', Module: 'পারিবারিক মডিউল', Source: 'উৎস', CreditCard: 'ক্রেডিট কার্ড', rename: 'নাম পরিবর্তন' },
  tx: { new: 'নতুন লেনদেন', from: 'থেকে', to: 'প্রতি', none: 'কিছু নয়', category: 'ক্যাটাগরি', customCategory: 'নিজের ক্যাটাগরি', addCategory: 'ক্যাটাগরি যোগ করুন', Income: 'আয়', Transfer: 'স্থানান্তর', Expense: 'খরচ', CreditCardPayment: 'কার্ড পরিশোধ' },
  family: { title: 'পরিবার', create: 'পরিবার তৈরি করুন', name: 'পরিবারের নাম', findById: 'পরিবারে যোগ দিন', familyId: 'পরিবার আইডি', requestJoin: 'যোগ দেওয়ার অনুরোধ', pending: 'মালিকের অনুমোদনের অপেক্ষায়', members: 'সদস্য', requests: 'যোগদানের অনুরোধ', approve: 'গ্রহণ', reject: 'প্রত্যাখ্যান', kick: 'সরান', kickConfirm: 'এই সদস্যকে পরিবার থেকে সরাবেন?', owner: 'মালিক', yourId: 'পরিবার আইডি', notFound: 'পরিবার পাওয়া যায়নি' },
  perm: { View: 'দেখা', Take: 'নেওয়া', Deposit: 'জমা', Manage: 'পরিচালনা' },
  settings: { language: 'ভাষা' },
  cat: { food: 'খাবার', groceries: 'বাজার', dining_out: 'বাইরে খাওয়া', transport: 'যাতায়াত', fuel: 'জ্বালানি', rent: 'ভাড়া', utilities: 'ইউটিলিটি', electricity: 'বিদ্যুৎ', water: 'পানি', gas: 'গ্যাস', internet: 'ইন্টারনেট', mobile: 'মোবাইল', healthcare: 'স্বাস্থ্যসেবা', medicine: 'ওষুধ', education: 'শিক্ষা', tuition: 'টিউশন', clothing: 'পোশাক', shopping: 'কেনাকাটা', entertainment: 'বিনোদন', travel: 'ভ্রমণ', gifts: 'উপহার', charity: 'দান', insurance: 'বীমা', household: 'গৃহস্থালি', personal_care: 'ব্যক্তিগত যত্ন', fees: 'ফি', taxes: 'কর', other: 'অন্যান্য' },
};
export default bn;
