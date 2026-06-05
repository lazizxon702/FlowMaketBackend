using FlowMarketService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FlowMarketService.Data;

/// <summary>
/// <c>dotnet ef</c> dizayn vaqtida kontekstni yaratish — <see cref="Program"/> dagi JWT tekshiruvlarisiz.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Production.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = ConnectionStringHelper.ResolvePostgres(config);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new AppDbContext(options);
    }
}
