import type { Resources } from './en';
const zh: Resources = {
  common: { cancel: '取消', save: '保存', create: '创建', name: '名称', amount: '金额', note: '备注', currency: '币种', loading: '加载中…', error: '出错了', retry: '重试', logout: '退出登录', confirm: '确认', search: '搜索', optional: '可选' },
  auth: { login: '登录', register: '创建账户', email: '邮箱', password: '密码', confirmPassword: '确认密码', displayName: '你的名字', noAccount: '没有账户？注册', haveAccount: '已有账户？登录', passwordHint: '至少 10 个字符' },
  tabs: { accounts: '资金', family: '家庭', settings: '设置' },
  accounts: { title: '我的资金', empty: '暂无内容', new: '新建账户', kind: '类型', balance: '余额', due: '应还金额', history: '历史', noTx: '暂无交易', permissions: '权限', Wallet: '钱包', Module: '家庭模块', Source: '来源', CreditCard: '信用卡', rename: '重命名' },
  tx: { new: '新交易', from: '从', to: '到', none: '无', category: '类别', customCategory: '自定义类别', addCategory: '添加类别', Income: '收入', Transfer: '转账', Expense: '支出', CreditCardPayment: '信用卡还款' },
  family: { title: '家庭', create: '创建家庭', name: '家庭名称', findById: '加入家庭', familyId: '家庭 ID', requestJoin: '申请加入', pending: '等待所有者接受你的申请', members: '成员', requests: '加入申请', approve: '接受', reject: '拒绝', kick: '移除', kickConfirm: '确定将该成员移出家庭？', owner: '所有者', yourId: '家庭 ID', notFound: '未找到家庭' },
  perm: { View: '查看', Take: '取款', Deposit: '存款', Manage: '管理' },
  settings: { language: '语言' },
  cat: { food: '餐饮', groceries: '买菜', dining_out: '外出就餐', transport: '交通', fuel: '燃油', rent: '房租', utilities: '公用事业', electricity: '电费', water: '水费', gas: '燃气', internet: '网络', mobile: '手机', healthcare: '医疗', medicine: '药品', education: '教育', tuition: '学费', clothing: '服装', shopping: '购物', entertainment: '娱乐', travel: '旅行', gifts: '礼品', charity: '捐赠', insurance: '保险', household: '家居', personal_care: '个人护理', fees: '费用', taxes: '税费', other: '其他' },
};
export default zh;
