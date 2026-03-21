using TaxLedger.Domain.Transactions;

namespace TaxLedger.Application.Pricing;

public interface IPriceEnrichmentService
{
    /// <summary>
    /// Takes a collection of parsed canonical transactions with no fiat value
    /// and returns them enriched with accurate fiat values at transaction time.
    /// </summary>
    Task<IEnumerable<CanonicalTransaction>> EnrichAsync(
        IEnumerable<CanonicalTransaction> transactions,
        string targetFiatCurrency);
}