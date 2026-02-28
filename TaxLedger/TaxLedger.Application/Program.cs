// 1. Get your data
using TaxLedger.Application;
using TaxLedger.Domain.Reporting.Countries;
using TaxLedger.Domain.TaxEngine.Strategies;
using TaxLedger.Domain.Transactions;

var myTransactions = SyntheticDataset.GetSampleTransactions();

// 2. Setup for Sweden (This is where you'd use a Factory if supporting multiple countries)
var swedishStrategy = new SwedishAverageCostStrategy();
var swedishReporter = new SwedishK4ReportGenerator();

// 3. Initialize and run
var taxApp = new TaxService(swedishStrategy, swedishReporter);
taxApp.GenerateTaxReport(myTransactions, "annual_tax_2024.txt");