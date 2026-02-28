using TaxLedger.Domain.TaxEngine;

namespace TaxLedger.Domain.Reporting
{
    public interface ITaxReportGenerator
    {
        void Export(IEnumerable<TaxCalculationResult> results, string filePath);
    }
}