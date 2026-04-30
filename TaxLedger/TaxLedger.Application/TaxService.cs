using TaxLedger.Domain.Transactions;
using TaxLedger.Domain.TaxEngine;
using TaxLedger.Domain.Reporting;

namespace TaxLedger.Application;

public class TaxService
{
    private readonly ITaxCalculationStrategy _calculationStrategy;

    public TaxService(ITaxCalculationStrategy strategy)
    {
        _calculationStrategy = strategy;
    }

    /// <summary>
    /// Calculates tax results for the target year and returns them.
    /// Use this method when you need the results for further processing (API, console output etc.)
    /// </summary>
    public IEnumerable<TaxCalculationResult> CalculateTax(
        IEnumerable<CanonicalTransaction> transactions,
        int targetYear)
    {
        var historicalData = transactions
            .Where(t => t.Timestamp.Year <= targetYear)
            .OrderBy(t => t.Timestamp);

        var allResults = _calculationStrategy.Calculate(historicalData);

        return allResults
            .Where(r => r.OriginTransaction.Timestamp.Year == targetYear)
            .ToList();
    }

}