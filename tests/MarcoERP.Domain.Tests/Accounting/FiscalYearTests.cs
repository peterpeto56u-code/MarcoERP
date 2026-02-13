using System;
using System.Linq;
using FluentAssertions;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Enums;
using Xunit;

namespace MarcoERP.Domain.Tests.Accounting
{
    public class FiscalYearTests
    {
        [Fact]
        public void Constructor_ValidYear_CreatesWithSetupStatus()
        {
            var fy = new FiscalYear(2026);

            fy.Year.Should().Be(2026);
            fy.Status.Should().Be(FiscalYearStatus.Setup);
            fy.StartDate.Should().Be(new DateTime(2026, 1, 1));
            fy.EndDate.Should().Be(new DateTime(2026, 12, 31));
        }

        [Fact]
        public void Constructor_ValidYear_Creates12Periods()
        {
            var fy = new FiscalYear(2026);
            fy.Periods.Count.Should().Be(12);
        }

        [Fact]
        public void Constructor_YearBelow2000_ThrowsException()
        {
            Action act = () => new FiscalYear(1999);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Constructor_YearAbove2100_ThrowsException()
        {
            Action act = () => new FiscalYear(2101);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Activate_SetupStatus_ChangesToActive()
        {
            var fy = new FiscalYear(2026);
            fy.Activate();
            fy.Status.Should().Be(FiscalYearStatus.Active);
        }

        [Fact]
        public void Activate_AlreadyActive_ThrowsException()
        {
            var fy = new FiscalYear(2026);
            fy.Activate();
            Action act = () => fy.Activate();
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Close_AllPeriodsLocked_ClosesSuccessfully()
        {
            var fy = new FiscalYear(2026);
            fy.Activate();

            // Lock all 12 periods
            var now = DateTime.UtcNow;
            foreach (var period in fy.Periods)
            {
                period.Lock("admin", now);
            }

            fy.Close("admin", now);
            fy.Status.Should().Be(FiscalYearStatus.Closed);
            fy.ClosedBy.Should().Be("admin");
            fy.ClosedAt.Should().NotBeNull();
        }

        [Fact]
        public void Close_WithOpenPeriods_ThrowsException()
        {
            var fy = new FiscalYear(2026);
            fy.Activate();
            // Don't lock any periods

            Action act = () => fy.Close("admin", DateTime.UtcNow);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Close_EmptyClosedBy_ThrowsException()
        {
            var fy = new FiscalYear(2026);
            fy.Activate();
            foreach (var period in fy.Periods)
                period.Lock("admin", DateTime.UtcNow);

            Action act = () => fy.Close("", DateTime.UtcNow);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void IsOpen_ActiveStatus_ReturnsTrue()
        {
            var fy = new FiscalYear(2026);
            fy.Activate();
            fy.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void IsOpen_SetupStatus_ReturnsFalse()
        {
            var fy = new FiscalYear(2026);
            fy.IsOpen.Should().BeFalse();
        }

        [Fact]
        public void ContainsDate_DateInRange_ReturnsTrue()
        {
            var fy = new FiscalYear(2026);
            fy.ContainsDate(new DateTime(2026, 6, 15)).Should().BeTrue();
        }

        [Fact]
        public void ContainsDate_DateOutOfRange_ReturnsFalse()
        {
            var fy = new FiscalYear(2026);
            fy.ContainsDate(new DateTime(2025, 12, 31)).Should().BeFalse();
        }

        [Fact]
        public void GetPeriod_ValidMonth_ReturnsPeriod()
        {
            var fy = new FiscalYear(2026);
            var period = fy.GetPeriod(3);
            period.Should().NotBeNull();
            period.Month.Should().Be(3);
        }

        [Fact]
        public void GetPeriod_InvalidMonth_ThrowsException()
        {
            var fy = new FiscalYear(2026);
            Action act = () => fy.GetPeriod(13);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Periods_AllHaveCorrectMonths()
        {
            var fy = new FiscalYear(2026);
            var months = fy.Periods.Select(p => p.Month).OrderBy(m => m).ToList();
            months.Should().BeEquivalentTo(Enumerable.Range(1, 12));
        }
    }
}
