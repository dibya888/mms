using MoneyApp.Domain;

namespace MoneyApp.Infrastructure.Persistence;

public static class CurrencySeed
{
    private static Currency C(string code, string name, string symbol, int dp = 2) => new() { Code = code, Name = name, Symbol = symbol, DecimalPlaces = dp };
    public static readonly Currency[] All =
    [
        C("BDT","Bangladeshi Taka","৳"), C("USD","US Dollar","$"), C("EUR","Euro","€"), C("GBP","Pound Sterling","£"),
        C("INR","Indian Rupee","₹"), C("JPY","Japanese Yen","¥",0), C("CNY","Chinese Yuan","¥"), C("AUD","Australian Dollar","A$"),
        C("CAD","Canadian Dollar","C$"), C("CHF","Swiss Franc","CHF"), C("SGD","Singapore Dollar","S$"), C("HKD","Hong Kong Dollar","HK$"),
        C("NZD","New Zealand Dollar","NZ$"), C("SEK","Swedish Krona","kr"), C("NOK","Norwegian Krone","kr"), C("DKK","Danish Krone","kr"),
        C("PLN","Polish Złoty","zł"), C("CZK","Czech Koruna","Kč"), C("HUF","Hungarian Forint","Ft"), C("RUB","Russian Ruble","₽"),
        C("TRY","Turkish Lira","₺"), C("AED","UAE Dirham","د.إ"), C("SAR","Saudi Riyal","﷼"), C("QAR","Qatari Riyal","﷼"),
        C("KWD","Kuwaiti Dinar","د.ك",3), C("MYR","Malaysian Ringgit","RM"), C("IDR","Indonesian Rupiah","Rp"), C("THB","Thai Baht","฿"),
        C("PHP","Philippine Peso","₱"), C("VND","Vietnamese Dong","₫",0), C("KRW","South Korean Won","₩",0), C("PKR","Pakistani Rupee","₨"),
        C("LKR","Sri Lankan Rupee","Rs"), C("NPR","Nepalese Rupee","Rs"), C("BRL","Brazilian Real","R$"), C("MXN","Mexican Peso","$"),
        C("ARS","Argentine Peso","$"), C("ZAR","South African Rand","R"), C("EGP","Egyptian Pound","E£"), C("NGN","Nigerian Naira","₦"),
        C("KES","Kenyan Shilling","KSh"), C("ILS","Israeli Shekel","₪"),
    ];
}

public static class CategorySeed
{
    private static readonly string[] Keys =
    [
        "food","groceries","dining_out","transport","fuel","rent","utilities","electricity","water","gas","internet","mobile",
        "healthcare","medicine","education","tuition","clothing","shopping","entertainment","travel","gifts","charity",
        "insurance","household","personal_care","fees","taxes","other"
    ];
    // Deterministic IDs so HasData migrations are stable.
    public static readonly ExpenseCategory[] All = Keys.Select((k, i) =>
        new ExpenseCategory { Id = new Guid($"00000000-0000-0000-0000-{i + 1:D12}"), Key = k }).ToArray();
}
