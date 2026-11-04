using Homework2.Domain.Common;

namespace Homework2.Application.UseCases.Storages.CreateStorage;

public interface ICreateStorageUseCase
{
    Task<Result<StorageIdDto>> ExecuteAsync(CreateStorageDto createStorageDto = default, CancellationToken cancellationToken = default);
}
