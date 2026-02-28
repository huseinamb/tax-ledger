using System;
using Xunit;
using TaxLedger.Domain.Transactions;
using TaxLedger.Domain.TaxEngine;

namespace TaxLedger.Tests.Transactions
{
    public class AverageCostCalculatorTests
    {
        [Fact]
        public void BuyCrypto_AddsToHoldings()
        {
            // Arrange
            var calc = new AverageCostCalculator();
            var tx = new CanonicalTransaction(
                timestamp: new DateTime(2023, 01, 01),       // safe timestamp
                type: TransactionType.Trade,
                assetIn: "FIAT",                              // buying crypto with fiat
                amountIn: 50000m,                             // amount of fiat spent
                assetOut: "BTC",                              // crypto received
                amountOut: 1m,
                feeAsset: null,
                feeAmount: 0m,
                fiatValueAtTimestamp: 50000m,
                fiatValueAtTimestampCurrency: "SEK"
            );

            // Act
            calc.ProcessTransaction(tx);

            // Assert
            var holdings = calc.GetHoldings("BTC");
            Assert.NotNull(holdings);
            Assert.Equal(1m, holdings.Amount);
            Assert.Equal(50000m, holdings.TotalCost);
        }

        [Fact]
        public void SellCrypto_CalculatesGainAndUpdatesHoldings()
        {
            // Arrange
            var calc = new AverageCostCalculator();

            // First add crypto holdings
            calc.AddHolding("BTC", 1m, 50000m);  // 1 BTC acquired at 50k SEK

            var tx = new CanonicalTransaction(
                timestamp: new DateTime(2023, 01, 02),
                type: TransactionType.Trade,
                assetIn: "BTC",        // selling BTC
                amountIn: 0.5m,        // sell half
                assetOut: "FIAT",      // received fiat
                amountOut: 25000m,
                feeAsset: null,
                feeAmount: 0m,
                fiatValueAtTimestamp: 25000m,
                fiatValueAtTimestampCurrency: "SEK"
            );

            // Act
            calc.ProcessTransaction(tx);

            // Assert holdings updated
            var btcHoldings = calc.GetHoldings("BTC");
            Assert.NotNull(btcHoldings);
            Assert.Equal(0.5m, btcHoldings.Amount);
            Assert.Equal(25000m, btcHoldings.TotalCost); // remaining cost

            // Gain calculation
            decimal gain = calc.GetRealizedGain("BTC");
            Assert.Equal(0m, gain); // average cost = sell price for this example
        }

        [Fact]
        public void SwapCrypto_CalculatesGainAndAddsNewAsset()
        {
            // Arrange
            var calc = new AverageCostCalculator();

            // Add initial holdings
            calc.AddHolding("BTC", 1m, 50000m);

            var tx = new CanonicalTransaction(
                timestamp: new DateTime(2023, 01, 03),
                type: TransactionType.Trade,
                assetIn: "BTC",        // swap BTC for ETH
                amountIn: 0.5m,
                assetOut: "ETH",
                amountOut: 10m,
                feeAsset: null,
                feeAmount: 0m,
                fiatValueAtTimestamp: 25000m,
                fiatValueAtTimestampCurrency: "SEK"
            );

            // Act
            calc.ProcessTransaction(tx);

            // Assert
            var btcHoldings = calc.GetHoldings("BTC");
            Assert.Equal(0.5m, btcHoldings.Amount);
            Assert.Equal(25000m, btcHoldings.TotalCost);

            var ethHoldings = calc.GetHoldings("ETH");
            Assert.Equal(10m, ethHoldings.Amount);
            Assert.Equal(25000m, ethHoldings.TotalCost); // cost based on swapped BTC

            decimal gain = calc.GetRealizedGain("BTC");
            Assert.Equal(0m, gain); // simplified: no profit yet
        }
    }
}