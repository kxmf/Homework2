using Homework2.Domain.Common;
using Homework2.Domain.Product;

namespace Homework2.Application.UseCases.Storages.CreateStorage;

public class CreateStorageUseCase : ICreateStorageUseCase
{
    public Task<Result<StorageIdDto>> ExecuteAsync(CreateStorageDto createStorageDto = default,
                                                   CancellationToken cancellationToken = default)
    {
        StorageId storageId;

        if (createStorageDto.StorageId.HasValue)
        {
            if (!StorageId.TryCreate(createStorageDto.StorageId.Value, out storageId))
            {
                return Task.FromResult<Result<StorageIdDto>>(
                    Error.Validation("Storage.Id", "StorageId cannot be empty."));
            }
        }
        else
            storageId = StorageId.Unique();

        return Task.FromResult<Result<StorageIdDto>>(new StorageIdDto(storageId.Value));
    }
}
