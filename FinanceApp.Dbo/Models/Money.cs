using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Dbo.Models;

[Owned]
public class Money
{
    public decimal Amount { get; private set; } = 0m;
    public string Currency { get; private set; } = "RUB";

    private Money() { } // для EF
    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be ISO-4217 (3 chars).");

        Amount   = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    public static Money From(decimal amount, string currency) => new(amount, currency);
    public Money Add(Money other)
    {
        if (!Currency.Equals(other.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Currencies must match.");
        return new Money(Amount + other.Amount, Currency);
    }
}