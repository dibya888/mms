import type { Resources } from './en';
const hi: Resources = {
  common: { cancel: 'रद्द करें', save: 'सहेजें', create: 'बनाएँ', name: 'नाम', amount: 'राशि', note: 'नोट', currency: 'मुद्रा', loading: 'लोड हो रहा है…', error: 'कुछ गलत हो गया', retry: 'पुनः प्रयास करें', logout: 'लॉग आउट', confirm: 'पुष्टि करें', search: 'खोजें', optional: 'वैकल्पिक' },
  auth: { login: 'लॉग इन', register: 'खाता बनाएँ', email: 'ईमेल', password: 'पासवर्ड', displayName: 'आपका नाम', noAccount: 'खाता नहीं है? पंजीकरण करें', haveAccount: 'खाता है? लॉग इन करें', passwordHint: 'कम से कम 10 अक्षर' },
  tabs: { accounts: 'पैसा', family: 'परिवार', settings: 'सेटिंग्स' },
  accounts: { title: 'मेरा पैसा', empty: 'अभी कुछ नहीं है', new: 'नया खाता', kind: 'प्रकार', balance: 'शेष', due: 'देय राशि', history: 'इतिहास', noTx: 'कोई लेनदेन नहीं', permissions: 'पहुँच', Wallet: 'वॉलेट', Module: 'पारिवारिक मॉड्यूल', Source: 'स्रोत', CreditCard: 'क्रेडिट कार्ड', rename: 'नाम बदलें' },
  tx: { new: 'नया लेनदेन', from: 'से', to: 'को', none: 'कोई नहीं', category: 'श्रेणी', customCategory: 'अपनी श्रेणी', addCategory: 'श्रेणी जोड़ें', Income: 'आय', Transfer: 'हस्तांतरण', Expense: 'खर्च', CreditCardPayment: 'कार्ड भुगतान' },
  family: { title: 'परिवार', create: 'परिवार बनाएँ', name: 'परिवार का नाम', findById: 'परिवार से जुड़ें', familyId: 'परिवार आईडी', requestJoin: 'जुड़ने का अनुरोध', pending: 'मालिक की स्वीकृति की प्रतीक्षा है', members: 'सदस्य', requests: 'जुड़ने के अनुरोध', approve: 'स्वीकार करें', reject: 'अस्वीकार करें', kick: 'हटाएँ', kickConfirm: 'इस सदस्य को परिवार से हटाएँ?', owner: 'मालिक', yourId: 'परिवार आईडी', notFound: 'परिवार नहीं मिला' },
  perm: { View: 'देखना', Take: 'निकालना', Deposit: 'जमा करना', Manage: 'प्रबंधन' },
  settings: { language: 'भाषा' },
  cat: { food: 'भोजन', groceries: 'किराना', dining_out: 'बाहर खाना', transport: 'परिवहन', fuel: 'ईंधन', rent: 'किराया', utilities: 'यूटिलिटी', electricity: 'बिजली', water: 'पानी', gas: 'गैस', internet: 'इंटरनेट', mobile: 'मोबाइल', healthcare: 'स्वास्थ्य सेवा', medicine: 'दवा', education: 'शिक्षा', tuition: 'ट्यूशन', clothing: 'कपड़े', shopping: 'खरीदारी', entertainment: 'मनोरंजन', travel: 'यात्रा', gifts: 'उपहार', charity: 'दान', insurance: 'बीमा', household: 'घरेलू', personal_care: 'व्यक्तिगत देखभाल', fees: 'शुल्क', taxes: 'कर', other: 'अन्य' },
};
export default hi;
