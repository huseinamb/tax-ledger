namespace TaxLedger.ExchangeAdapters.Binance;

/// <summary>
/// Registry of known Binance operation names grouped by category.
/// When a new operation name variant is discovered from a real export,
/// add it to the appropriate set here — no other code needs to change.
/// </summary>
public static class BinanceOperations
{
    public static readonly HashSet<string> Deposit = new(StringComparer.OrdinalIgnoreCase)
    {
        "Deposit"
    };

    public static readonly HashSet<string> Withdraw = new(StringComparer.OrdinalIgnoreCase)
    {
        "Withdraw",
        "Fiat Withdraw"
    };

    public static readonly HashSet<string> Transfer = new(StringComparer.OrdinalIgnoreCase)
    {
        "Transfer"
    };

    public static readonly HashSet<string> Fee = new(StringComparer.OrdinalIgnoreCase)
    {
        "Fee",
        "Transaction Fee"
    };
}