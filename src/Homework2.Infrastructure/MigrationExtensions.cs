using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Homework2.Infrastructure;

public static class MigrationExtensions
{
    public static IServiceProvider MigrateDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        return serviceProvider;
    }
}
