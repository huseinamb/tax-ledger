using System;
using System.Collections.Generic;

namespace TaxLedger.Domain.Transactions
{
    public static class SyntheticDataset
    {
        public static List<CanonicalTransaction> GetSampleTransactions()
        {
            var transactions = new List<CanonicalTransaction>();

            // 1️⃣ Deposit 100,000 SEK
            transactions.Add(new CanonicalTransaction(
                timestamp: new DateTime(2024, 01, 01, 10, 0, 0),
                type: TransactionType.Deposit,
                assetIn: "SEK",
                amountIn: 0m,             // Deposits don’t “spend” anything
                assetOut: "SEK",
                amountOut: 100_000m,
                feeAsset: null,
                feeAmount: 0m,
                fiatValueAtTimestamp: 100_000m,
                fiatValueAtTimestampCurrency: "SEK"
            ));

            // 2️⃣ Buy 1 BTC at 10,000 SEK
            transactions.Add(new CanonicalTransaction(
                timestamp: new DateTime(2024, 01, 02, 12, 0, 0),
                type: TransactionType.Trade,
                assetIn: "SEK",
                amountIn: 10_000m,
                assetOut: "BTC",
                amountOut: 1m,
                feeAsset: "BTC",
                feeAmount: 0.001m,
                fiatValueAtTimestamp: 10_000m,
                fiatValueAtTimestampCurrency: "SEK"
            ));

            // 3️⃣ Buy 1 BTC at 20,000 SEK
            transactions.Add(new CanonicalTransaction(
                timestamp: new DateTime(2024, 01, 03, 14, 0, 0),
                type: TransactionType.Trade,
                assetIn: "SEK",
                amountIn: 20_000m,
                assetOut: "BTC",
                amountOut: 1m,
                feeAsset: "BTC",
                feeAmount: 0.002m,
                fiatValueAtTimestamp: 20_000m,
                fiatValueAtTimestampCurrency: "SEK"
            ));

            // 4️⃣ Sell 1.5 BTC at 30,000 SEK
            transactions.Add(new CanonicalTransaction(
                timestamp: new DateTime(2024, 01, 04, 16, 0, 0),
                type: TransactionType.Trade,
                assetIn: "BTC",
                amountIn: 1.5m,
                assetOut: "SEK",
                amountOut: 30_000m,
                feeAsset: "BTC",
                feeAmount: 0.0015m,
                fiatValueAtTimestamp: 30_000m,
                fiatValueAtTimestampCurrency: "SEK"

            ));

            return transactions;
        }
    }
}