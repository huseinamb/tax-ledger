using TaxLedger.Domain.Transactions;

namespace TaxLedger.Domain.TaxEngine
{
    public class AssetHolding
    {
        public decimal TotalAmount { get; set; }
        public decimal TotalCost { get; set; } // The 'Omkostnadsbelopp' in Sweden
    }

    public class TaxCalculationResult
    {
        public CanonicalTransaction OriginTransaction { get; set; }
        public decimal PurchasePrice { get; set; } // Total cost basis for this sale
        public decimal SalePrice { get; set; }     // Total fiat value received
        public decimal GainLoss => SalePrice - PurchasePrice;
        public string Asset { get; set; }
    }
}