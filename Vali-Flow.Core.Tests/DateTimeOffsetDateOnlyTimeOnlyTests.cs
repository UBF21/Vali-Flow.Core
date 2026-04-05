using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

public record Event(
    DateTimeOffset StartOffset, DateTimeOffset EndOffset,
    DateOnly EventDate, DateOnly DeadlineDate,
    TimeOnly StartTime, TimeOnly EndTime,
    string Name, bool IsActive);

public class DateTimeOffsetDateOnlyTimeOnlyTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Event MakeEvent(
        DateTimeOffset? startOffset = null,
        DateTimeOffset? endOffset = null,
        DateOnly? eventDate = null,
        DateOnly? deadlineDate = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        string name = "Test Event",
        bool isActive = true)
        => new(
            startOffset ?? DateTimeOffset.UtcNow,
            endOffset ?? DateTimeOffset.UtcNow.AddDays(1),
            eventDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            deadlineDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            startTime ?? new TimeOnly(9, 0),
            endTime ?? new TimeOnly(17, 0),
            name,
            isActive);

    // ═══════════════════════════════════════════════════════════════════════
    // DateTimeOffset Tests
    // ═══════════════════════════════════════════════════════════════════════

    // 1. FutureDate: future value → true
    [Fact]
    public void DateTimeOffset_FutureDate_FutureValue_ReturnsTrue()
    {
        var future = DateTimeOffset.UtcNow.AddDays(1);
        var filter = new ValiFlow<Event>().FutureDate(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: future)).Should().BeTrue();
    }

    // 2. FutureDate: past value → false
    [Fact]
    public void DateTimeOffset_FutureDate_PastValue_ReturnsFalse()
    {
        var past = DateTimeOffset.UtcNow.AddDays(-1);
        var filter = new ValiFlow<Event>().FutureDate(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: past)).Should().BeFalse();
    }

    // 3. FutureDate: just past now (1 second ago) → false
    [Fact]
    public void DateTimeOffset_FutureDate_JustPastBoundary_ReturnsFalse()
    {
        var justPast = DateTimeOffset.UtcNow.AddSeconds(-2);
        var filter = new ValiFlow<Event>().FutureDate(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: justPast)).Should().BeFalse();
    }

    // 4. PastDate: past value → true
    [Fact]
    public void DateTimeOffset_PastDate_PastValue_ReturnsTrue()
    {
        var past = DateTimeOffset.UtcNow.AddDays(-1);
        var filter = new ValiFlow<Event>().PastDate(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: past)).Should().BeTrue();
    }

    // 5. PastDate: future value → false
    [Fact]
    public void DateTimeOffset_PastDate_FutureValue_ReturnsFalse()
    {
        var future = DateTimeOffset.UtcNow.AddDays(1);
        var filter = new ValiFlow<Event>().PastDate(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: future)).Should().BeFalse();
    }

    // 6. IsToday: today UTC → true
    [Fact]
    public void DateTimeOffset_IsToday_TodayUTC_ReturnsTrue()
    {
        var today = DateTimeOffset.UtcNow;
        var filter = new ValiFlow<Event>().IsToday(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: today)).Should().BeTrue();
    }

    // 7. IsToday: yesterday → false
    [Fact]
    public void DateTimeOffset_IsToday_Yesterday_ReturnsFalse()
    {
        var yesterday = DateTimeOffset.UtcNow.AddDays(-1);
        var filter = new ValiFlow<Event>().IsToday(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: yesterday)).Should().BeFalse();
    }

    // 8. IsToday: tomorrow → false
    [Fact]
    public void DateTimeOffset_IsToday_Tomorrow_ReturnsFalse()
    {
        var tomorrow = DateTimeOffset.UtcNow.AddDays(1);
        var filter = new ValiFlow<Event>().IsToday(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: tomorrow)).Should().BeFalse();
    }

    // 9. IsWeekend: Saturday → true
    [Fact]
    public void DateTimeOffset_IsWeekend_Saturday_ReturnsTrue()
    {
        var saturday = GetNextDayOfWeek(DayOfWeek.Saturday);
        var filter = new ValiFlow<Event>().IsWeekend(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: saturday)).Should().BeTrue();
    }

    // 10. IsWeekend: Sunday → true
    [Fact]
    public void DateTimeOffset_IsWeekend_Sunday_ReturnsTrue()
    {
        var sunday = GetNextDayOfWeek(DayOfWeek.Sunday);
        var filter = new ValiFlow<Event>().IsWeekend(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: sunday)).Should().BeTrue();
    }

    // 11. IsWeekend: Monday → false
    [Fact]
    public void DateTimeOffset_IsWeekend_Monday_ReturnsFalse()
    {
        var monday = GetNextDayOfWeek(DayOfWeek.Monday);
        var filter = new ValiFlow<Event>().IsWeekend(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: monday)).Should().BeFalse();
    }

    // 12. IsWeekday: Monday → true
    [Fact]
    public void DateTimeOffset_IsWeekday_Monday_ReturnsTrue()
    {
        var monday = GetNextDayOfWeek(DayOfWeek.Monday);
        var filter = new ValiFlow<Event>().IsWeekday(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: monday)).Should().BeTrue();
    }

    // 13. IsWeekday: Saturday → false
    [Fact]
    public void DateTimeOffset_IsWeekday_Saturday_ReturnsFalse()
    {
        var saturday = GetNextDayOfWeek(DayOfWeek.Saturday);
        var filter = new ValiFlow<Event>().IsWeekday(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: saturday)).Should().BeFalse();
    }

    // 14. IsWeekday: Sunday → false
    [Fact]
    public void DateTimeOffset_IsWeekday_Sunday_ReturnsFalse()
    {
        var sunday = GetNextDayOfWeek(DayOfWeek.Sunday);
        var filter = new ValiFlow<Event>().IsWeekday(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: sunday)).Should().BeFalse();
    }

    // 15. IsDayOfWeek(Monday): Monday → true
    [Fact]
    public void DateTimeOffset_IsDayOfWeek_Monday_MatchingDay_ReturnsTrue()
    {
        var monday = GetNextDayOfWeek(DayOfWeek.Monday);
        var filter = new ValiFlow<Event>().IsDayOfWeek(e => e.StartOffset, DayOfWeek.Monday).Build().Compile();

        filter(MakeEvent(startOffset: monday)).Should().BeTrue();
    }

    // 16. IsDayOfWeek(Monday): Tuesday → false
    [Fact]
    public void DateTimeOffset_IsDayOfWeek_Monday_DifferentDay_ReturnsFalse()
    {
        var tuesday = GetNextDayOfWeek(DayOfWeek.Tuesday);
        var filter = new ValiFlow<Event>().IsDayOfWeek(e => e.StartOffset, DayOfWeek.Monday).Build().Compile();

        filter(MakeEvent(startOffset: tuesday)).Should().BeFalse();
    }

    // 17. IsDayOfWeek(Sunday): Sunday → true
    [Fact]
    public void DateTimeOffset_IsDayOfWeek_Sunday_MatchingDay_ReturnsTrue()
    {
        var sunday = GetNextDayOfWeek(DayOfWeek.Sunday);
        var filter = new ValiFlow<Event>().IsDayOfWeek(e => e.StartOffset, DayOfWeek.Sunday).Build().Compile();

        filter(MakeEvent(startOffset: sunday)).Should().BeTrue();
    }

    // 18. IsDayOfWeek(Sunday): Monday → false
    [Fact]
    public void DateTimeOffset_IsDayOfWeek_Sunday_DifferentDay_ReturnsFalse()
    {
        var monday = GetNextDayOfWeek(DayOfWeek.Monday);
        var filter = new ValiFlow<Event>().IsDayOfWeek(e => e.StartOffset, DayOfWeek.Sunday).Build().Compile();

        filter(MakeEvent(startOffset: monday)).Should().BeFalse();
    }

    // 19. IsInMonth: matching month → true
    [Fact]
    public void DateTimeOffset_IsInMonth_MatchingMonth_ReturnsTrue()
    {
        var date = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().IsInMonth(e => e.StartOffset, 6).Build().Compile();

        filter(MakeEvent(startOffset: date)).Should().BeTrue();
    }

    // 20. IsInMonth: different month → false
    [Fact]
    public void DateTimeOffset_IsInMonth_DifferentMonth_ReturnsFalse()
    {
        var date = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().IsInMonth(e => e.StartOffset, 7).Build().Compile();

        filter(MakeEvent(startOffset: date)).Should().BeFalse();
    }

    // 21. IsInYear: matching year → true
    [Fact]
    public void DateTimeOffset_IsInYear_MatchingYear_ReturnsTrue()
    {
        var date = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().IsInYear(e => e.StartOffset, 2025).Build().Compile();

        filter(MakeEvent(startOffset: date)).Should().BeTrue();
    }

    // 22. IsInYear: different year → false
    [Fact]
    public void DateTimeOffset_IsInYear_DifferentYear_ReturnsFalse()
    {
        var date = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().IsInYear(e => e.StartOffset, 2024).Build().Compile();

        filter(MakeEvent(startOffset: date)).Should().BeFalse();
    }

    // 23. IsBefore: before specified date → true
    [Fact]
    public void DateTimeOffset_IsBefore_BeforeDate_ReturnsTrue()
    {
        var pivot = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var earlier = pivot.AddDays(-1);
        var filter = new ValiFlow<Event>().IsBefore(e => e.StartOffset, pivot).Build().Compile();

        filter(MakeEvent(startOffset: earlier)).Should().BeTrue();
    }

    // 24. IsBefore: after date → false
    [Fact]
    public void DateTimeOffset_IsBefore_AfterDate_ReturnsFalse()
    {
        var pivot = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var later = pivot.AddDays(1);
        var filter = new ValiFlow<Event>().IsBefore(e => e.StartOffset, pivot).Build().Compile();

        filter(MakeEvent(startOffset: later)).Should().BeFalse();
    }

    // 25. IsBefore: equal → false (strict)
    [Fact]
    public void DateTimeOffset_IsBefore_EqualDate_ReturnsFalse()
    {
        var pivot = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().IsBefore(e => e.StartOffset, pivot).Build().Compile();

        filter(MakeEvent(startOffset: pivot)).Should().BeFalse();
    }

    // 26. IsAfter: after date → true
    [Fact]
    public void DateTimeOffset_IsAfter_AfterDate_ReturnsTrue()
    {
        var pivot = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var later = pivot.AddDays(1);
        var filter = new ValiFlow<Event>().IsAfter(e => e.StartOffset, pivot).Build().Compile();

        filter(MakeEvent(startOffset: later)).Should().BeTrue();
    }

    // 27. IsAfter: before date → false
    [Fact]
    public void DateTimeOffset_IsAfter_BeforeDate_ReturnsFalse()
    {
        var pivot = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var earlier = pivot.AddDays(-1);
        var filter = new ValiFlow<Event>().IsAfter(e => e.StartOffset, pivot).Build().Compile();

        filter(MakeEvent(startOffset: earlier)).Should().BeFalse();
    }

    // 28. IsAfter: equal → false (strict)
    [Fact]
    public void DateTimeOffset_IsAfter_EqualDate_ReturnsFalse()
    {
        var pivot = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().IsAfter(e => e.StartOffset, pivot).Build().Compile();

        filter(MakeEvent(startOffset: pivot)).Should().BeFalse();
    }

    // 29. BetweenDates: within range → true
    [Fact]
    public void DateTimeOffset_BetweenDates_WithinRange_ReturnsTrue()
    {
        var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2025, 6, 30, 23, 59, 59, TimeSpan.Zero);
        var mid = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().BetweenDates(e => e.StartOffset, from, to).Build().Compile();

        filter(MakeEvent(startOffset: mid)).Should().BeTrue();
    }

    // 30. BetweenDates: before range → false
    [Fact]
    public void DateTimeOffset_BetweenDates_BeforeRange_ReturnsFalse()
    {
        var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2025, 6, 30, 23, 59, 59, TimeSpan.Zero);
        var before = new DateTimeOffset(2025, 5, 31, 23, 59, 59, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().BetweenDates(e => e.StartOffset, from, to).Build().Compile();

        filter(MakeEvent(startOffset: before)).Should().BeFalse();
    }

    // 31. BetweenDates: after range → false
    [Fact]
    public void DateTimeOffset_BetweenDates_AfterRange_ReturnsFalse()
    {
        var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2025, 6, 30, 23, 59, 59, TimeSpan.Zero);
        var after = new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().BetweenDates(e => e.StartOffset, from, to).Build().Compile();

        filter(MakeEvent(startOffset: after)).Should().BeFalse();
    }

    // 32. BetweenDates: at from boundary → true (inclusive)
    [Fact]
    public void DateTimeOffset_BetweenDates_AtFromBoundary_ReturnsTrue()
    {
        var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2025, 6, 30, 23, 59, 59, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().BetweenDates(e => e.StartOffset, from, to).Build().Compile();

        filter(MakeEvent(startOffset: from)).Should().BeTrue();
    }

    // 33. BetweenDates: at to boundary → true (inclusive)
    [Fact]
    public void DateTimeOffset_BetweenDates_AtToBoundary_ReturnsTrue()
    {
        var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2025, 6, 30, 23, 59, 59, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().BetweenDates(e => e.StartOffset, from, to).Build().Compile();

        filter(MakeEvent(startOffset: to)).Should().BeTrue();
    }

    // 34. InLastDays(30): 15 days ago → true
    [Fact]
    public void DateTimeOffset_InLastDays_15DaysAgo_ReturnsTrue()
    {
        var fifteenAgo = DateTimeOffset.UtcNow.AddDays(-15);
        var filter = new ValiFlow<Event>().InLastDays(e => e.StartOffset, 30).Build().Compile();

        filter(MakeEvent(startOffset: fifteenAgo)).Should().BeTrue();
    }

    // 35. InLastDays(30): 31 days ago → false
    [Fact]
    public void DateTimeOffset_InLastDays_31DaysAgo_ReturnsFalse()
    {
        var thirtyOneAgo = DateTimeOffset.UtcNow.AddDays(-31);
        var filter = new ValiFlow<Event>().InLastDays(e => e.StartOffset, 30).Build().Compile();

        filter(MakeEvent(startOffset: thirtyOneAgo)).Should().BeFalse();
    }

    // 36. InNextDays(30): 15 days from now → true
    [Fact]
    public void DateTimeOffset_InNextDays_15DaysFromNow_ReturnsTrue()
    {
        var fifteenAhead = DateTimeOffset.UtcNow.AddDays(15);
        var filter = new ValiFlow<Event>().InNextDays(e => e.StartOffset, 30).Build().Compile();

        filter(MakeEvent(startOffset: fifteenAhead)).Should().BeTrue();
    }

    // 37. InNextDays(30): today → false (today is excluded; range is (UtcNow.Date, UtcNow.Date+N])
    [Fact]
    public void DateTimeOffset_InNextDays_Today_ReturnsFalse()
    {
        var now = DateTimeOffset.UtcNow;
        var filter = new ValiFlow<Event>().InNextDays(e => e.StartOffset, 30).Build().Compile();

        filter(MakeEvent(startOffset: now)).Should().BeFalse();
    }

    // 38. BetweenDates combined with IsWeekday (And)
    [Fact]
    public void DateTimeOffset_BetweenDates_And_IsWeekday_CombinedFilter()
    {
        var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2025, 6, 30, 23, 59, 59, TimeSpan.Zero);
        // June 16, 2025 is Monday (weekday)
        var mondayInRange = new DateTimeOffset(2025, 6, 16, 10, 0, 0, TimeSpan.Zero);
        // June 14, 2025 is Saturday (weekend)
        var saturdayInRange = new DateTimeOffset(2025, 6, 14, 10, 0, 0, TimeSpan.Zero);
        // July 7, 2025 is Monday (weekday) but outside range
        var mondayOutOfRange = new DateTimeOffset(2025, 7, 7, 10, 0, 0, TimeSpan.Zero);

        var filter = new ValiFlow<Event>()
            .BetweenDates(e => e.StartOffset, from, to)
            .And()
            .IsWeekday(e => e.StartOffset)
            .Build().Compile();

        filter(MakeEvent(startOffset: mondayInRange)).Should().BeTrue();
        filter(MakeEvent(startOffset: saturdayInRange)).Should().BeFalse();
        filter(MakeEvent(startOffset: mondayOutOfRange)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DateOnly Tests
    // ═══════════════════════════════════════════════════════════════════════

    // 39. FutureDate: tomorrow → true
    [Fact]
    public void DateOnly_FutureDate_Tomorrow_ReturnsTrue()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var filter = new ValiFlow<Event>().FutureDate(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: tomorrow)).Should().BeTrue();
    }

    // 40. FutureDate: yesterday → false
    [Fact]
    public void DateOnly_FutureDate_Yesterday_ReturnsFalse()
    {
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var filter = new ValiFlow<Event>().FutureDate(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: yesterday)).Should().BeFalse();
    }

    // 41. FutureDate: today → false (strictly future)
    [Fact]
    public void DateOnly_FutureDate_Today_ReturnsFalse()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var filter = new ValiFlow<Event>().FutureDate(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: today)).Should().BeFalse();
    }

    // 42. PastDate: yesterday → true
    [Fact]
    public void DateOnly_PastDate_Yesterday_ReturnsTrue()
    {
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var filter = new ValiFlow<Event>().PastDate(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: yesterday)).Should().BeTrue();
    }

    // 43. PastDate: tomorrow → false
    [Fact]
    public void DateOnly_PastDate_Tomorrow_ReturnsFalse()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var filter = new ValiFlow<Event>().PastDate(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: tomorrow)).Should().BeFalse();
    }

    // 44. PastDate: today → false
    [Fact]
    public void DateOnly_PastDate_Today_ReturnsFalse()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var filter = new ValiFlow<Event>().PastDate(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: today)).Should().BeFalse();
    }

    // 45. IsToday: today → true
    [Fact]
    public void DateOnly_IsToday_Today_ReturnsTrue()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var filter = new ValiFlow<Event>().IsToday(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: today)).Should().BeTrue();
    }

    // 46. IsToday: yesterday → false
    [Fact]
    public void DateOnly_IsToday_Yesterday_ReturnsFalse()
    {
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var filter = new ValiFlow<Event>().IsToday(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: yesterday)).Should().BeFalse();
    }

    // 47. IsWeekend: Saturday → true
    [Fact]
    public void DateOnly_IsWeekend_Saturday_ReturnsTrue()
    {
        var saturday = new DateOnly(2025, 6, 14); // June 14, 2025 is Saturday
        var filter = new ValiFlow<Event>().IsWeekend(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: saturday)).Should().BeTrue();
    }

    // 48. IsWeekend: Monday → false
    [Fact]
    public void DateOnly_IsWeekend_Monday_ReturnsFalse()
    {
        var monday = new DateOnly(2025, 6, 16); // June 16, 2025 is Monday
        var filter = new ValiFlow<Event>().IsWeekend(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: monday)).Should().BeFalse();
    }

    // 49. IsWeekday: Wednesday → true
    [Fact]
    public void DateOnly_IsWeekday_Wednesday_ReturnsTrue()
    {
        var wednesday = new DateOnly(2025, 6, 18); // June 18, 2025 is Wednesday
        var filter = new ValiFlow<Event>().IsWeekday(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: wednesday)).Should().BeTrue();
    }

    // 50. IsWeekday: Sunday → false
    [Fact]
    public void DateOnly_IsWeekday_Sunday_ReturnsFalse()
    {
        var sunday = new DateOnly(2025, 6, 15); // June 15, 2025 is Sunday
        var filter = new ValiFlow<Event>().IsWeekday(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: sunday)).Should().BeFalse();
    }

    // 51. IsDayOfWeek: specific day matches → true
    [Fact]
    public void DateOnly_IsDayOfWeek_MatchingDay_ReturnsTrue()
    {
        var wednesday = new DateOnly(2025, 6, 18); // Wednesday
        var filter = new ValiFlow<Event>().IsDayOfWeek(e => e.EventDate, DayOfWeek.Wednesday).Build().Compile();

        filter(MakeEvent(eventDate: wednesday)).Should().BeTrue();
    }

    // 52. IsDayOfWeek: specific day does not match → false
    [Fact]
    public void DateOnly_IsDayOfWeek_NonMatchingDay_ReturnsFalse()
    {
        var wednesday = new DateOnly(2025, 6, 18); // Wednesday
        var filter = new ValiFlow<Event>().IsDayOfWeek(e => e.EventDate, DayOfWeek.Thursday).Build().Compile();

        filter(MakeEvent(eventDate: wednesday)).Should().BeFalse();
    }

    // 53. IsInMonth: same month → true
    [Fact]
    public void DateOnly_IsInMonth_SameMonth_ReturnsTrue()
    {
        var date = new DateOnly(2025, 8, 20);
        var filter = new ValiFlow<Event>().IsInMonth(e => e.EventDate, 8).Build().Compile();

        filter(MakeEvent(eventDate: date)).Should().BeTrue();
    }

    // 54. IsInMonth: different month → false
    [Fact]
    public void DateOnly_IsInMonth_DifferentMonth_ReturnsFalse()
    {
        var date = new DateOnly(2025, 8, 20);
        var filter = new ValiFlow<Event>().IsInMonth(e => e.EventDate, 9).Build().Compile();

        filter(MakeEvent(eventDate: date)).Should().BeFalse();
    }

    // 55. IsInYear: same year → true
    [Fact]
    public void DateOnly_IsInYear_SameYear_ReturnsTrue()
    {
        var date = new DateOnly(2025, 8, 20);
        var filter = new ValiFlow<Event>().IsInYear(e => e.EventDate, 2025).Build().Compile();

        filter(MakeEvent(eventDate: date)).Should().BeTrue();
    }

    // 56. IsInYear: different year → false
    [Fact]
    public void DateOnly_IsInYear_DifferentYear_ReturnsFalse()
    {
        var date = new DateOnly(2025, 8, 20);
        var filter = new ValiFlow<Event>().IsInYear(e => e.EventDate, 2024).Build().Compile();

        filter(MakeEvent(eventDate: date)).Should().BeFalse();
    }

    // 57. IsBefore: earlier date → true
    [Fact]
    public void DateOnly_IsBefore_EarlierDate_ReturnsTrue()
    {
        var pivot = new DateOnly(2025, 6, 15);
        var earlier = new DateOnly(2025, 6, 14);
        var filter = new ValiFlow<Event>().IsBefore(e => e.EventDate, pivot).Build().Compile();

        filter(MakeEvent(eventDate: earlier)).Should().BeTrue();
    }

    // 58. IsBefore: later date → false
    [Fact]
    public void DateOnly_IsBefore_LaterDate_ReturnsFalse()
    {
        var pivot = new DateOnly(2025, 6, 15);
        var later = new DateOnly(2025, 6, 16);
        var filter = new ValiFlow<Event>().IsBefore(e => e.EventDate, pivot).Build().Compile();

        filter(MakeEvent(eventDate: later)).Should().BeFalse();
    }

    // 59. IsBefore: same date → false (strict)
    [Fact]
    public void DateOnly_IsBefore_SameDate_ReturnsFalse()
    {
        var pivot = new DateOnly(2025, 6, 15);
        var filter = new ValiFlow<Event>().IsBefore(e => e.EventDate, pivot).Build().Compile();

        filter(MakeEvent(eventDate: pivot)).Should().BeFalse();
    }

    // 60. IsAfter: later date → true
    [Fact]
    public void DateOnly_IsAfter_LaterDate_ReturnsTrue()
    {
        var pivot = new DateOnly(2025, 6, 15);
        var later = new DateOnly(2025, 6, 16);
        var filter = new ValiFlow<Event>().IsAfter(e => e.EventDate, pivot).Build().Compile();

        filter(MakeEvent(eventDate: later)).Should().BeTrue();
    }

    // 61. IsAfter: earlier date → false
    [Fact]
    public void DateOnly_IsAfter_EarlierDate_ReturnsFalse()
    {
        var pivot = new DateOnly(2025, 6, 15);
        var earlier = new DateOnly(2025, 6, 14);
        var filter = new ValiFlow<Event>().IsAfter(e => e.EventDate, pivot).Build().Compile();

        filter(MakeEvent(eventDate: earlier)).Should().BeFalse();
    }

    // 62. IsAfter: same date → false (strict)
    [Fact]
    public void DateOnly_IsAfter_SameDate_ReturnsFalse()
    {
        var pivot = new DateOnly(2025, 6, 15);
        var filter = new ValiFlow<Event>().IsAfter(e => e.EventDate, pivot).Build().Compile();

        filter(MakeEvent(eventDate: pivot)).Should().BeFalse();
    }

    // 63. BetweenDates: inside → true
    [Fact]
    public void DateOnly_BetweenDates_InsideRange_ReturnsTrue()
    {
        var from = new DateOnly(2025, 6, 1);
        var to = new DateOnly(2025, 6, 30);
        var mid = new DateOnly(2025, 6, 15);
        var filter = new ValiFlow<Event>().BetweenDates(e => e.EventDate, from, to).Build().Compile();

        filter(MakeEvent(eventDate: mid)).Should().BeTrue();
    }

    // 64. BetweenDates: outside → false
    [Fact]
    public void DateOnly_BetweenDates_OutsideRange_ReturnsFalse()
    {
        var from = new DateOnly(2025, 6, 1);
        var to = new DateOnly(2025, 6, 30);
        var outside = new DateOnly(2025, 7, 1);
        var filter = new ValiFlow<Event>().BetweenDates(e => e.EventDate, from, to).Build().Compile();

        filter(MakeEvent(eventDate: outside)).Should().BeFalse();
    }

    // 65. BetweenDates: at from boundary → true (inclusive)
    [Fact]
    public void DateOnly_BetweenDates_AtFromBoundary_ReturnsTrue()
    {
        var from = new DateOnly(2025, 6, 1);
        var to = new DateOnly(2025, 6, 30);
        var filter = new ValiFlow<Event>().BetweenDates(e => e.EventDate, from, to).Build().Compile();

        filter(MakeEvent(eventDate: from)).Should().BeTrue();
    }

    // 66. BetweenDates: at to boundary → true (inclusive)
    [Fact]
    public void DateOnly_BetweenDates_AtToBoundary_ReturnsTrue()
    {
        var from = new DateOnly(2025, 6, 1);
        var to = new DateOnly(2025, 6, 30);
        var filter = new ValiFlow<Event>().BetweenDates(e => e.EventDate, from, to).Build().Compile();

        filter(MakeEvent(eventDate: to)).Should().BeTrue();
    }

    // 67. IsFirstDayOfMonth: day 1 → true
    [Fact]
    public void DateOnly_IsFirstDayOfMonth_DayOne_ReturnsTrue()
    {
        var firstDay = new DateOnly(2025, 6, 1);
        var filter = new ValiFlow<Event>().IsFirstDayOfMonth(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: firstDay)).Should().BeTrue();
    }

    // 68. IsFirstDayOfMonth: day 2 → false
    [Fact]
    public void DateOnly_IsFirstDayOfMonth_DayTwo_ReturnsFalse()
    {
        var secondDay = new DateOnly(2025, 6, 2);
        var filter = new ValiFlow<Event>().IsFirstDayOfMonth(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: secondDay)).Should().BeFalse();
    }

    // 69. IsFirstDayOfMonth: January 1st → true
    [Fact]
    public void DateOnly_IsFirstDayOfMonth_January1st_ReturnsTrue()
    {
        var jan1 = new DateOnly(2025, 1, 1);
        var filter = new ValiFlow<Event>().IsFirstDayOfMonth(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: jan1)).Should().BeTrue();
    }

    // 70. IsLastDayOfMonth: last day of month → true
    [Fact]
    public void DateOnly_IsLastDayOfMonth_LastDayOfMonth_ReturnsTrue()
    {
        var lastDay = new DateOnly(2025, 6, 30); // June has 30 days
        var filter = new ValiFlow<Event>().IsLastDayOfMonth(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: lastDay)).Should().BeTrue();
    }

    // 71. IsLastDayOfMonth: second-to-last → false
    [Fact]
    public void DateOnly_IsLastDayOfMonth_SecondToLastDay_ReturnsFalse()
    {
        var secondToLast = new DateOnly(2025, 6, 29);
        var filter = new ValiFlow<Event>().IsLastDayOfMonth(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: secondToLast)).Should().BeFalse();
    }

    // 72. IsLastDayOfMonth: February in non-leap year (day 28) → true
    [Fact]
    public void DateOnly_IsLastDayOfMonth_FebNonLeapYear_Day28_ReturnsTrue()
    {
        var feb28NonLeap = new DateOnly(2025, 2, 28); // 2025 is not a leap year
        var filter = new ValiFlow<Event>().IsLastDayOfMonth(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: feb28NonLeap)).Should().BeTrue();
    }

    // 73. IsLastDayOfMonth: February in leap year (day 29) → true
    [Fact]
    public void DateOnly_IsLastDayOfMonth_FebLeapYear_Day29_ReturnsTrue()
    {
        var feb29LeapYear = new DateOnly(2024, 2, 29); // 2024 is a leap year
        var filter = new ValiFlow<Event>().IsLastDayOfMonth(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: feb29LeapYear)).Should().BeTrue();
    }

    // 74. DateOnly combined filter: FutureDate AND IsWeekday
    [Fact]
    public void DateOnly_Combined_FutureDate_And_IsWeekday_ReturnsCorrectly()
    {
        // Find a future weekday
        var futureWeekday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        while (futureWeekday.DayOfWeek == DayOfWeek.Saturday || futureWeekday.DayOfWeek == DayOfWeek.Sunday)
            futureWeekday = futureWeekday.AddDays(1);

        // Find a future weekend day
        var futureWeekend = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        while (futureWeekend.DayOfWeek != DayOfWeek.Saturday && futureWeekend.DayOfWeek != DayOfWeek.Sunday)
            futureWeekend = futureWeekend.AddDays(1);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var filter = new ValiFlow<Event>()
            .FutureDate(e => e.EventDate)
            .And()
            .IsWeekday(e => e.EventDate)
            .Build().Compile();

        filter(MakeEvent(eventDate: futureWeekday)).Should().BeTrue();
        filter(MakeEvent(eventDate: futureWeekend)).Should().BeFalse();
        filter(MakeEvent(eventDate: today)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // TimeOnly Tests
    // ═══════════════════════════════════════════════════════════════════════

    // 75. IsBefore: earlier time → true
    [Fact]
    public void TimeOnly_IsBefore_EarlierTime_ReturnsTrue()
    {
        var pivot = new TimeOnly(12, 0);
        var earlier = new TimeOnly(9, 0);
        var filter = new ValiFlow<Event>().IsBefore(e => e.StartTime, pivot).Build().Compile();

        filter(MakeEvent(startTime: earlier)).Should().BeTrue();
    }

    // 76. IsBefore: later time → false
    [Fact]
    public void TimeOnly_IsBefore_LaterTime_ReturnsFalse()
    {
        var pivot = new TimeOnly(12, 0);
        var later = new TimeOnly(15, 0);
        var filter = new ValiFlow<Event>().IsBefore(e => e.StartTime, pivot).Build().Compile();

        filter(MakeEvent(startTime: later)).Should().BeFalse();
    }

    // 77. IsBefore: equal time → false (strict)
    [Fact]
    public void TimeOnly_IsBefore_EqualTime_ReturnsFalse()
    {
        var pivot = new TimeOnly(12, 0);
        var filter = new ValiFlow<Event>().IsBefore(e => e.StartTime, pivot).Build().Compile();

        filter(MakeEvent(startTime: pivot)).Should().BeFalse();
    }

    // 78. IsAfter: later time → true
    [Fact]
    public void TimeOnly_IsAfter_LaterTime_ReturnsTrue()
    {
        var pivot = new TimeOnly(12, 0);
        var later = new TimeOnly(15, 0);
        var filter = new ValiFlow<Event>().IsAfter(e => e.StartTime, pivot).Build().Compile();

        filter(MakeEvent(startTime: later)).Should().BeTrue();
    }

    // 79. IsAfter: earlier time → false
    [Fact]
    public void TimeOnly_IsAfter_EarlierTime_ReturnsFalse()
    {
        var pivot = new TimeOnly(12, 0);
        var earlier = new TimeOnly(9, 0);
        var filter = new ValiFlow<Event>().IsAfter(e => e.StartTime, pivot).Build().Compile();

        filter(MakeEvent(startTime: earlier)).Should().BeFalse();
    }

    // 80. IsAfter: equal time → false (strict)
    [Fact]
    public void TimeOnly_IsAfter_EqualTime_ReturnsFalse()
    {
        var pivot = new TimeOnly(12, 0);
        var filter = new ValiFlow<Event>().IsAfter(e => e.StartTime, pivot).Build().Compile();

        filter(MakeEvent(startTime: pivot)).Should().BeFalse();
    }

    // 81. IsBetween: inside range → true
    [Fact]
    public void TimeOnly_IsBetween_InsideRange_ReturnsTrue()
    {
        var from = new TimeOnly(9, 0);
        var to = new TimeOnly(17, 0);
        var mid = new TimeOnly(13, 0);
        var filter = new ValiFlow<Event>().IsBetween(e => e.StartTime, from, to).Build().Compile();

        filter(MakeEvent(startTime: mid)).Should().BeTrue();
    }

    // 82. IsBetween: outside range → false
    [Fact]
    public void TimeOnly_IsBetween_OutsideRange_ReturnsFalse()
    {
        var from = new TimeOnly(9, 0);
        var to = new TimeOnly(17, 0);
        var outside = new TimeOnly(20, 0);
        var filter = new ValiFlow<Event>().IsBetween(e => e.StartTime, from, to).Build().Compile();

        filter(MakeEvent(startTime: outside)).Should().BeFalse();
    }

    // 83. IsBetween: at from boundary → true (inclusive)
    [Fact]
    public void TimeOnly_IsBetween_AtFromBoundary_ReturnsTrue()
    {
        var from = new TimeOnly(9, 0);
        var to = new TimeOnly(17, 0);
        var filter = new ValiFlow<Event>().IsBetween(e => e.StartTime, from, to).Build().Compile();

        filter(MakeEvent(startTime: from)).Should().BeTrue();
    }

    // 84. IsBetween: at to boundary → true (inclusive)
    [Fact]
    public void TimeOnly_IsBetween_AtToBoundary_ReturnsTrue()
    {
        var from = new TimeOnly(9, 0);
        var to = new TimeOnly(17, 0);
        var filter = new ValiFlow<Event>().IsBetween(e => e.StartTime, from, to).Build().Compile();

        filter(MakeEvent(startTime: to)).Should().BeTrue();
    }

    // 85. IsBetween: before from → false
    [Fact]
    public void TimeOnly_IsBetween_BeforeFrom_ReturnsFalse()
    {
        var from = new TimeOnly(9, 0);
        var to = new TimeOnly(17, 0);
        var beforeFrom = new TimeOnly(8, 59);
        var filter = new ValiFlow<Event>().IsBetween(e => e.StartTime, from, to).Build().Compile();

        filter(MakeEvent(startTime: beforeFrom)).Should().BeFalse();
    }

    // 86. IsBetween: after to → false
    [Fact]
    public void TimeOnly_IsBetween_AfterTo_ReturnsFalse()
    {
        var from = new TimeOnly(9, 0);
        var to = new TimeOnly(17, 0);
        var afterTo = new TimeOnly(17, 1);
        var filter = new ValiFlow<Event>().IsBetween(e => e.StartTime, from, to).Build().Compile();

        filter(MakeEvent(startTime: afterTo)).Should().BeFalse();
    }

    // 87. IsAM: 00:00 → true
    [Fact]
    public void TimeOnly_IsAM_Midnight_ReturnsTrue()
    {
        var midnight = new TimeOnly(0, 0);
        var filter = new ValiFlow<Event>().IsAM(e => e.StartTime).Build().Compile();

        filter(MakeEvent(startTime: midnight)).Should().BeTrue();
    }

    // 88. IsAM: 11:59 → true
    [Fact]
    public void TimeOnly_IsAM_Eleven59_ReturnsTrue()
    {
        var almostNoon = new TimeOnly(11, 59);
        var filter = new ValiFlow<Event>().IsAM(e => e.StartTime).Build().Compile();

        filter(MakeEvent(startTime: almostNoon)).Should().BeTrue();
    }

    // 89. IsAM: 12:00 → false
    [Fact]
    public void TimeOnly_IsAM_Noon_ReturnsFalse()
    {
        var noon = new TimeOnly(12, 0);
        var filter = new ValiFlow<Event>().IsAM(e => e.StartTime).Build().Compile();

        filter(MakeEvent(startTime: noon)).Should().BeFalse();
    }

    // 90. IsPM: 12:00 → true
    [Fact]
    public void TimeOnly_IsPM_Noon_ReturnsTrue()
    {
        var noon = new TimeOnly(12, 0);
        var filter = new ValiFlow<Event>().IsPM(e => e.StartTime).Build().Compile();

        filter(MakeEvent(startTime: noon)).Should().BeTrue();
    }

    // 91. IsPM: 23:59 → true
    [Fact]
    public void TimeOnly_IsPM_LastMinuteOfDay_ReturnsTrue()
    {
        var lastMinute = new TimeOnly(23, 59);
        var filter = new ValiFlow<Event>().IsPM(e => e.StartTime).Build().Compile();

        filter(MakeEvent(startTime: lastMinute)).Should().BeTrue();
    }

    // 92. IsPM: 11:59 → false
    [Fact]
    public void TimeOnly_IsPM_AlmostNoon_ReturnsFalse()
    {
        var almostNoon = new TimeOnly(11, 59);
        var filter = new ValiFlow<Event>().IsPM(e => e.StartTime).Build().Compile();

        filter(MakeEvent(startTime: almostNoon)).Should().BeFalse();
    }

    // 93. IsExactTime: exact match → true
    [Fact]
    public void TimeOnly_IsExactTime_ExactMatch_ReturnsTrue()
    {
        var time = new TimeOnly(14, 30);
        var filter = new ValiFlow<Event>().IsExactTime(e => e.StartTime, time).Build().Compile();

        filter(MakeEvent(startTime: time)).Should().BeTrue();
    }

    // 94. IsExactTime: off by one minute → false
    [Fact]
    public void TimeOnly_IsExactTime_OffByOneMinute_ReturnsFalse()
    {
        var time = new TimeOnly(14, 30);
        var offByOne = new TimeOnly(14, 31);
        var filter = new ValiFlow<Event>().IsExactTime(e => e.StartTime, time).Build().Compile();

        filter(MakeEvent(startTime: offByOne)).Should().BeFalse();
    }

    // 95. IsInHour: matching hour → true
    [Fact]
    public void TimeOnly_IsInHour_MatchingHour_ReturnsTrue()
    {
        var time = new TimeOnly(14, 30);
        var filter = new ValiFlow<Event>().IsInHour(e => e.StartTime, 14).Build().Compile();

        filter(MakeEvent(startTime: time)).Should().BeTrue();
    }

    // 96. IsInHour: different hour → false
    [Fact]
    public void TimeOnly_IsInHour_DifferentHour_ReturnsFalse()
    {
        var time = new TimeOnly(14, 30);
        var filter = new ValiFlow<Event>().IsInHour(e => e.StartTime, 15).Build().Compile();

        filter(MakeEvent(startTime: time)).Should().BeFalse();
    }

    // 97. IsInHour: hour 0 → true for midnight
    [Fact]
    public void TimeOnly_IsInHour_HourZero_Midnight_ReturnsTrue()
    {
        var midnight = new TimeOnly(0, 45);
        var filter = new ValiFlow<Event>().IsInHour(e => e.StartTime, 0).Build().Compile();

        filter(MakeEvent(startTime: midnight)).Should().BeTrue();
    }

    // 98. TimeOnly combined: IsAM AND IsBefore(12:00)
    [Fact]
    public void TimeOnly_Combined_IsAM_And_IsBefore_Noon()
    {
        var amTime = new TimeOnly(9, 0);
        var noon = new TimeOnly(12, 0);
        var pmTime = new TimeOnly(13, 0);

        var filter = new ValiFlow<Event>()
            .IsAM(e => e.StartTime)
            .And()
            .IsBefore(e => e.StartTime, noon)
            .Build().Compile();

        filter(MakeEvent(startTime: amTime)).Should().BeTrue();
        filter(MakeEvent(startTime: pmTime)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Explain() Tests
    // ═══════════════════════════════════════════════════════════════════════

    // 99. Explain() with single GreaterThan condition → contains ">"
    [Fact]
    public void Explain_SingleGreaterThanCondition_ContainsGreaterThanSymbol()
    {
        var builder = new ValiFlow<Event>().GreaterThan(e => e.StartTime, new TimeOnly(9, 0));
        var explanation = builder.Explain();

        explanation.Should().Contain(">");
    }

    // 100. Explain() with AND condition → contains "AND"
    [Fact]
    public void Explain_AndCondition_ContainsAnd()
    {
        var builder = new ValiFlow<Event>()
            .IsAM(e => e.StartTime)
            .And()
            .IsBefore(e => e.StartTime, new TimeOnly(12, 0));
        var explanation = builder.Explain();

        explanation.Should().Contain("AND");
    }

    // 101. Explain() with OR condition → contains "OR"
    [Fact]
    public void Explain_OrCondition_ContainsOr()
    {
        var builder = new ValiFlow<Event>()
            .IsWeekend(e => e.StartOffset)
            .Or()
            .IsWeekend(e => e.EndOffset);
        var explanation = builder.Explain();

        explanation.Should().Contain("OR");
    }

    // 102. Explain() returns non-null string always
    [Fact]
    public void Explain_WithCondition_ReturnsNonNullString()
    {
        var builder = new ValiFlow<Event>().IsAM(e => e.StartTime);
        var explanation = builder.Explain();

        explanation.Should().NotBeNull();
    }

    // 103. Explain() result changes when negated
    [Fact]
    public void Explain_NormalVsNegated_ExpressionStringChanges()
    {
        var normalExpr = new ValiFlow<Event>().IsTrue(e => e.IsActive).Build();
        var negatedExpr = new ValiFlow<Event>().IsTrue(e => e.IsActive).BuildNegated();

        var normalStr = normalExpr.ToString();
        var negatedStr = negatedExpr.ToString();

        negatedStr.Should().NotBe(normalStr);
    }

    // 104. Explain() with no conditions produces non-null output
    [Fact]
    public void Explain_NoConditions_ReturnsNonNullString()
    {
        // Build with a simple condition and check Explain works
        var builder = new ValiFlow<Event>().IsTrue(e => e.IsActive);
        var explanation = builder.Explain();

        explanation.Should().NotBeNull();
        explanation.Should().NotBeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Additional edge-case tests
    // ═══════════════════════════════════════════════════════════════════════

    // 105. IsFirstDayOfMonth: day 28 → false
    [Fact]
    public void DateOnly_IsFirstDayOfMonth_Day28_ReturnsFalse()
    {
        var day28 = new DateOnly(2025, 6, 28);
        var filter = new ValiFlow<Event>().IsFirstDayOfMonth(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: day28)).Should().BeFalse();
    }

    // 106. IsLastDayOfMonth: December 31 → true
    [Fact]
    public void DateOnly_IsLastDayOfMonth_December31_ReturnsTrue()
    {
        var dec31 = new DateOnly(2025, 12, 31);
        var filter = new ValiFlow<Event>().IsLastDayOfMonth(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: dec31)).Should().BeTrue();
    }

    // 107. DateTimeOffset IsDayOfWeek Friday → true for Friday
    [Fact]
    public void DateTimeOffset_IsDayOfWeek_Friday_ReturnsTrue()
    {
        var friday = new DateTimeOffset(2025, 6, 20, 10, 0, 0, TimeSpan.Zero); // June 20, 2025 is Friday
        var filter = new ValiFlow<Event>().IsDayOfWeek(e => e.StartOffset, DayOfWeek.Friday).Build().Compile();

        filter(MakeEvent(startOffset: friday)).Should().BeTrue();
    }

    // 108. DateTimeOffset IsDayOfWeek Friday → false for Thursday
    [Fact]
    public void DateTimeOffset_IsDayOfWeek_Friday_ForThursday_ReturnsFalse()
    {
        var thursday = new DateTimeOffset(2025, 6, 19, 10, 0, 0, TimeSpan.Zero); // June 19, 2025 is Thursday
        var filter = new ValiFlow<Event>().IsDayOfWeek(e => e.StartOffset, DayOfWeek.Friday).Build().Compile();

        filter(MakeEvent(startOffset: thursday)).Should().BeFalse();
    }

    // 109. TimeOnly IsExactTime: same second-level precision → true
    [Fact]
    public void TimeOnly_IsExactTime_SecondPrecision_ReturnsTrue()
    {
        var time = new TimeOnly(14, 30, 45);
        var filter = new ValiFlow<Event>().IsExactTime(e => e.StartTime, time).Build().Compile();

        filter(MakeEvent(startTime: time)).Should().BeTrue();
    }

    // 110. DateOnly IsWeekend: Sunday also → true
    [Fact]
    public void DateOnly_IsWeekend_Sunday_ReturnsTrue()
    {
        var sunday = new DateOnly(2025, 6, 15); // June 15, 2025 is Sunday
        var filter = new ValiFlow<Event>().IsWeekend(e => e.EventDate).Build().Compile();

        filter(MakeEvent(eventDate: sunday)).Should().BeTrue();
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private static DateTimeOffset GetNextDayOfWeek(DayOfWeek day)
    {
        var now = DateTimeOffset.UtcNow;
        int daysToAdd = ((int)day - (int)now.DayOfWeek + 7) % 7;
        if (daysToAdd == 0) daysToAdd = 7; // ensure we get a future occurrence even if today matches
        return now.AddDays(daysToAdd);
    }

    // ── IsYesterday / IsTomorrow ───────────────────────────────────────────

    [Fact]
    public void DateTimeOffset_IsYesterday_YesterdayUtcValue_ReturnsTrue()
    {
        var yesterday = new DateTimeOffset(DateTimeOffset.UtcNow.UtcDateTime.Date.AddDays(-1), TimeSpan.Zero);
        var filter = new ValiFlow<Event>().IsYesterday(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: yesterday)).Should().BeTrue();
    }

    [Fact]
    public void DateTimeOffset_IsYesterday_TodayValue_ReturnsFalse()
    {
        var today = new DateTimeOffset(DateTimeOffset.UtcNow.UtcDateTime.Date, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().IsYesterday(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: today)).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_IsTomorrow_TomorrowUtcValue_ReturnsTrue()
    {
        var tomorrow = new DateTimeOffset(DateTimeOffset.UtcNow.UtcDateTime.Date.AddDays(1), TimeSpan.Zero);
        var filter = new ValiFlow<Event>().IsTomorrow(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: tomorrow)).Should().BeTrue();
    }

    [Fact]
    public void DateTimeOffset_IsTomorrow_TodayValue_ReturnsFalse()
    {
        var today = new DateTimeOffset(DateTimeOffset.UtcNow.UtcDateTime.Date, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().IsTomorrow(e => e.StartOffset).Build().Compile();

        filter(MakeEvent(startOffset: today)).Should().BeFalse();
    }

    // ── ValiFlow BetweenDates cross-property DateOnly ─────────────────────────

    private record DateOnlyRangeEvent(DateOnly Value, DateOnly From, DateOnly To);

    [Fact]
    public void BetweenDates_CrossProperty_DateOnly_ValiFlow_MatchesWhenValueInRange()
    {
        var filter = new ValiFlow<DateOnlyRangeEvent>()
            .BetweenDates(e => e.Value, e => e.From, e => e.To)
            .Build().Compile();

        filter(new DateOnlyRangeEvent(
            new DateOnly(2026, 3, 15),
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 20))).Should().BeTrue();

        filter(new DateOnlyRangeEvent(
            new DateOnly(2026, 3, 25),
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 20))).Should().BeFalse();
    }

}

