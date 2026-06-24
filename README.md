# TaxLedger

TaxLedger helps cryptocurrency traders generate accurate tax reports from their exchange transaction history. You export your trades from an exchange, TaxLedger prices each transaction at the correct market rate, applies the tax rules for your country, and produces a ready-to-review report.

Currently supports **Binance** exports and **Swedish tax regulations (K4 / Section D)**.

> ℹ️ **Note:** Tax calculation logic is verified against published Skatteverket examples and manually cross-verified using comprehensive, realistically simulated transaction datasets. As with any tax tool, always review the output carefully before filing. Live price data is fetched from Binance (crypto prices) and Frankfurter (forex rates, sourced from ECB) — small rounding differences may occur.

---

## What it does

1. **Parses** your Binance CSV export into a normalised transaction format
2. **Prices** each transaction in your local currency at the exact time it occurred — using Binance 1-minute klines for crypto prices and ECB rates via [Frankfurter](https://frankfurter.dev) for currency conversion
3. **Calculates** your taxable gains and losses using the correct method for your country
4. **Reports** a summary grouped by asset and a full transaction-level breakdown

For Sweden, this means applying *Genomsnittsmetoden* (GAV — average acquisition cost) and separating gains from losses as required by Skatteverket K4 Section D.

---

## Testing with Simulated Data

To demonstrate the application's capabilities without requiring real financial data, this repository includes an end-to-end data generation pipeline:

* **Pre-generated Samples:** You can find 10 pre-generated test files (`1_simulated_binance.csv` to `10_simulated_binance.csv`) inside the `\exchange_simulation` directory to test the tool immediately.
* **Data Generation Notebook:** The repository includes a Jupyter Notebook (`crypto_exchange_simulator.ipynb`) used to model algorithmic market trades and convert them seamlessly into the native Binance export format.

---

## Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js v18+](https://nodejs.org/)
- Internet connection (used to fetch historical prices and exchange rates — no API keys needed)

### 1. Clone the repository
```bash
git clone https://github.com/huseinamb/tax-ledger.git
cd tax-ledger
```

### 2. Start the backend API
```bash
cd TaxLedger
dotnet run --project TaxLedger.Api --launch-profile https
```
Note the HTTPS port shown in the terminal (e.g. `https://localhost:7148`).

### 3. Start the frontend
Open a second terminal:
```bash
cd frontend
npm install
npm run dev
```
Open your browser at `http://localhost:5173`.

### 4. Generate a report
1. Select your exchange (Binance) and country (Sweden)
2. Optionally enter a tax year — defaults to the latest year found in the data
3. Upload your Binance transaction history CSV export
4. Click **Generate Report**

The report shows a **summary by asset** (matching Skatteverket K4 Section D format) and a **full transaction breakdown** for manual verification.

### Run the tests
```bash
cd TaxLedger
dotnet test
```

### Explore the API with Swagger
With the backend running, open:
```
https://localhost:{port}/swagger
```

Available endpoints:
- `GET /api/exchanges` — returns supported exchanges
- `GET /api/countries` — returns supported countries
- `POST /api/report/json` — upload a CSV file and receive a tax report

---

## Supported exchanges and countries

| Exchange | Status |
|----------|--------|
| Binance  | ✅ Supported |
| Coinbase | — |
| Kraken   | — |

| Country | Tax method | Report format |
|---------|------------|---------------|
| Sweden  | GAV — Genomsnittsmetoden | K4 Section D |
| USA     | — | — |

---

## How it's built

The solution follows **Clean Architecture** — the tax engine has no knowledge of HTTP, files, or any specific exchange. Adding a new exchange is one new CSV adapter. Adding a new country is one new strategy class.

```
TaxLedger.Domain           # Transaction models, tax engine interfaces
TaxLedger.Application      # Pipeline orchestration, pricing contracts, factories
TaxLedger.Infrastructure   # Binance API, Frankfurter forex, price enrichment, factory implementations
TaxLedger.ExchangeAdapters # CSV parsers per exchange (currently Binance)
TaxLedger.Api              # ASP.NET Core Web API
TaxLedger.Tests            # xUnit unit and integration tests
frontend/                  # React + Vite single-page application
```

Prices are fetched from public endpoints only — no API keys are stored or required anywhere in the codebase.

---

## Known limitations & potential improvements

This project was built as a portfolio piece to demonstrate full-stack development with Clean Architecture, domain-driven design, and real API integration. The following areas are intentionally out of scope for now but represent natural next steps:

- **Additional exchanges** — the architecture supports new exchanges with one new adapter class. Coinbase and Kraken are natural candidates.
- **Additional countries** — adding a new country requires one new strategy class (e.g. US FIFO method).
- **CSV/PDF export** — the API currently returns JSON only. A downloadable K4 CSV or PDF would make the report directly usable for filing.
- **`.sru` export** — Skatteverket's digital filing format for direct submission.
- **Error handling** — the frontend shows basic error messages. A production app would benefit from more detailed feedback, especially around unsupported CSV formats.
