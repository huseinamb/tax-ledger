using TaxLedger.Application.Pricing;
using TaxLedger.Domain.Transactions;

namespace TaxLedger.Infrastructure.Enrichment;

public sealed class PriceEnrichmentService : IPriceEnrichmentService
{
    private readonly IEnumerable<ICryptoPriceProvider> _cryptoProviders;
    private readonly IEnumerable<IForexRateProvider> _forexProviders;

    private static readonly HashSet<string> FiatAndStablecoins = new(StringComparer.OrdinalIgnoreCase)
    {
        "USDT", "USDC", "BUSD", "DAI", "USD", "EUR", "SEK", "GBP"
    };

    public PriceEnrichmentService(
        IEnumerable<ICryptoPriceProvider> cryptoProviders,
        IEnumerable<IForexRateProvider> forexProviders)
    {
        _cryptoProviders = cryptoProviders;
        _forexProviders = forexProviders;
    }

    public async Task<IEnumerable<CanonicalTransaction>> EnrichAsync(
        IEnumerable<CanonicalTransaction> transactions,
        string targetFiatCurrency)
    {
        var list = transactions.ToList();

        // Prefetch all forex rates for the date range in one request
        var uniqueDates = list.Select(t => t.Timestamp).Distinct().ToList();
        await PrefetchForexAsync("USD", targetFiatCurrency, uniqueDates);

        var enriched = new List<CanonicalTransaction>();
        foreach (var tx in list)
            enriched.Add(await EnrichSingleAsync(tx, targetFiatCurrency));

        return enriched;
    }

    private async Task<CanonicalTransaction> EnrichSingleAsync(
        CanonicalTransaction tx,
        string targetFiatCurrency)
    {
        decimal valueUsd = await GetTransactionValueUsdAsync(tx);
        decimal valueFiat = await ConvertToTargetFiatAsync(valueUsd, targetFiatCurrency, tx.Timestamp);
        decimal feeValueFiat = await GetFeeValueFiatAsync(tx, targetFiatCurrency);

        return new CanonicalTransaction(
            timestamp: tx.Timestamp,
            type: tx.Type,
            assetIn: tx.AssetIn,
            amountIn: tx.AmountIn,
            assetOut: tx.AssetOut,
            amountOut: tx.AmountOut,
            feeAsset: targetFiatCurrency,
            feeAmount: feeValueFiat,   // now in fiat, not crypto units
            fiatValueAtTimestamp: valueFiat,
            fiatValueAtTimestampCurrency: targetFiatCurrency
        );
    }

    private async Task<decimal> GetTransactionValueUsdAsync(CanonicalTransaction tx)
    {
        switch (tx.Type)
        {
            case TransactionType.Trade:
                return await GetTradeValueUsdAsync(tx);

            case TransactionType.Deposit:
            case TransactionType.Withdrawal:
            case TransactionType.Transfer:
                return await GetSingleAssetValueUsdAsync(tx);

            default:
                return 0m;
        }
    }

    private async Task<decimal> GetTradeValueUsdAsync(CanonicalTransaction tx)
    {
        var assetIn = tx.AssetIn;
        var assetOut = tx.AssetOut;

        bool assetInIsFiat = assetIn != null && FiatAndStablecoins.Contains(assetIn);
        bool assetOutIsFiat = assetOut != null && FiatAndStablecoins.Contains(assetOut);

        // Case 1: Received crypto, spent fiat/stablecoin (e.g. AssetIn=BTC, AssetOut=EUR)
        if (!assetInIsFiat && assetOutIsFiat)
            return await NormalizeToUsdAsync(assetOut!, tx.AmountOut, tx.Timestamp);

        // Case 2: Received fiat/stablecoin, spent crypto (e.g. AssetIn=USDT, AssetOut=BTC)
        if (assetInIsFiat && !assetOutIsFiat)
            return await NormalizeToUsdAsync(assetIn!, tx.AmountIn, tx.Timestamp);

        // Case 3: Crypto → Crypto swap (e.g. AssetIn=ETH, AssetOut=BTC)
        if (!assetInIsFiat && !assetOutIsFiat && assetOut != null)
        {
            var price = await GetCryptoPriceUsdAsync(assetOut, tx.Timestamp);
            return price * tx.AmountOut;
        }

        return 0m;
    }

    private async Task<decimal> GetSingleAssetValueUsdAsync(CanonicalTransaction tx)
    {
        var asset = tx.AssetIn ?? tx.AssetOut;
        if (asset == null) return 0m;

        var amount = tx.AmountIn > 0 ? tx.AmountIn : tx.AmountOut;

        if (FiatAndStablecoins.Contains(asset))
            return await NormalizeToUsdAsync(asset, amount, tx.Timestamp);

        var price = await GetCryptoPriceUsdAsync(asset, tx.Timestamp);
        return price * amount;
    }

    private async Task<decimal> GetFeeValueFiatAsync(
      CanonicalTransaction tx,
      string targetFiatCurrency)
    {
        if (tx.FeeAsset == null || tx.FeeAmount == 0)
            return 0m;

        decimal feeUsd = FiatAndStablecoins.Contains(tx.FeeAsset)
            ? await NormalizeToUsdAsync(tx.FeeAsset, tx.FeeAmount, tx.Timestamp)
            : await GetCryptoPriceUsdAsync(tx.FeeAsset, tx.Timestamp) * tx.FeeAmount;

        return await ConvertToTargetFiatAsync(feeUsd, targetFiatCurrency, tx.Timestamp);
    }

    private async Task<decimal> GetCryptoPriceUsdAsync(string asset, DateTime timestamp)
    {
        var provider = _cryptoProviders.FirstOrDefault(p => p.Supports(asset))
            ?? throw new InvalidOperationException(
                $"No crypto price provider supports asset '{asset}'");

        return await provider.GetPriceInUsdAsync(asset, timestamp);
    }

    private async Task<decimal> ConvertToTargetFiatAsync(
        decimal valueUsd,
        string targetFiatCurrency,
        DateTime timestamp)
    {
        if (targetFiatCurrency.Equals("USD", StringComparison.OrdinalIgnoreCase))
            return valueUsd;

        var forexProvider = _forexProviders.FirstOrDefault(
            p => p.Supports("USD", targetFiatCurrency))
            ?? throw new InvalidOperationException(
                $"No forex provider supports USD → {targetFiatCurrency}");

        var rate = await forexProvider.GetRateAsync("USD", targetFiatCurrency, timestamp);
        return valueUsd * rate;
    }

    private Task PrefetchForexAsync(
        string fromCurrency,
        string toCurrency,
        IEnumerable<DateTime> dates)
    {
        var provider = _forexProviders.FirstOrDefault(p => p.Supports(fromCurrency, toCurrency));
        if (provider == null) return Task.CompletedTask;
        return provider.PrefetchAsync(fromCurrency, toCurrency, dates);
    }
    private async Task<decimal> NormalizeToUsdAsync(string asset, decimal amount, DateTime timestamp)
    {
        // Already USD or stablecoin pegged to USD
        if (asset.Equals("USD", StringComparison.OrdinalIgnoreCase) ||
            asset.Equals("USDT", StringComparison.OrdinalIgnoreCase) ||
            asset.Equals("USDC", StringComparison.OrdinalIgnoreCase) ||
            asset.Equals("BUSD", StringComparison.OrdinalIgnoreCase) ||
            asset.Equals("DAI", StringComparison.OrdinalIgnoreCase))
            return amount;

        // Non-USD fiat (EUR, SEK, GBP etc.) — convert to USD via forex
        var forexProvider = _forexProviders.FirstOrDefault(p => p.Supports(asset, "USD"))
            ?? throw new InvalidOperationException(
                $"No forex provider supports {asset} → USD");

        var rate = await forexProvider.GetRateAsync(asset, "USD", timestamp);
        return amount * rate;
    }
}