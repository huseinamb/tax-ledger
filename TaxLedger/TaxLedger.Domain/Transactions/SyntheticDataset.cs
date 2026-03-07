using TaxLedger.Domain.Transactions;
using System;
using System.Collections.Generic;

namespace TaxLedger.Domain.Transactions
{
    public static class SyntheticDataset
    {
        public static List<CanonicalTransaction> GetSampleTransactions()
        {
            var transactions = new List<CanonicalTransaction>();

            // 1️ Deposit 100,000 SEK (SEK enters wallet)
            transactions.Add(new CanonicalTransaction(
                timestamp: new DateTime(2024, 01, 01, 10, 0, 0),
                type: TransactionType.Deposit,
                assetIn: "SEK", amountIn: 100_000m,
                assetOut: null, amountOut: 0m,
                feeAsset: null, feeAmount: 0m,
                fiatValueAtTimestamp: 100_000m,
                fiatValueAtTimestampCurrency: "SEK"
            ));

            // 2️ Buy 1 BTC at 10,000 SEK (BTC enters, SEK leaves)
            transactions.Add(new CanonicalTransaction(
                timestamp: new DateTime(2024, 01, 02, 12, 0, 0),
                type: TransactionType.Trade,
                assetIn: "BTC", amountIn: 1m,
                assetOut: "SEK", amountOut: 10_000m,
                feeAsset: "BTC", feeAmount: 0.001m,
                fiatValueAtTimestamp: 10_000m,
                fiatValueAtTimestampCurrency: "SEK"
            ));

            // 3️ Buy 1 BTC at 20,000 SEK (BTC enters, SEK leaves)
            transactions.Add(new CanonicalTransaction(
                timestamp: new DateTime(2024, 01, 03, 14, 0, 0),
                type: TransactionType.Trade,
                assetIn: "BTC", amountIn: 1m,
                assetOut: "SEK", amountOut: 20_000m,
                feeAsset: "BTC", feeAmount: 0.002m,
                fiatValueAtTimestamp: 20_000m,
                fiatValueAtTimestampCurrency: "SEK"
            ));

            // 4️ Sell 1.5 BTC at 30,000 SEK (BTC leaves, SEK enters)
            transactions.Add(new CanonicalTransaction(
                timestamp: new DateTime(2024, 01, 04, 16, 0, 0),
                type: TransactionType.Trade,
                assetIn: "SEK", amountIn: 30_000m,
                assetOut: "BTC", amountOut: 1.5m,
                feeAsset: "BTC", feeAmount: 0.0015m,
                fiatValueAtTimestamp: 30_000m,
                fiatValueAtTimestampCurrency: "SEK"
            ));

            return transactions;
        }
    }
}