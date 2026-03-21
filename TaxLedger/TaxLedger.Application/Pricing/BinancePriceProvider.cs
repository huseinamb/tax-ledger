using System.Text.Json;
using TaxLedger.Application.Pricing;

namespace TaxLedger.Infrastructure.Pricing;

public sealed class BinancePriceProvider : ICryptoPriceProvider
{
    private readonly HttpClient _http;
    private readonly Dictionary<string, decimal> _cache = new();

    public BinancePriceProvider(HttpClient http)
    {
        _http = http;
    }

    // TODO: implement proper currency pair validation once all providers are added
    public bool Supports(string asset) => true;

    public async Task<decimal> GetPriceInUsdAsync(string asset, DateTime timestamp)
    {
        // Normalize to USDT pair e.g. BTC -> BTCUSDT
        var symbol = asset.EndsWith("USDT", StringComparison.OrdinalIgnoreCase)
            ? asset.ToUpper()
            : $"{asset.ToUpper()}USDT";

        // Cache key is symbol + minute-level precision
        var cacheKey = $"{symbol}|{timestamp:yyyy-MM-ddTHH:mm}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        var tsMs = new DateTimeOffset(timestamp, TimeSpan.Zero).ToUnixTimeMilliseconds();
        var url = $"https://api.binance.com/api/v3/klines?symbol={symbol}&interval=1m&startTime={tsMs}&limit=1";

        var resp = await _http.GetAsync(url);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        var klines = JsonSerializer.Deserialize<JsonElement[][]>(json);

        if (klines == null || klines.Length == 0)
            throw new InvalidOperationException(
                $"No kline data returned from Binance for {symbol} at {timestamp:u}");

        // Kline format: [openTime, open, high, low, close, ...]
        var close = decimal.Parse(klines[0][4].GetString()!);

        _cache[cacheKey] = close;
        return close;
    }
}