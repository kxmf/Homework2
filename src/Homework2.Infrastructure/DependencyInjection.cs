using Homework2.Domain.Product;
using Homework2.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Homework2.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        return services;
    }
}
