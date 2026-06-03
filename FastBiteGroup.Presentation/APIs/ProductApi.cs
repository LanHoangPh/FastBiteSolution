using FastBiteGroup.Presentation.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FastBiteGroup.Presentation.APIs;

public class ProductApi : ApiEndpoint, IEndpoint
{
    private const string BaseUrl = "/api/v{version:apiVersion}/products";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group1 = app.NewVersionedApi("Products")
                       .MapGroup(BaseUrl)
                       .HasApiVersion(1);
    }
}