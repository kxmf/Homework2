using Homework2.Domain.Common;
using Homework2.Domain.Product;
using Homework2.Presentation.Protos;

namespace Homework2.Api.Mappers;

public static class ProductTypeMapper
{
    public static Result<ProductType> ToDomain(this ProductTypeProto proto) => proto switch
    {
        ProductTypeProto.General => ProductType.General,
        ProductTypeProto.Homecare => ProductType.Homecare,
        ProductTypeProto.Electronics => ProductType.Electronics,
        ProductTypeProto.Groceries => ProductType.Groceries,
        _ => Error.Validation("Product.Type", "Product type is unspecified or unknown.")
    };

    public static ProductTypeProto ToProto(this ProductType domain) => domain switch
    {
        ProductType.General => ProductTypeProto.General,
        ProductType.Homecare => ProductTypeProto.Homecare,
        ProductType.Electronics => ProductTypeProto.Electronics,
        ProductType.Groceries => ProductTypeProto.Groceries,
        _ => ProductTypeProto.Unspecified
    };
}
