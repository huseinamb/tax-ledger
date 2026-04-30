namespace TaxLedger.Domain.Reporting;

public sealed class TaxReportSummaryRow
{
    public required string Asset { get; init; }
    public decimal TotalSalePrice { get; init; }
    public decimal TotalCostBasis { get; init; }
    public decimal TotalGain { get; init; }
    public decimal TotalLoss { get; init; }
}