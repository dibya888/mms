using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyApp.Application.Accounts;
using MoneyApp.Application.Auth;
using MoneyApp.Application.Categories;
using MoneyApp.Application.Common;
using MoneyApp.Application.Families;
using MoneyApp.Application.Ledger;
using MoneyApp.Infrastructure.Persistence;
using MoneyApp.Infrastructure.Security;

namespace MoneyApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMoneyApp(this IServiceCollection s, IConfiguration cfg)
    {
        s.AddDbContext<AppDbContext>(o => o.UseNpgsql(cfg.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required."))
            .UseSnakeCaseNamingConvention());
        s.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        s.AddOptions<JwtOptions>().Bind(cfg.GetSection(JwtOptions.Section))
            .Validate(o => Encoding.UTF8.GetByteCount(o.SigningKey) >= 32 && o.Issuer != "" && o.Audience != "", "Jwt config invalid (key >= 32 bytes)")
            .ValidateOnStart();
        s.AddSingleton<IPasswordService, PasswordService>();
        s.AddSingleton<ITokenService, TokenService>();
        s.AddScoped<AccessPolicy>();
        s.AddScoped<AuthService>(); s.AddScoped<FamilyService>(); s.AddScoped<AccountService>();
        s.AddScoped<LedgerService>(); s.AddScoped<CategoryService>();
        return s;
    }
}
