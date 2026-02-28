using TaxLedger.Domain.TaxEngine.Strategies;
using TaxLedger.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TaxLedger.Tests.TaxEngine.Strategies
{
    public class SwedishAverageCostStrategyTests
    {
        [Fact]
        public void Calculate_BtcToEthSwap_CorrectlyCalculatesGAVAndFees()
        {
            // Arrange
            var strategy = new SwedishAverageCostStrategy();
            var transactions = new List<CanonicalTransaction>
            {
                // 1. Buy 1 BTC for 500,000 SEK + 500 SEK fee
                new CanonicalTransaction(
                    timestamp: DateTime.Now.AddDays(-10),
                    type: TransactionType.Trade,
                    assetIn: "SEK",
                    amountIn: 500000m,
                    assetOut: "BTC",
                    amountOut: 1m,
                    feeAsset: "SEK",
                    feeAmount: 500m,
                    fiatValueAtTimestamp: 500000m,
                    fiatValueAtTimestampCurrency: "SEK"
                ),
                // 2. Swap 0.5 BTC for 10 ETH (Value 300,000 SEK) + 200 SEK fee
                new CanonicalTransaction(
                    timestamp: DateTime.Now.AddDays(-5),
                    type: TransactionType.Trade,
                    assetIn: "BTC",
                    amountIn: 0.5m,
                    assetOut: "ETH",
                    amountOut: 10m,
                    feeAsset: "SEK",
                    feeAmount: 200m,
                    fiatValueAtTimestamp: 300000m,
                    fiatValueAtTimestampCurrency: "SEK"
                )
            };

            // Act
            var results = strategy.Calculate(transactions).ToList();

            // Assert
            var btcResult = results.First(r => r.Asset == "BTC");

            // Expected Purchase Price (GAV): ((500,000 + 500) / 1) * 0.5 = 250,250
            Assert.Equal(250250m, btcResult.PurchasePrice);

            // Expected Sale Price: 300,000 - 200 = 299,800
            Assert.Equal(299800m, btcResult.SalePrice);
        }
    }
}