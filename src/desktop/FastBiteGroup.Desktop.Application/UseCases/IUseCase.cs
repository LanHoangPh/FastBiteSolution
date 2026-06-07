using FastBiteGroup.Desktop.Domain.Models.Shared;

namespace FastBiteGroup.Desktop.Application.UseCases;

public interface IUseCase<in TRequest, TResponse>
{
    Task<Result<TResponse>> ExecuteAsync(TRequest request);
}

public interface IUseCase<in TRequest>
{
    Task<Result> ExecuteAsync(TRequest request);
}
