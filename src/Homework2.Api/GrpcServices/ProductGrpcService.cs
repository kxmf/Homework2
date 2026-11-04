using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Homework2.Api.Mappers;
using Homework2.Application.UseCases.Products.CreateProducts;
using Homework2.Application.UseCases.Products.GetProductById;
using Homework2.Application.UseCases.Products.GetProducts;
using Homework2.Domain.Common;
using Homework2.Domain.Product;
using Homework2.Presentation.Protos;

namespace Homework2.Api.GrpcServices;

public class ProductGrpcService(
    ICreateProductUseCase createProductUseCase,
    IGetProductByIdUseCase getProductByIdUseCase,
    IGetProductsUseCase getProductsUseCase) : ProductProtoService.ProductProtoServiceBase
{
    public override async Task<CreateProductResponse> CreateProduct(
        CreateProductRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.StorageId, out Guid storageGuid))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid StorageId format (expected GUID)."));
        }

        var typeResult = request.Type.ToDomain();
        if (typeResult.IsFailure)
        {
            throw MapErrorToRpcException(typeResult.Error);
        }

        var createDto = new CreateProductDto(
            Title: request.Title,
            Price: (decimal)request.Price,
            Weight: request.Weight,
            Type: typeResult.Value,
            StorageId: storageGuid);

        Result<ProductIdDto> result = await createProductUseCase.ExecuteAsync(createDto, context.CancellationToken);

        if (result.IsFailure)
        {
            throw MapErrorToRpcException(result.Error);
        }

        return new CreateProductResponse
        {
            Id = result.Value.Id.ToString()
        };
    }

    public override async Task<ProductResponse> GetProductById(
        GetProductByIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out Guid productGuid))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ProductId format (expected GUID)."));
        }

        var result = await getProductByIdUseCase.ExecuteAsync(
            new GetProductByIdDto(productGuid),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw MapErrorToRpcException(result.Error);
        }

        return MapToProductResponse(result.Value);
    }

    public override async Task<GetProductsResponse> GetProducts(
        GetProductsRequest request,
        ServerCallContext context)
    {
        DateOnly? createdAt = request.CreatedAt != null
            ? DateOnly.FromDateTime(request.CreatedAt.ToDateTime())
            : null;

        ProductType? productType = null;
        if (request.HasType && request.Type != ProductTypeProto.Unspecified)
        {
            var typeResult = request.Type.ToDomain();
            if (typeResult.IsFailure)
            {
                throw MapErrorToRpcException(typeResult.Error);
            }
            productType = typeResult.Value;
        }

        Guid? storageId = null;
        if (request.HasStorageId && !string.IsNullOrWhiteSpace(request.StorageId))
        {
            if (!Guid.TryParse(request.StorageId, out Guid parsedStorageId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid StorageId filter format (expected GUID)."));
            }
            storageId = parsedStorageId;
        }

        int pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
        int pageSize = request.PageSize > 0 ? request.PageSize : 10;

        var query = new GetProductsDto(createdAt, productType, storageId, pageNumber, pageSize);
        var result = await getProductsUseCase.ExecuteAsync(query, context.CancellationToken);

        if (result.IsFailure)
        {
            throw MapErrorToRpcException(result.Error);
        }

        var response = new GetProductsResponse();
        foreach (var item in result.Value.Items)
        {
            response.Products.Add(new ProductResponse
            {
                Id = item.ProductId.ToString(),
                Title = item.Title,
                Price = (double)item.Price,
                Weight = item.Weight,
                Type = item.ProductType.ToProto(),
                CreatedAt = Timestamp.FromDateTime(item.CreatedAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)),
                StorageId = item.StorageId.ToString()
            });
        }

        return response;
    }

    private static ProductResponse MapToProductResponse(ProductDto dto) =>
        new()
        {
            Id = dto.ProductId.ToString(),
            Title = dto.Title,
            Price = (double)dto.Price,
            Weight = dto.Weight,
            Type = dto.ProductType.ToProto(),
            CreatedAt = Timestamp.FromDateTime(dto.CreatedAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)),
            StorageId = dto.StorageId.ToString()
        };

    private static RpcException MapErrorToRpcException(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCode.NotFound,
            ErrorType.Validation => StatusCode.InvalidArgument,
            ErrorType.Conflict => StatusCode.AlreadyExists,
            _ => StatusCode.Internal
        };

        return new RpcException(new Status(statusCode, error.Message));
    }
}
