using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using FastBiteGroup.Integration.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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

    [Fact]
    public async Task Login_WithUnescapedControlCharactersInJsonBody_ShouldBeCleanedAndProcessed()
    {
        // Arrange: raw JSON with unescaped newline (0x0A) inside the password string
        var rawJson = "{\n  \"email\": \"testuser@fastbite.local\",\n  \"password\": \"Password@123!\n\"\n}";
        var content = new StringContent(rawJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/v1/auth/login", content);

        // Assert: 
        // If the escaper middleware failed, the JSON deserializer would throw, returning a BadHttpRequestException.
        // If the middleware succeeds, the JSON is cleaned and parsed correctly, executing the use case.
        // Since the credentials don't match any registered user, it should return a 400 Bad Request with ProblemDetails, but NOT a 500 or raw JSON error.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Bad Request");
        problemDetails.Type.Should().Be("Auth.InvalidCredentials");
    }
}
