using TaxLedger.Infrastructure.Pricing;
using Xunit;

namespace TaxLedger.Tests.Integration;

public class BinancePriceProviderTests
{
    private readonly BinancePriceProvider _provider;

    public BinancePriceProviderTests()
    {
        var httpClient = new HttpClient();
        _provider = new BinancePriceProvider(httpClient);
    }

    [Fact]
    public async Task GetPriceInUsdAsync_BtcAtKnownTimestamp_ReturnsReasonablePrice()
    {
        // Arrange
        // BTC was trading around 70,000-73,000 USD in mid-March 2024
        var timestamp = new DateTime(2024, 3, 14, 10, 32, 0, DateTimeKind.Utc);

        // Act
        var price = await _provider.GetPriceInUsdAsync("BTC", timestamp);

        // Assert
        Assert.True(price > 50_000m && price < 100_000m,
            $"Expected BTC price between 50,000 and 100,000 USD, but got {price}");
    }

    [Fact]
    public async Task GetPriceInUsdAsync_EthAtKnownTimestamp_ReturnsReasonablePrice()
    {
        // Arrange
        // ETH was trading around 3,800-4,000 USD in mid-March 2024
        var timestamp = new DateTime(2024, 3, 14, 10, 32, 0, DateTimeKind.Utc);

        // Act
        var price = await _provider.GetPriceInUsdAsync("ETH", timestamp);

        // Assert
        Assert.True(price > 2_000m && price < 6_000m,
            $"Expected ETH price between 2,000 and 6,000 USD, but got {price}");
    }

    [Fact]
    public async Task GetPriceInUsdAsync_SameAssetAndTimestamp_ReturnsCachedResult()
    {
        // Arrange
        var timestamp = new DateTime(2024, 3, 14, 10, 32, 0, DateTimeKind.Utc);

        // Act — call twice, second should hit cache
        var price1 = await _provider.GetPriceInUsdAsync("BTC", timestamp);
        var price2 = await _provider.GetPriceInUsdAsync("BTC", timestamp);

        // Assert
        Assert.Equal(price1, price2);
    }
}