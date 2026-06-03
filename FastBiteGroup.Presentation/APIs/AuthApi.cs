using FastBiteGroup.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FastBiteGroup.Presentation.APIs;

public class AuthApi : ApiEndpoint, IEndpoint
{
    private const string BaseUrl = "/api/v{version:apiVersion}/auth";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group1 = app.NewVersionedApi("Authentication")
                       .MapGroup(BaseUrl)
                       .HasApiVersion(1);
    }

}
