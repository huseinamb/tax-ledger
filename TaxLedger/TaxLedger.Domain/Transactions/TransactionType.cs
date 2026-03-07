using System.Text.Json.Serialization;

namespace TaxLedger.Domain.Transactions
{
    [JsonConverter(typeof(JsonStringEnumConverter))] // Add this attribute
    public enum TransactionType
    {
        Trade,
        Deposit,
        Withdrawal,
        Fee,
        Transfer
    }
}