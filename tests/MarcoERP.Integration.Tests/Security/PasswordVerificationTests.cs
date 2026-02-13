using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MarcoERP.Integration.Tests.Security;

/// <summary>
/// Integration tests to verify stored password hashes.
/// </summary>
public class PasswordVerificationTests
{
    [Fact]
    public void AdminPassword_ShouldMatch_Admin123456()
    {
        // Arrange - This is the actual hash from database
        var storedHash = "$2a$12$K.yeXoucE83pQps.IA88l.22Y3OjSubSJn7rg7LizUenn0.YnDbjW";
        var password = "Admin@123456";

        // Act
        var isValid = BCrypt.Net.BCrypt.Verify(password, storedHash);

        // Assert
        isValid.Should().BeTrue("the admin password should be Admin@123456");
    }

    [Fact]
    public void GenerateHash_ForLOLO9090()
    {
        // Arrange
        var password = "LOLO9090..";

        // Act
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        // Assert - just output the hash
        hash.Should().NotBeNullOrEmpty();
        BCrypt.Net.BCrypt.Verify(password, hash).Should().BeTrue();
    }
}
