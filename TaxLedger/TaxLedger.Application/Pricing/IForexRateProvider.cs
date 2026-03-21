namespace TaxLedger.Application.Pricing;

public interface IForexRateProvider
{
    /// <summary>
    /// Returns the exchange rate from <paramref name="fromCurrency"/> to
    /// <paramref name="toCurrency"/> for the given date.
    /// </summary>
    Task<decimal> GetRateAsync(string fromCurrency, string toCurrency, DateTime date);

    /// <summary>
    /// Prefetches and caches rates for a range of dates in a single request.
    /// Implementations that don't support batching can leave this as a no-op.
    /// </summary>
    Task PrefetchAsync(string fromCurrency, string toCurrency, IEnumerable<DateTime> dates);

    bool Supports(string fromCurrency, string toCurrency);
}