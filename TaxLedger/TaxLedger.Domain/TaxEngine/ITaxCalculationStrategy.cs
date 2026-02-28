using TaxLedger.Domain.Transactions;

namespace TaxLedger.Domain.TaxEngine
{
    public interface ITaxCalculationStrategy
    {
        // We process all transactions to produce a list of taxable events
        IEnumerable<TaxCalculationResult> Calculate(IEnumerable<CanonicalTransaction> transactions);
    }
}