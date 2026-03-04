using TaxLedger.Data;
using Xunit;

namespace TaxLedger.Tests.Data
{
    public class SyntheticAssetTest
    {
        [Fact]
        public void Valid_prices_StayWithinBoundaries()
        {
            // Arrange
            decimal minPrice = 200000m;
            decimal maxPrice = 700000m;
            DateTime start = new DateTime(2023, 1, 1);
            DateTime end = new DateTime(2023, 12, 31);

            var btc = new SyntheticAsset("BTC", start, end, minPrice, maxPrice);

            // Act
            // Assuming GetPrices() returns the Dictionary<DateTime, decimal>
            var prices = btc.GetPrices().Values;

            // Assert
            // 1. Ensure we generated exactly 365 days of data
            int expectedDays = (end - start).Days + 1;
            Assert.Equal(expectedDays, prices.Count);

            // 2. Ensure NO price is below the minimum
            Assert.All(prices, p => Assert.True(p >= minPrice, $"Price {p} was below minimum {minPrice}"));

            // 3. Ensure NO price is above the maximum
            Assert.All(prices, p => Assert.True(p <= maxPrice, $"Price {p} was above maximum {maxPrice}"));

        }
    }
}