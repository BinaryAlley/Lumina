#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Plugins;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryTypeArtworkProviders;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Plugins.GetLibraryTypeArtworkProviders;

/// <summary>
/// Contains integration tests for the <see cref="GetLibraryTypeArtworkProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeArtworkProvidersEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeArtworkProvidersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetLibraryTypeArtworkProvidersEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Theory]
    [InlineData("Book")]
    [InlineData("music")] // The casing of the type is ignored.
    public async Task GetLibraryTypeArtworkProviders_WhenLibraryTypeIsValid_ShouldReturnOkWithTheAvailableProviders(string libraryType)
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/artwork-providers/{libraryType}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        List<LibraryArtworkProviderResponse>? providers = JsonSerializer.Deserialize<List<LibraryArtworkProviderResponse>>(content, _jsonOptions);
        Assert.NotNull(providers);
        // The test host loads no plugins, so no provider supports the type; every provider that is returned starts disabled.
        Assert.All(providers!, provider => Assert.False(provider.IsEnabled));
    }

    [Fact]
    public async Task GetLibraryTypeArtworkProviders_WhenLibraryTypeIsUnknown_ShouldReturnProblemResult()
    {
        // Arrange
        string libraryType = "NotARealLibraryType";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/artwork-providers/{libraryType}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("UnknownLibraryType", errors!["General.Validation"]);
        Assert.Equal($"/api/v1/libraries/artwork-providers/{libraryType}", problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));
    }

    [Fact]
    public async Task GetLibraryTypeArtworkProviders_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync("/api/v1/libraries/artwork-providers/Book");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
