#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeAsset;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Themes.Queries.GetThemeAsset;

/// <summary>
/// Contains integration tests for the <see cref="GetThemeAssetEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeAssetEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeAssetEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetThemeAssetEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Performs no setup; the asset endpoint is accessed anonymously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetThemeAsset_WhenCalledWithExistingThemeAndAsset_ShouldReturnAssetFile()
    {
        // Arrange
        await ThemeTestHelpers.WaitForBundledThemeAsync(_apiFactory);
        HttpClient anonymousClient = _apiFactory.CreateClient();

        // Act
        // the asset path is relative to the theme pack root and includes the assets/ prefix, so the URL is
        // /themes/{themeId}/assets/assets/{assetPath} (the first assets/ segment is the route prefix)
        HttpResponseMessage response = await anonymousClient.GetAsync("/api/v1/themes/editorial-paper/assets/assets/theme.css");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);

        byte[] bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public async Task GetThemeAsset_WhenThemeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        HttpClient anonymousClient = _apiFactory.CreateClient();
        string themeId = "nonexistent-theme";

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/api/v1/themes/{themeId}/assets/theme.css");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        Assert.Equal("General.NotFound", problemDetails["title"].GetString());
        Assert.Equal("ThemeNotFound", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/themes/{themeId}/assets/theme.css", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task GetThemeAsset_WhenAssetPathIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        await ThemeTestHelpers.WaitForBundledThemeAsync(_apiFactory);
        HttpClient anonymousClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync("/api/v1/themes/editorial-paper/assets/");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains("ThemeAssetPathCannotBeEmpty", errors["General.Validation"]);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
