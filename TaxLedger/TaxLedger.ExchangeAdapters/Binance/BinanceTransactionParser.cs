using System;
using System.Collections.Generic;
using System.Linq;
using TaxLedger.Domain.Transactions;
using TaxLedger.Application.Parsing;

namespace TaxLedger.ExchangeAdapters.Binance
{
    public sealed class BinanceTransactionParser : IExchangeTransactionParser<BinanceRawRow>
    {
        public IEnumerable<CanonicalTransaction> Parse(IEnumerable<BinanceRawRow> rows)
        {
            var canonicalList = new List<CanonicalTransaction>();

            // Handle deposits, withdrawals, transfers directly
            var nonTradeRows = rows
                .Where(r => r.Operation.Contains("Deposit", StringComparison.OrdinalIgnoreCase) ||
                            r.Operation.Contains("Withdraw", StringComparison.OrdinalIgnoreCase) ||
                            r.Operation.Contains("Transfer", StringComparison.OrdinalIgnoreCase));

            foreach (var row in nonTradeRows)
            {
                var type = row.Operation.Contains("Deposit", StringComparison.OrdinalIgnoreCase) ? TransactionType.Deposit :
                           row.Operation.Contains("Withdraw", StringComparison.OrdinalIgnoreCase) ? TransactionType.Withdrawal :
                           TransactionType.Transfer;

                canonicalList.Add(new CanonicalTransaction(
                    timestamp: row.Time,
                    type: type,
                    assetIn: row.Change > 0 ? row.Coin : null,
                    amountIn: row.Change > 0 ? Convert.ToDecimal(row.Change) : 0m,
                    assetOut: row.Change < 0 ? row.Coin : null,
                    amountOut: row.Change < 0 ? Math.Abs(Convert.ToDecimal(row.Change)) : 0m,
                    feeAsset: null,
                    feeAmount: 0m,
                    fiatValueAtTimestamp: Math.Abs(Convert.ToDecimal(row.Change)), // best effort for now
                    fiatValueAtTimestampCurrency: row.Coin // placeholder, may improve later
                ));
            }

            // Handle trades (everything else)
            var tradeRows = rows
                .Where(r => !r.Operation.Contains("Deposit", StringComparison.OrdinalIgnoreCase) &&
                            !r.Operation.Contains("Withdraw", StringComparison.OrdinalIgnoreCase) &&
                            !r.Operation.Contains("Transfer", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => r.Time) // ensure chronological order
                .ToList();

            // Group by Time, Operation, Coin to reduce duplicates (sum changes)
            var grouped = tradeRows
                .GroupBy(r => new { r.Time, r.Operation, r.Coin })
                .Select(g => new
                {
                    g.Key.Time,
                    g.Key.Operation,
                    g.Key.Coin,
                    TotalChange = g.Sum(x => x.Change)
                })
                .ToList();

            // For each trade group, map to CanonicalTransaction
            // Assumption: after grouping we will have either 2 rows (in/out) or 3 rows (in/out/fee)
            // AssetIn: positive change, AssetOut: negative change, Fee: regex or Operation contains "Fee"
            var tradesByTime = grouped.GroupBy(g => g.Time);

            foreach (var timeGroup in tradesByTime)
            {
                var feeRow = timeGroup.FirstOrDefault(r => r.Operation.Contains("Fee", StringComparison.OrdinalIgnoreCase));
                var inRow = timeGroup.FirstOrDefault(r => r.TotalChange > 0 && r != feeRow);
                var outRow = timeGroup.FirstOrDefault(r => r.TotalChange < 0 && r != feeRow);



                if (inRow == null && outRow == null)
                {
                    throw new InvalidOperationException($"Trade at {timeGroup.Key} has no in/out rows.");
                }

                canonicalList.Add(new CanonicalTransaction(
                         timestamp: timeGroup.Key,
                         type: TransactionType.Trade,
                         assetIn: inRow?.Coin,
                         amountIn: inRow != null ? Convert.ToDecimal(inRow.TotalChange) : 0m,
                         assetOut: outRow?.Coin,
                         amountOut: outRow != null ? Math.Abs(Convert.ToDecimal(outRow.TotalChange)) : 0m,
                         feeAsset: feeRow?.Coin,
                         feeAmount: feeRow != null ? Math.Abs(Convert.ToDecimal(feeRow.TotalChange)) : 0m,

                         // TODO: calculate correct fiat value based on exchange rates
                         fiatValueAtTimestamp: 0m,

                         // TODO: determine the correct fiat currency for this trade
                         fiatValueAtTimestampCurrency: "USD"
                     ));
                                }

            return canonicalList.OrderBy(t => t.Timestamp).ToList();
        }
    }
}