using TaxLedger.Domain.Reporting;
using TaxLedger.Domain.TaxEngine;

namespace TaxLedger.Application;

public static class TaxReportSummarizer
{
    public static IEnumerable<TaxReportSummaryRow> Summarize(
        IEnumerable<TaxCalculationResult> results)
    {
        return results
            .GroupBy(r => r.Asset)
            .Select(g => new TaxReportSummaryRow
            {
                Asset = g.Key,
                TotalSalePrice = g.Sum(x => x.SalePrice),
                TotalCostBasis = g.Sum(x => x.PurchasePrice),
                TotalGain = g.Where(x => (x.SalePrice - x.PurchasePrice) > 0)
                                  .Sum(x => x.SalePrice - x.PurchasePrice),
                TotalLoss = g.Where(x => (x.SalePrice - x.PurchasePrice) < 0)
                                  .Sum(x => Math.Abs(x.SalePrice - x.PurchasePrice))
            });
    }
}