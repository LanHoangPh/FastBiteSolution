using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using FastBiteGroup.Integration.Tests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FastBiteGroup.Integration.Tests.Auth;

public class AuthTests : BaseIntegrationTest
{
    public AuthTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_Should_Create_User_And_Return_RegisterResponse()
    {
        // Arrange
        var command = new AuthCommands.RegisterCommand(
            "testuser@fastbite.local",
            "Password@123!",
            "Test",
            "User",
            new DateTime(2000, 1, 1)
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var registerResponse = await response.Content.ReadFromJsonAsync<RegisterResponse>();

        registerResponse.Should().NotBeNull();
        registerResponse!.Message.Should().NotBeNullOrWhiteSpace();
    }
}
