using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // Prefer static readonly fields over constant array arguments

namespace MoneyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    decimal_places = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_currencies", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "expense_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expense_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "families",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    public_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_families", x => x.id);
                    table.ForeignKey(
                        name: "fk_families_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    balance = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.id);
                    table.CheckConstraint("ck_accounts_balance_nonneg", "balance >= 0 OR kind = 3");
                    table.CheckConstraint("ck_accounts_module_family", "(kind = 2) = (family_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_accounts_currencies_currency_code",
                        column: x => x.currency_code,
                        principalTable: "currencies",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_accounts_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_accounts_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "family_members",
                columns: table => new
                {
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_family_members", x => new { x.family_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_family_members_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_family_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "module_permissions",
                columns: table => new
                {
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permissions = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_module_permissions", x => new { x.account_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_module_permissions_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_module_permissions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    from_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                    table.CheckConstraint("ck_tx_amount_pos", "amount > 0");
                    table.ForeignKey(
                        name: "fk_transactions_accounts_from_account_id",
                        column: x => x.from_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_accounts_to_account_id",
                        column: x => x.to_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_expense_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "expense_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "currencies",
                columns: new[] { "code", "decimal_places", "name", "symbol" },
                values: new object[,]
                {
                    { "AED", 2, "UAE Dirham", "د.إ" },
                    { "ARS", 2, "Argentine Peso", "$" },
                    { "AUD", 2, "Australian Dollar", "A$" },
                    { "BDT", 2, "Bangladeshi Taka", "৳" },
                    { "BRL", 2, "Brazilian Real", "R$" },
                    { "CAD", 2, "Canadian Dollar", "C$" },
                    { "CHF", 2, "Swiss Franc", "CHF" },
                    { "CNY", 2, "Chinese Yuan", "¥" },
                    { "CZK", 2, "Czech Koruna", "Kč" },
                    { "DKK", 2, "Danish Krone", "kr" },
                    { "EGP", 2, "Egyptian Pound", "E£" },
                    { "EUR", 2, "Euro", "€" },
                    { "GBP", 2, "Pound Sterling", "£" },
                    { "HKD", 2, "Hong Kong Dollar", "HK$" },
                    { "HUF", 2, "Hungarian Forint", "Ft" },
                    { "IDR", 2, "Indonesian Rupiah", "Rp" },
                    { "ILS", 2, "Israeli Shekel", "₪" },
                    { "INR", 2, "Indian Rupee", "₹" },
                    { "JPY", 0, "Japanese Yen", "¥" },
                    { "KES", 2, "Kenyan Shilling", "KSh" },
                    { "KRW", 0, "South Korean Won", "₩" },
                    { "KWD", 3, "Kuwaiti Dinar", "د.ك" },
                    { "LKR", 2, "Sri Lankan Rupee", "Rs" },
                    { "MXN", 2, "Mexican Peso", "$" },
                    { "MYR", 2, "Malaysian Ringgit", "RM" },
                    { "NGN", 2, "Nigerian Naira", "₦" },
                    { "NOK", 2, "Norwegian Krone", "kr" },
                    { "NPR", 2, "Nepalese Rupee", "Rs" },
                    { "NZD", 2, "New Zealand Dollar", "NZ$" },
                    { "PHP", 2, "Philippine Peso", "₱" },
                    { "PKR", 2, "Pakistani Rupee", "₨" },
                    { "PLN", 2, "Polish Złoty", "zł" },
                    { "QAR", 2, "Qatari Riyal", "﷼" },
                    { "RUB", 2, "Russian Ruble", "₽" },
                    { "SAR", 2, "Saudi Riyal", "﷼" },
                    { "SEK", 2, "Swedish Krona", "kr" },
                    { "SGD", 2, "Singapore Dollar", "S$" },
                    { "THB", 2, "Thai Baht", "฿" },
                    { "TRY", 2, "Turkish Lira", "₺" },
                    { "USD", 2, "US Dollar", "$" },
                    { "VND", 0, "Vietnamese Dong", "₫" },
                    { "ZAR", 2, "South African Rand", "R" }
                });

            migrationBuilder.InsertData(
                table: "expense_categories",
                columns: new[] { "id", "key", "name", "owner_user_id" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "food", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "groceries", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "dining_out", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "transport", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "fuel", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "rent", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "utilities", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "electricity", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "water", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "gas", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "internet", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "mobile", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "healthcare", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "medicine", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "education", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "tuition", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000017"), "clothing", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000018"), "shopping", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000019"), "entertainment", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000020"), "travel", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "gifts", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "charity", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "insurance", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000024"), "household", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000025"), "personal_care", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000026"), "fees", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000027"), "taxes", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000028"), "other", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_currency_code",
                table: "accounts",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_family_id",
                table: "accounts",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_owner_user_id",
                table: "accounts",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_expense_categories_owner_user_id",
                table: "expense_categories",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_families_owner_user_id",
                table: "families",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_families_public_id",
                table: "families",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_family_members_user_id",
                table: "family_members",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_module_permissions_user_id",
                table: "module_permissions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_family_id",
                table: "refresh_tokens",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_category_id",
                table: "transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_from_account_id_created_at",
                table: "transactions",
                columns: new[] { "from_account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_to_account_id_created_at",
                table: "transactions",
                columns: new[] { "to_account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_users_normalized_email",
                table: "users",
                column: "normalized_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "family_members");

            migrationBuilder.DropTable(
                name: "module_permissions");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "expense_categories");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "families");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
