using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MarcoERP.Application.Common;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Application.Services.Settings;
using MarcoERP.Domain.Entities.Settings;
using MarcoERP.Domain.Interfaces;
using Moq;
using Xunit;

namespace MarcoERP.Application.Tests.Settings
{
    /// <summary>
    /// Phase 5: VersionService tests.
    /// </summary>
    public sealed class VersionServiceTests
    {
        private readonly Mock<IVersionRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ICurrentUserService> _userMock;
        private readonly Mock<IDateTimeProvider> _dateTimeMock;
        private readonly VersionService _sut;

        public VersionServiceTests()
        {
            _repoMock = new Mock<IVersionRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _userMock = new Mock<ICurrentUserService>();
            _dateTimeMock = new Mock<IDateTimeProvider>();
            _userMock.Setup(u => u.Username).Returns("admin");
            _userMock.Setup(u => u.IsAuthenticated).Returns(true);
            _userMock.Setup(u => u.HasPermission(It.IsAny<string>())).Returns(true);

            _sut = new VersionService(_repoMock.Object, _uowMock.Object, _userMock.Object, _dateTimeMock.Object);
        }

        [Fact]
        public async Task GetCurrentVersionAsync_ReturnsLatestVersion()
        {
            _repoMock.Setup(r => r.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SystemVersion("1.1.0", "admin", "Phase 5", DateTime.UtcNow));

            var result = await _sut.GetCurrentVersionAsync();

            result.Should().Be("1.1.0");
        }

        [Fact]
        public async Task GetCurrentVersionAsync_NoVersions_ReturnsZero()
        {
            _repoMock.Setup(r => r.GetLatestVersionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((SystemVersion)null);

            var result = await _sut.GetCurrentVersionAsync();

            result.Should().Be("0.0.0");
        }

        [Fact]
        public async Task RegisterNewVersionAsync_ValidVersion_Succeeds()
        {
            _repoMock.Setup(r => r.VersionExistsAsync("2.0.0", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _sut.RegisterNewVersionAsync("2.0.0", "Major release");

            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<SystemVersion>(), It.IsAny<CancellationToken>()), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RegisterNewVersionAsync_DuplicateVersion_Fails()
        {
            _repoMock.Setup(r => r.VersionExistsAsync("1.0.0", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.RegisterNewVersionAsync("1.0.0", "duplicate");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("مسجل مسبقاً");
        }

        [Fact]
        public async Task RegisterNewVersionAsync_EmptyVersion_Fails()
        {
            var result = await _sut.RegisterNewVersionAsync("", "empty");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("مطلوب");
        }
    }
}
