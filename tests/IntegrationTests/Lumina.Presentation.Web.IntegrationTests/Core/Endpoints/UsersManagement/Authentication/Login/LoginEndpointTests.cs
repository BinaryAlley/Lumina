#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Login;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// Contains integration tests for the <c>/{culture}/auth/api-login</c> route served by the <see cref="LoginEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public LoginEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldSignInAndReturnRedirectUrl()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage loginRequest = CreateLoginRequest(new LoginRequest(Username: "testuser", Password: "TestPass123!"), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(loginRequest);
        using JsonDocument json = await WebTestHelpers.ReadJsonAsync(response);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("/en-us/", json.RootElement.GetProperty("data").GetString());
        string allSetCookies = string.Join("; ", response.Headers.GetValues("Set-Cookie"));
        Assert.Contains(".Lumina.Auth", allSetCookies);
        Assert.Contains("Token=", allSetCookies);
    }

    [Fact]
    public async Task Login_WhenTotpRequired_ShouldReturnTotpRequiredSignal()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        ProblemDetailsDto problemDetails = new()
        {
            Title = "General.Validation",
            Detail = "OneOrMoreValidationErrorsOccurred",
            Extensions = new Dictionary<string, JsonElement>
            {
                ["errors"] = CreateErrorsElement(["InvalidTotpCode"])
            }
        };
        _apiFactory.ApiClientStub.RegisterPostException("auth/login", new ApiException(problemDetails, HttpStatusCode.UnprocessableEntity, "auth/login"));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage loginRequest = CreateLoginRequest(new LoginRequest(Username: "testuser", Password: "TestPass123!"), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(loginRequest);
        using JsonDocument json = await WebTestHelpers.ReadJsonAsync(response);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("data").GetProperty("isTotpRequired").GetBoolean());
    }

    [Fact]
    public async Task Login_WhenApiReturnsForbidden_ShouldReturnFailureJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostException("auth/login", new ApiException(new ProblemDetailsDto { Title = "General.Failure", Detail = "InvalidUsernameOrPassword" }, HttpStatusCode.Forbidden, "auth/login"));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage loginRequest = CreateLoginRequest(new LoginRequest(Username: "testuser", Password: "WrongPass123!"), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(loginRequest);
        using JsonDocument json = await WebTestHelpers.ReadJsonAsync(response);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("<b>", json.RootElement.GetProperty("errorMessage").GetString());
    }

    private static HttpRequestMessage CreateLoginRequest(LoginRequest request, string antiforgeryToken)
    {
        HttpRequestMessage loginRequest = new(HttpMethod.Post, "/en-us/auth/api-login")
        {
            Content = JsonContent.Create(request)
        };
        loginRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        loginRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        loginRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return loginRequest;
    }

    private static JsonElement CreateErrorsElement(string[] errorCodes)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteStartArray("General.Validation");
            foreach (string errorCode in errorCodes)
                writer.WriteStringValue(errorCode);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        return JsonDocument.Parse(stream.ToArray()).RootElement.Clone();
    }
}
