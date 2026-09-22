using Microsoft.EntityFrameworkCore;
using MoneyApp.Application.Common;
using MoneyApp.Domain;

namespace MoneyApp.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountModuleAccess> ModulePermissions => Set<AccountModuleAccess>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<LedgerTransaction> Transactions => Set<LedgerTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users"); e.HasKey(x => x.Id);
            e.HasIndex(x => x.NormalizedEmail).IsUnique();
            e.Property(x => x.Email).HasMaxLength(254); e.Property(x => x.NormalizedEmail).HasMaxLength(254);
            e.Property(x => x.DisplayName).HasMaxLength(100); e.Property(x => x.LanguageCode).HasMaxLength(10);
        });
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens"); e.HasKey(x => x.Id);
            e.HasIndex(x => x.TokenHash).IsUnique(); e.HasIndex(x => x.FamilyId);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Currency>(e =>
        {
            e.ToTable("currencies"); e.HasKey(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(3); e.Property(x => x.Name).HasMaxLength(100); e.Property(x => x.Symbol).HasMaxLength(10);
            e.HasData(CurrencySeed.All);
        });
        modelBuilder.Entity<Family>(e =>
        {
            e.ToTable("families"); e.HasKey(x => x.Id);
            e.HasIndex(x => x.PublicId).IsUnique();
            e.Property(x => x.PublicId).HasMaxLength(10); e.Property(x => x.Name).HasMaxLength(100);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<FamilyMember>(e =>
        {
            e.ToTable("family_members"); e.HasKey(x => new { x.FamilyId, x.UserId });
            e.HasIndex(x => x.UserId).IsUnique(); // one family (or one pending request) per user
            e.HasOne<Family>().WithMany().HasForeignKey(x => x.FamilyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Account>(e =>
        {
            e.ToTable("accounts", t =>
            {
                t.HasCheckConstraint("ck_accounts_balance_nonneg", "balance >= 0 OR kind = 3");
                t.HasCheckConstraint("ck_accounts_module_family", "(kind = 2) = (family_id IS NOT NULL)");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100); e.Property(x => x.CurrencyCode).HasMaxLength(3);
            e.Property(x => x.Balance).HasPrecision(19, 4);
            e.HasIndex(x => x.OwnerUserId); e.HasIndex(x => x.FamilyId);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Family>().WithMany().HasForeignKey(x => x.FamilyId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<AccountModuleAccess>(e =>
        {
            e.ToTable("module_permissions"); e.HasKey(x => new { x.AccountId, x.UserId });
            e.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ExpenseCategory>(e =>
        {
            e.ToTable("expense_categories"); e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(60); e.Property(x => x.Name).HasMaxLength(60);
            e.HasIndex(x => x.OwnerUserId);
            e.HasData(CategorySeed.All);
        });
        modelBuilder.Entity<LedgerTransaction>(e =>
        {
            e.ToTable("transactions", t => t.HasCheckConstraint("ck_tx_amount_pos", "amount > 0"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(19, 4); e.Property(x => x.CurrencyCode).HasMaxLength(3); e.Property(x => x.Note).HasMaxLength(500);
            e.HasIndex(x => new { x.FromAccountId, x.CreatedAt }); e.HasIndex(x => new { x.ToAccountId, x.CreatedAt });
            e.HasOne<Account>().WithMany().HasForeignKey(x => x.FromAccountId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Account>().WithMany().HasForeignKey(x => x.ToAccountId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ExpenseCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
