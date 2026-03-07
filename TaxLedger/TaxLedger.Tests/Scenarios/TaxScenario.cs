using TaxLedger.Domain.Transactions;

public class TaxScenario
{
    public string Name { get; set; } = string.Empty;
    public List<CanonicalTransaction> Transactions { get; set; } = new();

    // Using Dictionaries allows you to support multi-asset scenarios (e.g., BTC, ETH, SOL)
    public Dictionary<string, decimal> ExpectedCapitalGains { get; set; } = new();
    public Dictionary<string, decimal> ExpectedCapitalLosses { get; set; } = new();
}