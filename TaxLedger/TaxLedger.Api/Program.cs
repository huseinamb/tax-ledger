using TaxLedger.Application;
using TaxLedger.Application.Pricing;
using TaxLedger.Domain.Reporting;
using TaxLedger.Domain.Reporting.Countries;
using TaxLedger.Domain.TaxEngine;
using TaxLedger.Domain.TaxEngine.Strategies;
using TaxLedger.ExchangeAdapters.Binance;
using TaxLedger.Infrastructure.Enrichment;
using TaxLedger.Infrastructure.Pricing;
using TaxLedger.Domain.Reporting.Countries;

// ── 1. Read CSV ────────────────────────────────────────────────────────────────
var csvPath = args.Length > 0
    ? args[0]
    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "1_simulated_binance.csv");

Console.WriteLine($"Reading CSV from: {csvPath}");
var rawRows = BinanceCsvReader.ReadFromFile(csvPath);

// ── 2. Parse into canonical transactions ──────────────────────────────────────
var parser = new BinanceTransactionParser();
var transactions = parser.Parse(rawRows).ToList();
Console.WriteLine($"Parsed {transactions.Count} transactions");

// ── 3. Enrich with fiat values ────────────────────────────────────────────────
var http = new HttpClient();
var cryptoProvider = new BinancePriceProvider(http);
var forexProvider = new FrankfurterForexProvider(http);
var enrichmentService = new PriceEnrichmentService(
    new[] { cryptoProvider },
    new[] { forexProvider }
);

Console.WriteLine("Enriching transactions with fiat values (calling Binance + Frankfurter)...");
var enriched = (await enrichmentService.EnrichAsync(transactions, "SEK")).ToList();
Console.WriteLine($"Enriched {enriched.Count} transactions");

// ── 4. Calculate tax ──────────────────────────────────────────────────────────
var strategy = new SwedishAverageCostStrategy();
var reporter = new SwedishK4ReportGenerator();
var taxService = new TaxService(strategy, reporter);

int targetYear = 2024;
var results = taxService.CalculateTax(enriched, targetYear).ToList();
Console.WriteLine($"\nTax results for {targetYear}: {results.Count} taxable events");

// ── 5. Print summary to console ───────────────────────────────────────────────
Console.WriteLine("\n--- PIPELINE OUTPUT ---");
Console.WriteLine($"{"Asset",-8} {"Sale Price (SEK)",18} {"Cost Basis (SEK)",18} {"Gain/Loss (SEK)",16}");
Console.WriteLine(new string('-', 65));

foreach (var r in results.OrderBy(r => r.OriginTransaction.Timestamp))
{
    var gainLoss = r.SalePrice - r.PurchasePrice;
    Console.WriteLine(
        $"{r.Asset,-8} {r.SalePrice,18:N0} {r.PurchasePrice,18:N0} {gainLoss,16:N0}");
}

Console.WriteLine(new string('-', 65));
Console.WriteLine($"{"TOTAL",-8} {results.Sum(r => r.SalePrice),18:N0} {results.Sum(r => r.PurchasePrice),18:N0} {results.Sum(r => r.GainLoss),16:N0}");