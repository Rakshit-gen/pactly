using Pactly.Core.Services;
using Xunit;

namespace Pactly.Tests.Services;

public class ProrationServiceTests
{
    private readonly ProrationService _service = new();

    [Fact]
    public void Calculate_HalfwayThroughCycle_CreditsHalfOfOldPlan()
    {
        var periodStart = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero);
        var now = new DateTimeOffset(2026, 1, 16, 0, 0, 0, TimeSpan.Zero);

        var result = _service.Calculate(3000, 6000, periodStart, periodEnd, now);

        Assert.Equal(1500, result.CreditCents);
        Assert.Equal(3000, result.ChargeCents);
        Assert.Equal(1500, result.NetCents);
    }

    [Fact]
    public void Calculate_AtStartOfCycle_CreditsNearlyFullOldPlan()
    {
        var periodStart = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero);

        var result = _service.Calculate(3000, 6000, periodStart, periodEnd, periodStart);

        Assert.Equal(3000, result.CreditCents);
        Assert.Equal(6000, result.ChargeCents);
    }

    [Fact]
    public void Calculate_AtEndOfCycle_CreditsNothing()
    {
        var periodStart = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero);

        var result = _service.Calculate(3000, 6000, periodStart, periodEnd, periodEnd);

        Assert.Equal(0, result.CreditCents);
        Assert.Equal(0, result.ChargeCents);
    }

    [Fact]
    public void Calculate_Downgrade_ProducesNegativeNetCents()
    {
        var periodStart = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero);
        var now = new DateTimeOffset(2026, 1, 16, 0, 0, 0, TimeSpan.Zero);

        var result = _service.Calculate(6000, 3000, periodStart, periodEnd, now);

        Assert.True(result.NetCents < 0);
    }

    [Fact]
    public void Calculate_ZeroLengthPeriod_ChargesFullNewPriceWithNoCredit()
    {
        var moment = DateTimeOffset.UtcNow;

        var result = _service.Calculate(3000, 6000, moment, moment, moment);

        Assert.Equal(0, result.CreditCents);
        Assert.Equal(6000, result.ChargeCents);
    }
}
