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

        public void GenerateTaxReport(IEnumerable<CanonicalTransaction> transactions, string outputPath)
        {
            // 1. Run the math using the injected strategy (e.g., Swedish Average Cost)
            var results = _calculationStrategy.Calculate(transactions);

            // 2. Pass the results to the reporter (e.g., K4 Generator)
            _reportGenerator.Export(results, outputPath);
        }
    }
}