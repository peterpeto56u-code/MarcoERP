using System;
using FluentAssertions;
using MarcoERP.Domain.Entities.Accounting;
using MarcoERP.Domain.Enums;
using Xunit;

namespace MarcoERP.Domain.Tests.Accounting
{
    public class FiscalPeriodTests
    {
        private FiscalPeriod CreateOpenPeriod()
        {
            // Create via FiscalYear constructor which creates all 12 periods
            var fy = new FiscalYear(2026);
            return fy.GetPeriod(1); // January
        }

        [Fact]
        public void Constructor_ViaFiscalYear_CreatesOpenPeriod()
        {
            var period = CreateOpenPeriod();
            period.Status.Should().Be(PeriodStatus.Open);
            period.IsOpen.Should().BeTrue();
            period.Month.Should().Be(1);
            period.Year.Should().Be(2026);
        }

        [Fact]
        public void Lock_OpenPeriod_ChangesToLocked()
        {
            var period = CreateOpenPeriod();
            var now = DateTime.UtcNow;
            period.Lock("admin", now);

            period.Status.Should().Be(PeriodStatus.Locked);
            period.IsOpen.Should().BeFalse();
            period.LockedBy.Should().Be("admin");
            period.LockedAt.Should().Be(now);
        }

        [Fact]
        public void Lock_AlreadyLocked_ThrowsException()
        {
            var period = CreateOpenPeriod();
            period.Lock("admin", DateTime.UtcNow);

            Action act = () => period.Lock("admin", DateTime.UtcNow);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Lock_EmptyLockedBy_ThrowsException()
        {
            var period = CreateOpenPeriod();
            Action act = () => period.Lock("", DateTime.UtcNow);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Unlock_LockedPeriod_ChangesToOpen()
        {
            var period = CreateOpenPeriod();
            period.Lock("admin", DateTime.UtcNow);
            period.Unlock("تصحيح أخطاء");

            period.Status.Should().Be(PeriodStatus.Open);
            period.IsOpen.Should().BeTrue();
            period.UnlockReason.Should().Be("تصحيح أخطاء");
            period.LockedAt.Should().BeNull();
            period.LockedBy.Should().BeNull();
        }

        [Fact]
        public void Unlock_AlreadyOpen_ThrowsException()
        {
            var period = CreateOpenPeriod();
            Action act = () => period.Unlock("سبب");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Unlock_EmptyReason_ThrowsException()
        {
            var period = CreateOpenPeriod();
            period.Lock("admin", DateTime.UtcNow);
            Action act = () => period.Unlock("");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void ContainsDate_DateInRange_ReturnsTrue()
        {
            var period = CreateOpenPeriod();
            period.ContainsDate(new DateTime(2026, 1, 15)).Should().BeTrue();
        }

        [Fact]
        public void ContainsDate_DateOutOfRange_ReturnsFalse()
        {
            var period = CreateOpenPeriod();
            period.ContainsDate(new DateTime(2026, 2, 1)).Should().BeFalse();
        }

        [Fact]
        public void ContainsDate_FirstDayOfMonth_ReturnsTrue()
        {
            var period = CreateOpenPeriod();
            period.ContainsDate(new DateTime(2026, 1, 1)).Should().BeTrue();
        }

        [Fact]
        public void ContainsDate_LastDayOfMonth_ReturnsTrue()
        {
            var period = CreateOpenPeriod();
            period.ContainsDate(new DateTime(2026, 1, 31)).Should().BeTrue();
        }
    }
}
