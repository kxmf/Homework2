using Dapper;
using FluentMigrator.Runner;
using Homework2.Domain.Product;
using Homework2.Infrastructure.Repositories;
using Homework2.Infrastructure.TypeHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Homework2.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));

        services.AddScoped<IProductRepository, PostgresProductRepository>();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(DependencyInjection).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }
}
