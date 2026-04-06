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
            var sortedTxs = transactions.OrderBy(t => t.Timestamp).ToList();

            foreach (var tx in sortedTxs)
            {
                // Non-taxable events: still affect the pool but produce no result
                if (tx.Type == TransactionType.Withdrawal || tx.Type == TransactionType.Transfer)
                {
                    if (!string.IsNullOrEmpty(tx.AssetOut) && tx.AssetOut != "SEK")
                        ReducePool(tx.AssetOut, tx.AmountOut);
                    continue; // skip disposal logic entirely
                }

                // --- ACQUISITION ---
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

                // --- DISPOSAL (trades only) ---
                if (!string.IsNullOrEmpty(tx.AssetOut) && tx.AssetOut != "SEK")
                {
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
        private void ReducePool(string asset, decimal amount)
        {
            if (!_holdings.ContainsKey(asset) || _holdings[asset].TotalAmount == 0)
            {
                Console.WriteLine($"Warning: withdrawal/transfer of {amount} {asset} " +
                                  $"but no holding found. Possible missing purchase history.");
                return;
            }

            var holding = _holdings[asset];
            decimal averageCostPerUnit = holding.TotalCost / holding.TotalAmount;

            // Reduce both amount and cost proportionally — same math as a disposal
            // but without generating a taxable event
            holding.TotalAmount -= amount;
            holding.TotalCost -= amount * averageCostPerUnit;
        }
    }
}