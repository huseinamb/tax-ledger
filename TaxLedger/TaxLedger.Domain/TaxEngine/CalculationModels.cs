using TaxLedger.Domain.Transactions;

namespace TaxLedger.Domain.TaxEngine
{
    public class AssetHolding
    {
        public decimal TotalAmount { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class TaxCalculationResult
    {
        public required CanonicalTransaction OriginTransaction { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal GainLoss => SalePrice - PurchasePrice;
        public required string Asset { get; set; }
    }
}