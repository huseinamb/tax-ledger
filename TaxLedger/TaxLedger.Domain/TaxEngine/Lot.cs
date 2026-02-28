using System;

namespace TaxLedger.Domain.TaxEngine
{
    public class Lot
    {
        public string Asset { get; set; }
        public decimal Amount { get; set; }         // Remaining amount in the lot
        public decimal CostInSek { get; set; }      // Total SEK cost for the lot

        public Lot(string asset, decimal amount, decimal costInSek)
        {
            Asset = asset;
            Amount = amount;
            CostInSek = costInSek;
        }
    }
}