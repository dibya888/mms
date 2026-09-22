# Why LedgerService has no unit tests here

`LedgerService.PostAsync` locks account rows with
`SELECT ... FOR UPDATE` via `FromSqlInterpolated`, which EF Core's
InMemory provider (used by the other tests in this project) does not
support — it throws at runtime. That row lock is exactly what stops a
double-spend under concurrent requests, so it should not be mocked
away.

Test it against a real Postgres instance instead, for example with
Testcontainers:

```bash
dotnet add tests/MoneyApp.Tests package Testcontainers.PostgreSql
```

Cases worth covering there:
- Income / Transfer / Expense / CreditCardPayment each update the
  right side(s) by the right amount.
- Transfer and expense both reject an amount greater than the
  balance.
- Two concurrent transfers that would together overdraw one account:
  the second must fail, not silently overdraw.
- Expense against a `Wallet`/`Module` requires `Take`; deposit into a
  `Module` requires `Deposit`; both are rejected with `Forbidden`
  otherwise (mirrors `Accounts/AccessPolicyTests.cs`).
- Cross-currency transfer is rejected.
