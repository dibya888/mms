const en = {
  common: { cancel: 'Cancel', save: 'Save', create: 'Create', name: 'Name', amount: 'Amount', note: 'Note', currency: 'Currency', loading: 'Loading…', error: 'Something went wrong', retry: 'Retry', logout: 'Log out', confirm: 'Confirm', search: 'Search', optional: 'Optional' },
  auth: { login: 'Log in', register: 'Create account', email: 'Email', password: 'Password', confirmPassword: 'Confirm password', displayName: 'Your name', noAccount: 'No account? Register', haveAccount: 'Have an account? Log in', passwordHint: 'At least 10 characters' },
  tabs: { accounts: 'Money', family: 'Family', settings: 'Settings' },
  accounts: { title: 'My money', empty: 'Nothing here yet', new: 'New account', kind: 'Type', balance: 'Balance', due: 'Amount due', history: 'History', noTx: 'No transactions', permissions: 'Access', Wallet: 'Wallet', Module: 'Family module', Source: 'Source', CreditCard: 'Credit card', rename: 'Rename' },
  tx: { new: 'New transaction', from: 'From', to: 'To', none: 'None', category: 'Category', customCategory: 'Custom category', addCategory: 'Add category', Income: 'Income', Transfer: 'Transfer', Expense: 'Expense', CreditCardPayment: 'Card payment' },
  family: { title: 'Family', create: 'Create a family', name: 'Family name', findById: 'Join a family', familyId: 'Family ID', requestJoin: 'Request to join', pending: 'Waiting for the owner to accept your request', members: 'Members', requests: 'Join requests', approve: 'Accept', reject: 'Decline', kick: 'Remove', kickConfirm: 'Remove this member from the family?', owner: 'Owner', yourId: 'Family ID', notFound: 'Family not found' },
  perm: { View: 'View', Take: 'Take', Deposit: 'Deposit', Manage: 'Manage' },
  settings: { language: 'Language' },
  cat: { food: 'Food', groceries: 'Groceries', dining_out: 'Dining out', transport: 'Transport', fuel: 'Fuel', rent: 'Rent', utilities: 'Utilities', electricity: 'Electricity', water: 'Water', gas: 'Gas', internet: 'Internet', mobile: 'Mobile', healthcare: 'Healthcare', medicine: 'Medicine', education: 'Education', tuition: 'Tuition', clothing: 'Clothing', shopping: 'Shopping', entertainment: 'Entertainment', travel: 'Travel', gifts: 'Gifts', charity: 'Charity', insurance: 'Insurance', household: 'Household', personal_care: 'Personal care', fees: 'Fees', taxes: 'Taxes', other: 'Other' },
};
export type Resources = typeof en;
export default en;
