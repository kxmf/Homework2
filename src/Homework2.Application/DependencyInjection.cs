using Homework2.Application.UseCases.Products.CreateProducts;
using Homework2.Application.UseCases.Products.GetProductById;
using Homework2.Application.UseCases.Products.GetProducts;
using Homework2.Application.UseCases.Storages.CreateStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Homework2.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateProductUseCase, CreateProductUseCase>();
        services.AddScoped<IGetProductByIdUseCase, GetProductByIdUseCase>();
        services.AddScoped<IGetProductsUseCase, GetProductsUseCase>();
        services.AddScoped<ICreateStorageUseCase, CreateStorageUseCase>();

        return services;
    }
}
