using TaxLedger.Infrastructure.Pricing;
using Xunit;

namespace TaxLedger.Tests.Integration;

public class FrankfurterForexProviderTests
{
    private readonly FrankfurterForexProvider _provider;

    public FrankfurterForexProviderTests()
    {
        var httpClient = new HttpClient();
        _provider = new FrankfurterForexProvider(httpClient);
    }

    [Fact]
    public async Task GetRateAsync_UsdToSek_ReturnsReasonableRate()
    {
        // Arrange
        // USD/SEK was around 10.3-10.6 in March 2024
        var date = new DateTime(2024, 3, 14, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var rate = await _provider.GetRateAsync("USD", "SEK", date);

        // Assert
        Assert.True(rate > 9m && rate < 11m,
            $"Expected USD/SEK between 9 and 11, but got {rate}");
    }

    [Fact]
    public async Task GetRateAsync_Weekend_ReturnsFridayRate()
    {
        // Arrange — March 16 2024 is a Saturday
        var saturday = new DateTime(2024, 3, 16, 0, 0, 0, DateTimeKind.Utc);

        // Act — Frankfurter should return the nearest available rate (Friday)
        var rate = await _provider.GetRateAsync("USD", "SEK", saturday);

        // Assert — should still return a valid rate, not throw
        Assert.True(rate > 9m && rate < 11m,
            $"Expected valid USD/SEK rate for weekend date, but got {rate}");
    }

    [Fact]
    public async Task PrefetchAsync_FullYear_PopulatesCacheCorrectly()
    {
        // Arrange — a few dates spread across 2024
        var dates = new List<DateTime>
    {
        new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2024, 12, 15, 0, 0, 0, DateTimeKind.Utc),
    };

        // Act — prefetch all dates in a single request
        await _provider.PrefetchAsync("USD", "SEK", dates);

        // Assert — valid rates are returned for all prefetched dates
        // Note: this does not verify cache usage, only that rates are available after prefetch
        // TODO: use a mocked HttpClient to assert only one HTTP call was made
        var rate = await _provider.GetRateAsync("USD", "SEK", dates[0]);
        Assert.True(rate > 8m && rate < 15m,
            $"Expected valid USD/SEK rate after prefetch, but got {rate}");
    }
}