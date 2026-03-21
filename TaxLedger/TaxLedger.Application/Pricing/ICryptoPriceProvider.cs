namespace TaxLedger.Application.Pricing;

public interface ICryptoPriceProvider
{
    /// <summary>
    /// Returns the price of <paramref name="asset"/> in USD at the given timestamp.
    /// </summary>
    Task<decimal> GetPriceInUsdAsync(string asset, DateTime timestamp);

    /// <summary>
    /// Returns true if this provider can price the given asset.
    /// </summary>
    bool Supports(string asset);
}