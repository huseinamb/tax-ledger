using System.Text.Json;
using TaxLedger.Application.Pricing;

namespace TaxLedger.Infrastructure.Pricing;

public sealed class FrankfurterForexProvider : IForexRateProvider
{
    private readonly HttpClient _http;
    // Cache key format: "USD|SEK|2024-03-14"
    private readonly Dictionary<string, decimal> _cache = new();

    public FrankfurterForexProvider(HttpClient http)
    {
        _http = http;
    }

    // TODO: implement proper currency pair validation once all providers are added
    public bool Supports(string fromCurrency, string toCurrency) => true;

    public async Task<decimal> GetRateAsync(string fromCurrency, string toCurrency, DateTime date)
    {
        var cacheKey = $"{fromCurrency}|{toCurrency}|{date:yyyy-MM-dd}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        var url = $"https://api.frankfurter.dev/v1/{date:yyyy-MM-dd}?base={fromCurrency}&symbols={toCurrency}";
        var resp = await _http.GetAsync(url);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        var doc = JsonSerializer.Deserialize<JsonElement>(json);

        var rate = doc.GetProperty("rates").GetProperty(toCurrency).GetDecimal();
        var actualDate = doc.GetProperty("date").GetString()!;

        // Cache by actual date returned (may differ from requested date)
        _cache[$"{fromCurrency}|{toCurrency}|{actualDate}"] = rate;
        return rate;
    }

    public async Task PrefetchAsync(string fromCurrency, string toCurrency, IEnumerable<DateTime> dates)
    {
        var sorted = dates.OrderBy(d => d).ToList();
        if (!sorted.Any()) return;

        var start = sorted.First().Date.ToString("yyyy-MM-dd");
        var end = sorted.Last().Date.ToString("yyyy-MM-dd");

        var url = $"https://api.frankfurter.dev/v1/{start}..{end}?base={fromCurrency}&symbols={toCurrency}";
        var resp = await _http.GetAsync(url);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        var doc = JsonSerializer.Deserialize<JsonElement>(json);

        if (!doc.TryGetProperty("rates", out var allRates))
            return;

        foreach (var dateEntry in allRates.EnumerateObject())
        {
            if (dateEntry.Value.TryGetProperty(toCurrency, out var rateEl))
            {
                var cacheKey = $"{fromCurrency}|{toCurrency}|{dateEntry.Name}";
                _cache[cacheKey] = rateEl.GetDecimal();
            }
        }
    }
}