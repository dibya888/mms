# Generating the database migration

This sandbox has no .NET SDK and no network access to Microsoft's download
hosts, so the migration could not be generated or verified here. Run it
yourself once you have the .NET 10 SDK installed:

```bash
cd backend
dotnet tool install --global dotnet-ef --version 10.*
export PATH="$PATH:$HOME/.dotnet/tools"

dotnet ef migrations add InitialCreate \
  --project src/MoneyApp.Infrastructure \
  --startup-project src/MoneyApp.Api

# Apply it to a running Postgres instance:
export ConnectionStrings__Default="Host=localhost;Database=moneyapp;Username=moneyapp;Password=<dev-only>"
export Jwt__SigningKey="$(openssl rand -base64 48)"
dotnet ef database update \
  --project src/MoneyApp.Infrastructure \
  --startup-project src/MoneyApp.Api
```

First, `dotnet build` the solution and fix anything the compiler flags —
this code was written without a compiler available, so treat the first
build as a review pass, not a formality.

`ConnectionStrings__Default` and `Jwt__SigningKey` must come from the
environment (or a secret manager) in every environment, never from
`appsettings.json`. `Jwt__SigningKey` must decode to at least 32 bytes;
the command above generates one.
