
namespace TaxLedger.Data
{
    public class SyntheticAsset
    {
        public string Name { get; }
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly decimal _minPrice;
        private readonly decimal _maxPrice;
        private readonly double _dailyRange;
        private readonly Dictionary<DateTime, decimal> _prices = new();
        private readonly Random _random = new();

        public SyntheticAsset(
            string name,
            DateTime startDate,
            DateTime endDate,
            decimal minPrice,
            decimal maxPrice,
            double dailyRange = 0.05) // 5% default for crypto volatility
        {
            Name = name;
            _startDate = startDate.Date;
            _endDate = endDate.Date;
            _minPrice = minPrice;
            _maxPrice = maxPrice;
            _dailyRange = dailyRange;

            GenerateData();
        }

        private void GenerateData()
        {
            int totalDays = (_endDate - _startDate).Days + 1;
            // Start at a random point between min/max
            decimal price = _minPrice + (decimal)_random.NextDouble() * (_maxPrice - _minPrice);

            for (int i = 0; i < totalDays; i++)
            {
                DateTime currentDate = _startDate.AddDays(i);
                _prices[currentDate] = Math.Round(price, 2);

                // Calculate next price
                double changePercent = (_random.NextDouble() * 2 - 1) * _dailyRange;
                decimal newPrice = price * (1 + (decimal)changePercent);

                // Reflective Boundaries (Simple version)
                if (newPrice > _maxPrice) newPrice = _maxPrice - (newPrice - _maxPrice);
                if (newPrice < _minPrice) newPrice = _minPrice + (_minPrice - newPrice);

                price = newPrice;
            }
        }

        public decimal GetPrice(DateTime date)
        {
            // If date is missing, return the closest available date
            if (_prices.TryGetValue(date.Date, out decimal price)) return price;
            return _prices.Values.Last();
        }
        public Dictionary<DateTime, decimal> GetPrices()
        {
            return _prices;
        }
    }
}