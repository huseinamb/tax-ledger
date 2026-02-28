using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaxLedger.Domain.TaxEngine;

namespace TaxLedger.Domain.Reporting.Countries
{
    public class SwedishK4ReportGenerator : ITaxReportGenerator
    {
        public void Export(IEnumerable<TaxCalculationResult> results, string filePath)
        {
            // Group by asset (BTC, ETH, etc.) as required by Skatteverket K4 Section D
            var groupedResults = results
                .GroupBy(r => r.Asset)
                .Select(g => new
                {
                    Asset = g.Key,
                    TotalSalePrice = g.Sum(x => x.SalePrice),
                    TotalCostBasis = g.Sum(x => x.PurchasePrice),
                    // Note: In Sweden, you list Gains and Losses separately because
                    // losses are only 70% deductible.
                    TotalGain = g.Where(x => (x.SalePrice - x.PurchasePrice) > 0)
                                 .Sum(x => x.SalePrice - x.PurchasePrice),
                    TotalLoss = g.Where(x => (x.SalePrice - x.PurchasePrice) < 0)
                                 .Sum(x => Math.Abs(x.SalePrice - x.PurchasePrice))
                }).ToList();

            try
            {
                // Use Path.GetFullPath to show exactly where it's going
                string absolutePath = Path.GetFullPath(filePath);

                StringBuilder csvContent = new StringBuilder();
                csvContent.AppendLine("Beteckning (Asset);Försäljningspris;Omkostnadsbelopp;Vinst;Förlust");

                foreach (var summary in groupedResults)
                {
                    // Formatting to whole numbers (F0) as per Skatteverket preference
                    csvContent.AppendLine($"{summary.Asset};{summary.TotalSalePrice:F0};{summary.TotalCostBasis:F0};{summary.TotalGain:F0};{summary.TotalLoss:F0}");
                }

                File.WriteAllText(absolutePath, csvContent.ToString(), Encoding.UTF8);

                Console.WriteLine("\n========================================");
                Console.WriteLine($"SUCCESS! File created at:");
                Console.WriteLine(absolutePath); // Copy this path to find your file!
                Console.WriteLine("========================================\n");
                PrintToConsole(groupedResults);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Could not save file. {ex.Message}");
            }
        }

        private void PrintToConsole(dynamic groupedResults)
        {
            Console.WriteLine("\n--- SKATTEVERKET K4 REPORT SUMMARY (SECTION D) ---");
            Console.WriteLine($"{"Asset",-10} | {"Sale Price",-12} | {"Cost Basis",-12} | {"Profit",-10} | {"Loss",-10}");
            Console.WriteLine(new string('-', 65));

            foreach (var s in groupedResults)
            {
                Console.WriteLine($"{s.Asset,-10} | {s.TotalSalePrice,12:N0} | {s.TotalCostBasis,12:N0} | {s.TotalGain,10:N0} | {s.TotalLoss,10:N0}");
            }
        }
    }
}