# Architecture
- backend/: .NET 10 (LTS) clean architecture — Domain / Application / Infrastructure / Api. EF Core 10 + Npgsql (all queries parameterised).
- mobile/: React Native + TypeScript (i18next for localisation).
- Secrets (ConnectionStrings__Default, Jwt__SigningKey) come from environment variables only.
