using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

public class DateTimeExpressionTests
{
    private static Product MakeProduct(DateTime createdAt) =>
        new("Product", 10m, 1, true, createdAt, new List<string>());

    // 1. FutureDate — matches future dates
    [Fact]
    public void FutureDate_MatchesFutureDates_RejectsPast()
    {
        var filter = new ValiFlow<Product>()
            .FutureDate(p => p.CreatedAt)
            .Build();

        filter.Compile()(MakeProduct(DateTime.Now.AddDays(1))).Should().BeTrue();
        filter.Compile()(MakeProduct(DateTime.Now.AddYears(1))).Should().BeTrue();
        filter.Compile()(MakeProduct(DateTime.Now.AddDays(-1))).Should().BeFalse();
    }

    // 2. PastDate — matches past dates
    [Fact]
    public void PastDate_MatchesPastDates_RejectsFuture()
    {
        var filter = new ValiFlow<Product>()
            .PastDate(p => p.CreatedAt)
            .Build();

        filter.Compile()(MakeProduct(DateTime.Now.AddDays(-1))).Should().BeTrue();
        filter.Compile()(MakeProduct(DateTime.Now.AddYears(-1))).Should().BeTrue();
        filter.Compile()(MakeProduct(DateTime.Now.AddDays(1))).Should().BeFalse();
    }

    // 3. BetweenDates(start, end) — matches within range
    [Fact]
    public void BetweenDates_MatchesWithinRange_RejectsOutside()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);

        var filter = new ValiFlow<Product>()
            .BetweenDates(p => p.CreatedAt, start, end)
            .Build();

        filter.Compile()(MakeProduct(new DateTime(2024, 6, 15))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2024, 1, 1))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2024, 12, 31))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2023, 12, 31))).Should().BeFalse();
        filter.Compile()(MakeProduct(new DateTime(2025, 1, 1))).Should().BeFalse();
    }

    // 4. SameMonthAs(reference) — matches same month/year
    [Fact]
    public void SameMonthAs_MatchesSameMonthYear()
    {
        var reference = new DateTime(2025, 3, 15);

        var filter = new ValiFlow<Product>()
            .SameMonthAs(p => p.CreatedAt, reference)
            .Build();

        filter.Compile()(MakeProduct(new DateTime(2025, 3, 1))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2025, 3, 31))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2025, 4, 15))).Should().BeFalse();
        filter.Compile()(MakeProduct(new DateTime(2024, 3, 15))).Should().BeFalse();
    }

    // 5. SameYearAs(reference) — matches same year
    [Fact]
    public void SameYearAs_MatchesSameYear()
    {
        var reference = new DateTime(2025, 6, 1);

        var filter = new ValiFlow<Product>()
            .SameYearAs(p => p.CreatedAt, reference)
            .Build();

        filter.Compile()(MakeProduct(new DateTime(2025, 1, 1))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2025, 12, 31))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2024, 6, 1))).Should().BeFalse();
        filter.Compile()(MakeProduct(new DateTime(2026, 6, 1))).Should().BeFalse();
    }

    // 6. IsWeekend — matches Saturday/Sunday
    [Fact]
    public void IsWeekend_MatchesSaturdayAndSunday()
    {
        var filter = new ValiFlow<Product>()
            .IsWeekend(p => p.CreatedAt)
            .Build();

        // Find a Saturday and Sunday
        var saturday = GetNextDayOfWeek(DateTime.Today, DayOfWeek.Saturday);
        var sunday = GetNextDayOfWeek(DateTime.Today, DayOfWeek.Sunday);
        var monday = GetNextDayOfWeek(DateTime.Today, DayOfWeek.Monday);

        filter.Compile()(MakeProduct(saturday)).Should().BeTrue();
        filter.Compile()(MakeProduct(sunday)).Should().BeTrue();
        filter.Compile()(MakeProduct(monday)).Should().BeFalse();
    }

    // 7. IsWeekday — matches Mon-Fri
    [Fact]
    public void IsWeekday_MatchesMondayThroughFriday()
    {
        var filter = new ValiFlow<Product>()
            .IsWeekday(p => p.CreatedAt)
            .Build();

        var monday = GetNextDayOfWeek(DateTime.Today, DayOfWeek.Monday);
        var friday = GetNextDayOfWeek(DateTime.Today, DayOfWeek.Friday);
        var saturday = GetNextDayOfWeek(DateTime.Today, DayOfWeek.Saturday);

        filter.Compile()(MakeProduct(monday)).Should().BeTrue();
        filter.Compile()(MakeProduct(friday)).Should().BeTrue();
        filter.Compile()(MakeProduct(saturday)).Should().BeFalse();
    }

    // 8. IsDayOfWeek(DayOfWeek.Monday) — matches Mondays
    [Fact]
    public void IsDayOfWeek_Monday_MatchesMondaysOnly()
    {
        var filter = new ValiFlow<Product>()
            .IsDayOfWeek(p => p.CreatedAt, DayOfWeek.Monday)
            .Build();

        var monday = GetNextDayOfWeek(DateTime.Today, DayOfWeek.Monday);
        var tuesday = GetNextDayOfWeek(DateTime.Today, DayOfWeek.Tuesday);

        filter.Compile()(MakeProduct(monday)).Should().BeTrue();
        filter.Compile()(MakeProduct(tuesday)).Should().BeFalse();
    }

    // 9. IsInMonth(3) — matches March dates
    [Fact]
    public void IsInMonth_3_MatchesMarchDates()
    {
        var filter = new ValiFlow<Product>()
            .IsInMonth(p => p.CreatedAt, 3)
            .Build();

        filter.Compile()(MakeProduct(new DateTime(2025, 3, 15))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2024, 3, 1))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2025, 4, 15))).Should().BeFalse();
        filter.Compile()(MakeProduct(new DateTime(2025, 2, 28))).Should().BeFalse();
    }

    // 10. IsInYear(2025) — matches 2025 dates
    [Fact]
    public void IsInYear_2025_Matches2025Dates()
    {
        var filter = new ValiFlow<Product>()
            .IsInYear(p => p.CreatedAt, 2025)
            .Build();

        filter.Compile()(MakeProduct(new DateTime(2025, 1, 1))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2025, 12, 31))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2024, 6, 15))).Should().BeFalse();
        filter.Compile()(MakeProduct(new DateTime(2026, 1, 1))).Should().BeFalse();
    }

    // 11. IsBefore(someDate) — matches dates before
    [Fact]
    public void IsBefore_SomeDate_MatchesDatesBefore()
    {
        var cutoff = new DateTime(2025, 6, 1);

        var filter = new ValiFlow<Product>()
            .IsBefore(p => p.CreatedAt, cutoff)
            .Build();

        filter.Compile()(MakeProduct(new DateTime(2025, 5, 31))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2024, 1, 1))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2025, 6, 1))).Should().BeFalse();
        filter.Compile()(MakeProduct(new DateTime(2025, 6, 2))).Should().BeFalse();
    }

    // 12. IsAfter(someDate) — matches dates after
    [Fact]
    public void IsAfter_SomeDate_MatchesDatesAfter()
    {
        var cutoff = new DateTime(2025, 6, 1);

        var filter = new ValiFlow<Product>()
            .IsAfter(p => p.CreatedAt, cutoff)
            .Build();

        filter.Compile()(MakeProduct(new DateTime(2025, 6, 2))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2026, 1, 1))).Should().BeTrue();
        filter.Compile()(MakeProduct(new DateTime(2025, 6, 1))).Should().BeFalse();
        filter.Compile()(MakeProduct(new DateTime(2025, 5, 31))).Should().BeFalse();
    }

    [Fact]
    public void IsToday_DateTime_TodayValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().IsToday(p => p.CreatedAt).Build().Compile();
        filter(MakeProduct(DateTime.Today)).Should().BeTrue();
    }

    [Fact]
    public void IsToday_DateTime_YesterdayValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().IsToday(p => p.CreatedAt).Build().Compile();
        filter(MakeProduct(DateTime.Today.AddDays(-1))).Should().BeFalse();
    }

    [Fact]
    public void IsYesterday_DateTime_YesterdayValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().IsYesterday(p => p.CreatedAt).Build().Compile();
        filter(MakeProduct(DateTime.Today.AddDays(-1))).Should().BeTrue();
    }

    [Fact]
    public void IsYesterday_DateTime_TodayValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().IsYesterday(p => p.CreatedAt).Build().Compile();
        filter(MakeProduct(DateTime.Today)).Should().BeFalse();
    }

    [Fact]
    public void IsTomorrow_DateTime_TomorrowValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().IsTomorrow(p => p.CreatedAt).Build().Compile();
        filter(MakeProduct(DateTime.Today.AddDays(1))).Should().BeTrue();
    }

    [Fact]
    public void IsTomorrow_DateTime_TodayValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().IsTomorrow(p => p.CreatedAt).Build().Compile();
        filter(MakeProduct(DateTime.Today)).Should().BeFalse();
    }

    [Fact]
    public void IsLeapYear_DateTime_LeapYear2024_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().IsLeapYear(p => p.CreatedAt).Build().Compile();
        filter(MakeProduct(new DateTime(2024, 6, 15))).Should().BeTrue();
    }

    [Fact]
    public void IsLeapYear_DateTime_NonLeapYear2023_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().IsLeapYear(p => p.CreatedAt).Build().Compile();
        filter(MakeProduct(new DateTime(2023, 6, 15))).Should().BeFalse();
    }

    [Fact]
    public void InLastDays_DateTime_WithinRange_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().InLastDays(p => p.CreatedAt, 7).Build().Compile();
        filter(MakeProduct(DateTime.Today.AddDays(-3))).Should().BeTrue();
    }

    [Fact]
    public void InLastDays_DateTime_OutsideRange_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().InLastDays(p => p.CreatedAt, 7).Build().Compile();
        filter(MakeProduct(DateTime.Today.AddDays(-10))).Should().BeFalse();
    }

    [Fact]
    public void InNextDays_DateTime_WithinRange_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().InNextDays(p => p.CreatedAt, 7).Build().Compile();
        filter(MakeProduct(DateTime.Today.AddDays(3))).Should().BeTrue();
    }

    [Fact]
    public void InNextDays_DateTime_OutsideRange_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().InNextDays(p => p.CreatedAt, 7).Build().Compile();
        filter(MakeProduct(DateTime.Today.AddDays(10))).Should().BeFalse();
    }

    [Fact]
    public void BeforeDate_DateTime_DateBeforeCutoff_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().BeforeDate(p => p.CreatedAt, new DateTime(2025, 6, 15)).Build().Compile();
        filter(MakeProduct(new DateTime(2025, 6, 14, 23, 59, 0))).Should().BeTrue();
    }

    [Fact]
    public void BeforeDate_DateTime_SameDayCutoff_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().BeforeDate(p => p.CreatedAt, new DateTime(2025, 6, 15)).Build().Compile();
        filter(MakeProduct(new DateTime(2025, 6, 15, 8, 0, 0))).Should().BeFalse();
    }

    [Fact]
    public void AfterDate_DateTime_DateAfterCutoff_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().AfterDate(p => p.CreatedAt, new DateTime(2025, 6, 15)).Build().Compile();
        filter(MakeProduct(new DateTime(2025, 6, 16, 0, 0, 0))).Should().BeTrue();
    }

    [Fact]
    public void AfterDate_DateTime_SameDayCutoff_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().AfterDate(p => p.CreatedAt, new DateTime(2025, 6, 15)).Build().Compile();
        filter(MakeProduct(new DateTime(2025, 6, 15, 8, 0, 0))).Should().BeFalse();
    }

    [Fact]
    public void ExactDate_DateTime_SameDate_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().ExactDate(p => p.CreatedAt, new DateTime(2025, 6, 15)).Build().Compile();
        filter(MakeProduct(new DateTime(2025, 6, 15, 12, 0, 0))).Should().BeTrue();
    }

    [Fact]
    public void ExactDate_DateTime_DifferentDate_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().ExactDate(p => p.CreatedAt, new DateTime(2025, 6, 15)).Build().Compile();
        filter(MakeProduct(new DateTime(2025, 6, 16, 0, 0, 0))).Should().BeFalse();
    }

    private record DateRangeProduct(DateTime Value, DateTime Start, DateTime End);

    [Fact]
    public void BetweenDates_CrossProperty_DateTime_ValiFlow_MatchesWhenValueInRange()
    {
        var filter = new ValiFlow<DateRangeProduct>()
            .BetweenDates(e => e.Value, e => e.Start, e => e.End)
            .Build().Compile();

        filter(new DateRangeProduct(new DateTime(2026, 3, 15), new DateTime(2026, 3, 10), new DateTime(2026, 3, 20)))
            .Should().BeTrue();
        filter(new DateRangeProduct(new DateTime(2026, 3, 25), new DateTime(2026, 3, 10), new DateTime(2026, 3, 20)))
            .Should().BeFalse();
    }

    [Fact]
    public void InNextDays_TomorrowDate_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().InNextDays(p => p.CreatedAt, 5).Build();
        filter.Compile()(MakeProduct(DateTime.Today.AddDays(1))).Should().BeTrue();
    }

    [Fact]
    public void InNextDays_TodayDate_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().InNextDays(p => p.CreatedAt, 5).Build();
        filter.Compile()(MakeProduct(DateTime.Today)).Should().BeFalse();
    }

    [Fact]
    public void InLastDays_YesterdayDate_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().InLastDays(p => p.CreatedAt, 5).Build();
        filter.Compile()(MakeProduct(DateTime.Today.AddDays(-1))).Should().BeTrue();
    }

    [Fact]
    public void InLastDays_TodayDate_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().InLastDays(p => p.CreatedAt, 5).Build();
        filter.Compile()(MakeProduct(DateTime.Today)).Should().BeFalse();
    }

    private static DateTime GetNextDayOfWeek(DateTime from, DayOfWeek dayOfWeek)
    {
        int daysUntil = ((int)dayOfWeek - (int)from.DayOfWeek + 7) % 7;
        if (daysUntil == 0) daysUntil = 7;
        return from.AddDays(daysUntil).Date;
    }
}
