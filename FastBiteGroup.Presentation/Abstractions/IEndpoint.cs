using Microsoft.AspNetCore.Routing;

namespace FastBiteGroup.Presentation.Abstractions;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
