using HospitalCore.Modules.Patients.Infrastructure;
using HospitalCore.Modules.Patients.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

[assembly: AssemblyFixture(typeof(HospitalCore.Api.IntegrationTests.HospitalCoreApiFactory))]

namespace HospitalCore.Api.IntegrationTests;

/// <summary>
/// Hosts the API in memory against a disposable SQL Server container shared by every test in the assembly.
/// </summary>
public sealed class HospitalCoreApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2025-CU11-ubuntu-24.04";

    private readonly MsSqlContainer _database = new MsSqlBuilder(SqlServerImage).Build();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PatientsDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{PatientsModule.ConnectionStringName}"] = _database.GetConnectionString(),
            }));
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}
