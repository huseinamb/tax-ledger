// 1. Get your data
using System.Formats.Asn1;
using System.Globalization;
using TaxLedger.Application;
using TaxLedger.Data;
using TaxLedger.Domain.Reporting.Countries;
using TaxLedger.Domain.TaxEngine.Strategies;
using TaxLedger.Domain.Transactions;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using TaxLedger.ExchangeAdapters.Binance;

var myTransactions = SyntheticDataset.GetSampleTransactions();

// 2. Setup for Sweden (This is where you'd use a Factory if supporting multiple countries)
var swedishStrategy = new SwedishAverageCostStrategy();
var swedishReporter = new SwedishK4ReportGenerator();

// 3. Initialize and run
var taxApp = new TaxService(swedishStrategy, swedishReporter);
taxApp.GenerateTaxReport(myTransactions,2024, "annual_tax_2024.txt");




// CSV file path
var path = "1_simulated_binance.csv";


using var reader = new StreamReader(path);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

var rawRows = csv.GetRecords<BinanceRawRow>().ToList();

// Parse
var parser = new BinanceTransactionParser();
var canonical = parser.Parse(rawRows);

Console.WriteLine($"Parsed {canonical.Count()} canonical transactions.");
foreach (var tx in canonical.Take(5))
{
    Console.WriteLine($"{tx.Timestamp} {tx.Type} {tx.AssetIn} {tx.AmountIn} -> {tx.AssetOut} {tx.AmountOut} Fee: {tx.FeeAmount} {tx.FeeAsset}");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

//var asset = new SyntheticAsset(
//           name: "SYNTH-BTC",
//           startDate: new DateTime(2025, 10, 1),
//           endDate: DateTime.Today,
//           minPrice: 20000,
//           maxPrice: 80000,
//           dailyRange: 0.02
//       );

//var btc = new SyntheticAsset("BTC", new DateTime(2023, 1, 1), new DateTime(2023, 12, 31), 200000m, 700000m);
//Console.WriteLine($"Today's price: {btc.GetPrice(new DateTime(2023, 1, 21))}");