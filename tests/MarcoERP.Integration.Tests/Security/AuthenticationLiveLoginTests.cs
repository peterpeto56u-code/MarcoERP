using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MarcoERP.Application.DTOs.Security;
using MarcoERP.Application.Services.Security;
using MarcoERP.Infrastructure.Security;
using MarcoERP.Infrastructure.Services;
using MarcoERP.Persistence;
using MarcoERP.Persistence.Repositories;
using MarcoERP.Persistence.Repositories.Security;
using Xunit;

namespace MarcoERP.Integration.Tests.Security
{
    public sealed class AuthenticationLiveLoginTests
    {
        private const string ConnectionString = "Server=.\\SQL2022;Database=MarcoERP;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";

        [Fact]
        public async Task LoginAdmin_WithKnownPassword_DoesNotThrowAndReturnsServiceResult()
        {
            var options = new DbContextOptionsBuilder<MarcoDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            await using var db = new MarcoDbContext(options);

            var userRepo = new UserRepository(db);
            var roleRepo = new RoleRepository(db);
            var passwordHasher = new PasswordHasher();
            var dateTimeProvider = new DateTimeProvider();
            var unitOfWork = new UnitOfWork(db);
            var auditRepo = new AuditLogRepository(db);
            var auditLogger = new AuditLogger(auditRepo, unitOfWork, dateTimeProvider);

            var sut = new AuthenticationService(userRepo, roleRepo, passwordHasher, dateTimeProvider, unitOfWork, auditLogger);

            var action = async () => await sut.LoginAsync(new LoginDto
            {
                Username = "admin",
                Password = "Admin@123456"
            });

            var result = await action.Should().NotThrowAsync();
            result.Subject.Should().NotBeNull();
            result.Subject.ErrorMessage.Should().NotContain("خطأ");
        }
    }
}
