using TaxLedger.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TaxLedger.Domain.TaxEngine.Strategies
{
    public class SwedishAverageCostStrategy : ITaxCalculationStrategy
    {
        private readonly Dictionary<string, AssetHolding> _holdings = new();

        public IEnumerable<TaxCalculationResult> Calculate(IEnumerable<CanonicalTransaction> transactions)
        {
            var results = new List<TaxCalculationResult>();
            // Crucial: Process chronological order to maintain correct GAV
            var sortedTxs = transactions.OrderBy(t => t.Timestamp).ToList();

            foreach (var tx in sortedTxs)
            {
                // --- SCENARIO: ACQUISITION (Asset enters the wallet) ---
                // AssetIn is the crypto you just got (e.g., BTC).
                if (!string.IsNullOrEmpty(tx.AssetIn) && tx.AssetIn != "SEK")
                {
                    UpdatePool(
                        tx.AssetIn,
                        tx.AmountIn,
                        tx.FiatValueAtTimestamp,
                        tx.FeeAmount,
                        tx.FeeAsset,
                        isAcquisition: true
                    );
                }

                // --- SCENARIO: DISPOSAL (Asset leaves the wallet) ---
                // AssetOut is the crypto you just gave away or sold.
                if (!string.IsNullOrEmpty(tx.AssetOut) && tx.AssetOut != "SEK")
                {
                    // SWEDEN RULE: Sale Price is reduced by fees ONLY if paid in SEK.
                    decimal disposalFeeInSek = (tx.FeeAsset == "SEK") ? tx.FeeAmount : 0m;

                    var result = ProcessDisposal(tx, disposalFeeInSek);
                    if (result != null) results.Add(result);
                }
            }

            return results;
        }

        private void UpdatePool(string asset, decimal amount, decimal fiatValue, decimal feeAmount, string? feeAsset, bool isAcquisition)
        {
            if (!_holdings.ContainsKey(asset))
                _holdings[asset] = new AssetHolding();

            if (isAcquisition)
            {
                _holdings[asset].TotalAmount += amount;

                // SWEDEN RULE: Acquisition cost (Omkostnadsbelopp) increases by value + SEK fees.
                // If you buy BTC for 10,000 + 100 fee, your cost basis is 10,100.
                decimal feeInSek = (feeAsset == "SEK") ? feeAmount : 0m;
                _holdings[asset].TotalCost += (fiatValue + feeInSek);
            }
        }

        private TaxCalculationResult? ProcessDisposal(CanonicalTransaction tx, decimal feeInSek)
        {
            string assetSold = tx.AssetOut!;

            // Safety Check: Avoid negative balances
            if (!_holdings.ContainsKey(assetSold) || _holdings[assetSold].TotalAmount == 0)
            {
                // In a production app, we would log a warning: "Missing purchase history for {assetSold}"
                return null;
            }

            var holding = _holdings[assetSold];

            // GAV Calculation (Average Cost per unit)
            decimal averageCostPerUnit = holding.TotalCost / holding.TotalAmount;

            // This is the "Omkostnadsbelopp" for the specific amount sold
            decimal costBasisOfSoldAmount = tx.AmountOut * averageCostPerUnit;

            // Sale Price (Försäljningspris) after deducting SEK fees
            // Note: If this was a swap, FiatValueAtTimestamp is the SEK market value of the trade
            decimal netSalePrice = tx.FiatValueAtTimestamp - feeInSek;

            var result = new TaxCalculationResult
            {
                OriginTransaction = tx,
                Asset = assetSold,
                PurchasePrice = costBasisOfSoldAmount, // "Omkostnadsbelopp" for K4
                SalePrice = netSalePrice              // "Försäljningspris" for K4
            };

            // Update pool: Reduce holdings by the amount that left the wallet
            holding.TotalAmount -= tx.AmountOut;
            holding.TotalCost -= costBasisOfSoldAmount;

            return result;
        }
    }
}