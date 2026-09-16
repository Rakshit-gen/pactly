namespace Pactly.Core.Services;

public record ProrationResult(int CreditCents, int ChargeCents)
{
    public int NetCents => ChargeCents - CreditCents;
}

public class ProrationService
{
    public ProrationResult Calculate(
        int oldMonthlyTotalCents,
        int newMonthlyTotalCents,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        DateTimeOffset now)
    {
        var totalDays = (periodEnd - periodStart).TotalDays;
        if (totalDays <= 0)
        {
            return new ProrationResult(0, newMonthlyTotalCents);
        }

        var daysRemaining = Math.Clamp((periodEnd - now).TotalDays, 0, totalDays);

        var oldDailyRate = oldMonthlyTotalCents / totalDays;
        var newDailyRate = newMonthlyTotalCents / totalDays;

        var creditCents = (int)Math.Round(oldDailyRate * daysRemaining, MidpointRounding.AwayFromZero);
        var chargeCents = (int)Math.Round(newDailyRate * daysRemaining, MidpointRounding.AwayFromZero);

        return new ProrationResult(creditCents, chargeCents);
    }
}
