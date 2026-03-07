using TaxLedger.Domain.TaxEngine.Strategies;
using TaxLedger.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Text.Json;

namespace TaxLedger.Tests.TaxEngine.Strategies
{
    public class SwedishAverageCostStrategyTests
    {
        [Theory]
        [MemberData(nameof(GetScenarios))]
        public void Calculate_Scenario_MatchesExpectedResults(TaxScenario scenario)
        {
            var strategy = new SwedishAverageCostStrategy();
            var results = strategy.Calculate(scenario.Transactions).ToList();

            // Group results by Asset to compare against our Dictionaries
            var actualGains = results
                .Where(r => (r.SalePrice - r.PurchasePrice) > 0)
                .GroupBy(r => r.Asset)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.SalePrice - r.PurchasePrice));

            var actualLosses = results
                .Where(r => (r.SalePrice - r.PurchasePrice) < 0)
                .GroupBy(r => r.Asset)
                .ToDictionary(g => g.Key, g => Math.Abs(g.Sum(r => r.SalePrice - r.PurchasePrice)));

            // Assert Gains
            foreach (var expected in scenario.ExpectedCapitalGains)
            {
                Assert.True(actualGains.ContainsKey(expected.Key), $"Missing gain for {expected.Key}");
                Assert.Equal(expected.Value, Math.Round(actualGains[expected.Key], 0));
            }

            // Assert Losses
            foreach (var expected in scenario.ExpectedCapitalLosses)
            {
                Assert.True(actualLosses.ContainsKey(expected.Key), $"Missing loss for {expected.Key}");
                Assert.Equal(expected.Value, Math.Round(actualLosses[expected.Key], 0));
            }
        }
        public static IEnumerable<object[]> GetScenarios()
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "Scenarios");
            var files = Directory.GetFiles(folder, "*.json");

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var scenario = JsonSerializer.Deserialize<TaxScenario>(json);
                yield return new object[] { scenario! };
            }
        }
        [Fact]
        public void Calculate_BtcToEthSwap_CorrectlyCalculatesGAVAndFees()
        {
            // Arrange
            var strategy = new SwedishAverageCostStrategy();
            var transactions = new List<CanonicalTransaction>
    {
        // 1. Buy 1 BTC for 500,000 SEK (BTC enters, SEK leaves)
        new CanonicalTransaction(
            timestamp: DateTime.Now.AddDays(-10),
            type: TransactionType.Trade,
            assetIn: "BTC", amountIn: 1m,        // BTC IN
            assetOut: "SEK", amountOut: 500000m,  // SEK OUT
            feeAsset: "SEK", feeAmount: 500m,
            fiatValueAtTimestamp: 500000m,
            fiatValueAtTimestampCurrency: "SEK"
        ),
        // 2. Swap 0.5 BTC for 10 ETH (BTC leaves, ETH enters)
        new CanonicalTransaction(
            timestamp: DateTime.Now.AddDays(-5),
            type: TransactionType.Trade,
            assetIn: "ETH", amountIn: 10m,        // ETH IN
            assetOut: "BTC", amountOut: 0.5m,     // BTC OUT
            feeAsset: "SEK", feeAmount: 200m,
            fiatValueAtTimestamp: 300000m,
            fiatValueAtTimestampCurrency: "SEK"
        )
    };

            // Act
            var results = strategy.Calculate(transactions).ToList();

            // Assert
            var btcResult = results.First(r => r.Asset == "BTC");

            // Math check:
            // Pool: 1 BTC at 500,500 total cost (500k + 500 fee). GAV = 500,500.
            // Sale: 0.5 BTC sold. CostBasis = 0.5 * 500,500 = 250,250.
            Assert.Equal(250250m, btcResult.PurchasePrice);

            // Sale Price = 300,000 SEK market value - 200 SEK fee = 299,800.
            Assert.Equal(299800m, btcResult.SalePrice);
        }
    }
}