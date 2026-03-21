using System.Globalization;

namespace TaxLedger.ExchangeAdapters.Binance;

/// <summary>
/// Reads a Binance transaction history CSV export into BinanceRawRow objects.
/// Handles scientific notation (e.g. -2.66e-06) and invariant culture decimal parsing.
/// </summary>
public static class BinanceCsvReader
{
    public static IEnumerable<BinanceRawRow> ReadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Binance CSV file not found: {filePath}");

        var lines = File.ReadAllLines(filePath);

        if (lines.Length < 2)
            throw new InvalidOperationException("CSV file is empty or has no data rows.");

        ValidateHeader(lines[0].Split(','));

        var rows = new List<BinanceRawRow>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split(',');
            if (cols.Length < 5)
            {
                Console.WriteLine($"Warning: skipping malformed row at line {i + 1}: {line}");
                continue;
            }

            rows.Add(new BinanceRawRow
            {
                Time = DateTime.Parse(cols[0].Trim(), CultureInfo.InvariantCulture),
                Account = cols[1].Trim(),
                Operation = cols[2].Trim(),
                Coin = cols[3].Trim(),
                Change = double.Parse(cols[4].Trim(), CultureInfo.InvariantCulture)
            });
        }

        return rows;
    }

    private static void ValidateHeader(string[] header)
    {
        var expected = new[] { "Time", "Account", "Operation", "Coin", "Change" };
        for (int i = 0; i < expected.Length; i++)
        {
            if (!header[i].Trim().Equals(expected[i], StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Unexpected CSV header at column {i + 1}. " +
                    $"Expected '{expected[i]}' but got '{header[i].Trim()}'. " +
                    $"Is this a valid Binance export?");
        }
    }
}