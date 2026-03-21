using TaxLedger.Domain.Transactions;
using TaxLedger.Infrastructure.Enrichment;
using TaxLedger.Infrastructure.Pricing;
using Xunit;

namespace TaxLedger.Tests.Integration;

public class PriceEnrichmentServiceTests
{
    private readonly PriceEnrichmentService _service;

    public PriceEnrichmentServiceTests()
    {
        var httpClient = new HttpClient();
        var cryptoProvider = new BinancePriceProvider(httpClient);
        var forexProvider = new FrankfurterForexProvider(httpClient);

        _service = new PriceEnrichmentService(
            new[] { cryptoProvider },
            new[] { forexProvider }
        );
    }

    [Fact]
    public async Task EnrichAsync_BtcBuyWithUsdt_PopulatesFiatValueInSek()
    {
        // Arrange — buy 0.1 BTC with USDT
        var transactions = new List<CanonicalTransaction>
        {
            new CanonicalTransaction(
                timestamp: new DateTime(2024, 3, 14, 10, 32, 0, DateTimeKind.Utc),
                type: TransactionType.Trade,
                assetIn: "BTC",
                amountIn: 0.1m,
                assetOut: "USDT",
                amountOut: 7200m,
                feeAsset: "BTC",
                feeAmount: 0.0001m,
                fiatValueAtTimestamp: 0m,      // unenriched
                fiatValueAtTimestampCurrency: "USD"
            )
        };

        // Act
        var enriched = (await _service.EnrichAsync(transactions, "SEK")).ToList();

        // Assert
        var tx = enriched[0];
        Assert.Equal("SEK", tx.FiatValueAtTimestampCurrency);
        Assert.True(tx.FiatValueAtTimestamp > 0,
            "Expected fiat value to be populated after enrichment");
        Assert.Equal("SEK", tx.FeeAsset);
        Assert.True(tx.FeeAmount > 0,
            "Expected fee to be converted to SEK after enrichment");
    }

    [Fact]
    public async Task EnrichAsync_BtcDeposit_PopulatesFiatValueInSek()
    {
        // Arrange — deposit 0.5 BTC
        var transactions = new List<CanonicalTransaction>
        {
            new CanonicalTransaction(
                timestamp: new DateTime(2024, 3, 14, 10, 32, 0, DateTimeKind.Utc),
                type: TransactionType.Deposit,
                assetIn: "BTC",
                amountIn: 0.5m,
                assetOut: null,
                amountOut: 0m,
                feeAsset: null,
                feeAmount: 0m,
                fiatValueAtTimestamp: 0m,
                fiatValueAtTimestampCurrency: "USD"
            )
        };

        // Act
        var enriched = (await _service.EnrichAsync(transactions, "SEK")).ToList();

        // Assert
        var tx = enriched[0];
        Assert.Equal("SEK", tx.FiatValueAtTimestampCurrency);
        Assert.True(tx.FiatValueAtTimestamp > 0,
            "Expected deposit fiat value to be populated after enrichment");
    }

    [Fact]
    public async Task EnrichAsync_CryptoCryptoSwap_PopulatesFiatValueInSek()
    {
        // Arrange — swap 1 ETH for some BTC
        var transactions = new List<CanonicalTransaction>
        {
            new CanonicalTransaction(
                timestamp: new DateTime(2024, 3, 14, 10, 32, 0, DateTimeKind.Utc),
                type: TransactionType.Trade,
                assetIn: "ETH",
                amountIn: 1m,
                assetOut: "BTC",
                amountOut: 0.05m,
                feeAsset: "BNB",
                feeAmount: 0.01m,
                fiatValueAtTimestamp: 0m,
                fiatValueAtTimestampCurrency: "USD"
            )
        };

        // Act
        var enriched = (await _service.EnrichAsync(transactions, "SEK")).ToList();

        // Assert
        var tx = enriched[0];
        Assert.Equal("SEK", tx.FiatValueAtTimestampCurrency);
        Assert.True(tx.FiatValueAtTimestamp > 0,
            "Expected swap fiat value to be populated after enrichment");
        Assert.Equal("SEK", tx.FeeAsset);
        Assert.True(tx.FeeAmount > 0,
            "Expected BNB fee to be converted to SEK");
    }
}