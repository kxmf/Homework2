using Dapper;
using FluentMigrator.Runner;
using Homework2.Infrastructure;
using Homework2.Infrastructure.TypeHandlers;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace Homework2.IntegrationTests.Fixtures;

public class PostgresDatabaseFixture : IAsyncLifetime
{
    public const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=products_db;Username=postgres;Password=postgres;";

    public string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") ?? DefaultConnectionString;

    public NpgsqlDataSource DataSource { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        DataSource = NpgsqlDataSource.Create(ConnectionString);

        try
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(ConnectionString)
                    .ScanIn(typeof(DependencyInjection).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .BuildServiceProvider();

            services.MigrateDatabase();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PostgresDatabaseFixture] Migration warning: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    public async Task ResetDatabaseAsync()
    {
        try
        {
            await using var connection = await DataSource.OpenConnectionAsync();
            await connection.ExecuteAsync("TRUNCATE TABLE products CASCADE;");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PostgresDatabaseFixture] Truncate warning: {ex.Message}");
        }
    }

    public async Task DisposeAsync()
    {
        if (DataSource is not null)
        {
            await DataSource.DisposeAsync();
        }
    }
}
