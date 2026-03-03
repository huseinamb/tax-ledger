using System.Collections.Generic;
using TaxLedger.Domain.Transactions;
using TaxLedger.Domain.TaxEngine;
using TaxLedger.Domain.Reporting;

namespace TaxLedger.Application
{
    public class TaxService
    {
        private readonly ITaxCalculationStrategy _calculationStrategy;
        private readonly ITaxReportGenerator _reportGenerator;

        // Constructor Injection allows you to swap these out for different countries
        public TaxService(ITaxCalculationStrategy strategy, ITaxReportGenerator reporter)
        {
            _calculationStrategy = strategy;
            _reportGenerator = reporter;
        }

        public void GenerateTaxReport(IEnumerable<CanonicalTransaction> transactions, int targetYear, string outputPath)
        {
            // Step 1: Filter to all history up to the end of the target year
            var historicalData = transactions
                .Where(t => t.Timestamp.Year <= targetYear)
                .OrderBy(t => t.Timestamp);

            // Step 2: Strategy calculates EVERYTHING (Builds the GAV pool correctly)
            var allResults = _calculationStrategy.Calculate(historicalData);

            // Step 3: Service slices the results for the specific tax year
            var taxYearResults = allResults
                .Where(r => r.OriginTransaction.Timestamp.Year == targetYear)
                .ToList();

            _reportGenerator.Export(taxYearResults, outputPath);
        }
    }
}