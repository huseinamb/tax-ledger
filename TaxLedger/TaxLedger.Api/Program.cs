using TaxLedger.Application;
using TaxLedger.Application.Factories;
using TaxLedger.Application.Pricing;
using TaxLedger.Domain.Reporting;
using TaxLedger.Domain.TaxEngine;
using TaxLedger.Infrastructure.Enrichment;
using TaxLedger.Infrastructure.Factories;
using TaxLedger.Infrastructure.Pricing;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HTTP client
builder.Services.AddHttpClient();

// Pricing providers
builder.Services.AddSingleton<ICryptoPriceProvider, BinancePriceProvider>();
builder.Services.AddSingleton<IForexRateProvider, FrankfurterForexProvider>();
builder.Services.AddSingleton<IPriceEnrichmentService, PriceEnrichmentService>();

// Factories
builder.Services.AddSingleton<IExchangeParserFactory, ExchangeParserFactory>();
builder.Services.AddSingleton<ICountryStrategyFactory, CountryStrategyFactory>();

// Tax service
//builder.Services.AddScoped<TaxService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend");

// ── Middleware ─────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();