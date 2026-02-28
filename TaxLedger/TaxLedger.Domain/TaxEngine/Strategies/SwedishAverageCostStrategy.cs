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
                // TODO: Handle 'Lån' (Loans). 
                // In Sweden, lending crypto (utlåning) can be a taxable disposal if you lose 
                // "disfogensrätt" (right of disposal) or receive a different token (e.g., aBTC).

                // --- SCENARIO: ACQUISITION (Entering/Increasing a Position) ---
                // Covers: 
                // 1. Buy BTC with SEK (AssetOut: BTC, AssetIn: SEK)
                // 2. Received ETH from a Swap (AssetOut: ETH)
                // 3. Deposit BTC from another wallet (AssetOut: BTC, AssetIn: null)
                if (!string.IsNullOrEmpty(tx.AssetOut) && tx.AssetOut != "SEK")
                {
                    UpdatePool(
                        tx.AssetOut,
                        tx.AmountOut,
                        tx.FiatValueAtTimestamp,
                        tx.FeeAmount,
                        tx.FeeAsset,
                        isAcquisition: true
                    );
                }

                // --- SCENARIO: DISPOSAL (Leaving/Reducing a Position) ---
                // Covers:
                // 1. Sell BTC for SEK (AssetIn: BTC, AssetOut: SEK)
                // 2. Swap BTC for ETH (AssetIn: BTC - this part is the "Sale" of BTC)
                // 3. Using crypto to pay for a service (AssetIn: Crypto, AssetOut: null/Service)
                if (!string.IsNullOrEmpty(tx.AssetIn) && tx.AssetIn != "SEK")
                {
                    // SWEDEN RULE: Sale Price is reduced by fees ONLY if paid in SEK.
                    decimal disposalFeeInSek = (tx.FeeAsset == "SEK") ? tx.FeeAmount : 0m;

                    // TODO: Handle 'FeeAsset' if NOT SEK. 
                    // If a fee is paid in BNB/BTC, that fee is technically a separate "Sale" 
                    // of that crypto and needs its own TaxCalculationResult.

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
                decimal feeInSek = (feeAsset == "SEK") ? feeAmount : 0m;
                _holdings[asset].TotalCost += (fiatValue + feeInSek);

                // TODO: If feeAsset != "SEK", the SEK value of that crypto fee should still 
                // be added to this asset's cost basis, but it requires a price look-up.
            }
        }

        private TaxCalculationResult? ProcessDisposal(CanonicalTransaction tx, decimal feeInSek)
        {
            // Safety Check: If we sell an asset we don't have recorded.
            if (!_holdings.ContainsKey(tx.AssetIn!) || _holdings[tx.AssetIn!].TotalAmount == 0)
            {
                // TODO: Handle "Missing Acquisition". Usually, we assume 0 cost basis (100% profit) 
                // or throw a warning to the user to check their transaction history.
                return null;
            }

            var holding = _holdings[tx.AssetIn!];

            // Math: GAV (Genomsnittligt omkostnadsbelopp) per unit
            decimal averageCostPerUnit = holding.TotalCost / holding.TotalAmount;
            decimal costBasisOfSoldAmount = tx.AmountIn * averageCostPerUnit;

            // Sale Price after deducting SEK fees
            decimal netSalePrice = tx.FiatValueAtTimestamp - feeInSek;

            var result = new TaxCalculationResult
            {
                OriginTransaction = tx,
                Asset = tx.AssetIn!,
                PurchasePrice = costBasisOfSoldAmount, // Used for K4 Form
                SalePrice = netSalePrice              // Used for K4 Form
            };

            // Update pool for the next transaction in the timeline
            holding.TotalAmount -= tx.AmountIn;
            holding.TotalCost -= costBasisOfSoldAmount;

            return result;
        }
    }
}