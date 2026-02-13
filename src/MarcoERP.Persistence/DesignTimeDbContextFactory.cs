using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MarcoERP.Persistence
{
    /// <summary>
    /// Design-time factory for MarcoDbContext.
    /// Used by EF Core CLI tools (dotnet ef migrations) when no DI container is available.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MarcoDbContext>
    {
        public MarcoDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MarcoDbContext>();
            optionsBuilder.UseSqlServer(
                "Server=.\\SQL2022;Database=MarcoERP;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true");

            return new MarcoDbContext(optionsBuilder.Options);
        }
    }
}
