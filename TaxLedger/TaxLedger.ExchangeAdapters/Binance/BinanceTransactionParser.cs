using System;
using System.Collections.Generic;
using System.Linq;
using TaxLedger.Domain.Transactions;
using TaxLedger.Application.Parsing;

namespace TaxLedger.ExchangeAdapters.Binance;

public sealed class BinanceTransactionParser : IExchangeTransactionParser<BinanceRawRow>
{
    public IEnumerable<CanonicalTransaction> Parse(IEnumerable<BinanceRawRow> rows)
    {
        var canonicalList = new List<CanonicalTransaction>();
        var rowList = rows.ToList();

        // --- Non-trade rows: deposits, withdrawals, transfers ---
        var nonTradeRows = rowList
            .Where(r => IsDeposit(r) || IsWithdrawal(r) || IsTransfer(r));

        foreach (var row in nonTradeRows)
        {
            var type = IsDeposit(row) ? TransactionType.Deposit :
                       IsWithdrawal(row) ? TransactionType.Withdrawal :
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
                fiatValueAtTimestamp: 0m,
                fiatValueAtTimestampCurrency: "USD"
            ));
        }

        // --- Trade rows: everything that is not deposit/withdrawal/transfer ---
        var tradeRows = rowList
            .Where(r => !IsDeposit(r) && !IsWithdrawal(r) && !IsTransfer(r))
            .OrderBy(r => r.Time)
            .ToList();

        // Sum changes for identical Time+Operation+Coin
        // This handles Binance splitting one trade into n identical row sets
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

        // Reconstruct each trade from its legs grouped by timestamp
        var tradesByTime = grouped.GroupBy(g => g.Time);

        foreach (var timeGroup in tradesByTime)
        {
            // Fee leg identified first to exclude it from in/out detection
            var feeRow = timeGroup.FirstOrDefault(r => BinanceOperations.Fee.Contains(r.Operation));
            var inRow = timeGroup.FirstOrDefault(r => r.TotalChange > 0 && r != feeRow);
            var outRow = timeGroup.FirstOrDefault(r => r.TotalChange < 0 && r != feeRow);

            if (inRow == null && outRow == null)
            {
                Console.WriteLine(
                    $"Warning: skipping unrecognised row group at {timeGroup.Key} " +
                    $"— no in/out legs found. Operations: " +
                    $"{string.Join(", ", timeGroup.Select(r => r.Operation))}");
                continue;
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
                fiatValueAtTimestamp: 0m,
                fiatValueAtTimestampCurrency: "USD"
            ));
        }

        return canonicalList.OrderBy(t => t.Timestamp).ToList();
    }

    private static bool IsDeposit(BinanceRawRow r) => BinanceOperations.Deposit.Contains(r.Operation);
    private static bool IsWithdrawal(BinanceRawRow r) => BinanceOperations.Withdraw.Contains(r.Operation);
    private static bool IsTransfer(BinanceRawRow r) => BinanceOperations.Transfer.Contains(r.Operation);
}