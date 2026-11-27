#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.Responses.Themes;
using Nemera.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeTemplate;
using Nemera.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Core.Endpoints.Themes.Queries.GetThemeTemplate;

/// <summary>
/// Contains integration tests for the <see cref="GetThemeTemplateEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeTemplateEndpointTests : IClassFixture<AuthenticatedNemeraApiFactory>, IAsyncLifetime
{
    private readonly AuthenticatedNemeraApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeTemplateEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetThemeTemplateEndpointTests(AuthenticatedNemeraApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Performs no setup; the template endpoint is accessed anonymously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetThemeTemplate_WhenCalledWithExistingThemeAndPageKey_ShouldReturnTemplate()
    {
        // Arrange
        await ThemeTestHelpers.WaitForBundledThemeAsync(_apiFactory);
        HttpClient anonymousClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync("/api/v1/themes/editorial-paper/templates/shared/layout");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ThemeTemplateResponse? result = await response.Content.ReadFromJsonAsync<ThemeTemplateResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal("editorial-paper", result!.Theme.ThemeId);
        Assert.False(string.IsNullOrWhiteSpace(result.Template));
    }

    [Theory]
    [InlineData("nemera-default")]
    [InlineData("editorial-paper")]
    public async Task GetThemeTemplate_WhenCalledForTheDeletedItemsPage_ShouldReturnTheBundledTemplate(string themeId)
    {
        // Arrange
        await ThemeTestHelpers.WaitForBundledThemeAsync(_apiFactory);
        HttpClient anonymousClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/api/v1/themes/{themeId}/templates/library/written-content-library/book-library/books/deleted-items");

        // Assert
        response.EnsureSuccessStatusCode();
        ThemeTemplateResponse? result = await response.Content.ReadFromJsonAsync<ThemeTemplateResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(themeId, result!.Theme.ThemeId);
        Assert.Contains("btn-back-to-library", result.Template);
        Assert.Contains("restoreUrlTemplate", result.Template);
    }

    [Theory]
    [InlineData("nemera-default")]
    [InlineData("editorial-paper")]
    public async Task GetThemeTemplate_WhenCalledForTheMusicDeletedItemsPage_ShouldReturnTheBundledTemplateWithItsRestoreHooks(string themeId)
    {
        // Arrange
        await ThemeTestHelpers.WaitForBundledThemeAsync(_apiFactory);
        HttpClient anonymousClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/api/v1/themes/{themeId}/templates/library/audio-library/music-library/deleted-items");

        // Assert
        response.EnsureSuccessStatusCode();
        ThemeTemplateResponse? result = await response.Content.ReadFromJsonAsync<ThemeTemplateResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(themeId, result!.Theme.ThemeId);
        Assert.Contains("deleted-artists-table", result.Template);
        Assert.Contains("deleted-albums-table", result.Template);
        Assert.Contains("deleted-tracks-table", result.Template);
        Assert.Contains("btn-restore-artist", result.Template);
        Assert.Contains("btn-restore-album", result.Template);
        Assert.Contains("btn-restore-track", result.Template);
        Assert.Contains("pageData.artistRestoreUrl", result.Template);
        Assert.Contains("pageData.albumRestoreUrl", result.Template);
        Assert.Contains("pageData.trackRestoreUrl", result.Template);
    }

    [Fact]
    public async Task GetThemeTemplate_WhenThemeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        HttpClient anonymousClient = _apiFactory.CreateClient();
        string themeId = "nonexistent-theme";

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/api/v1/themes/{themeId}/templates/shared/layout");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        Assert.Equal("General.NotFound", problemDetails["title"].GetString());
        Assert.Equal("ThemeNotFound", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/themes/{themeId}/templates/shared/layout", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task GetThemeTemplate_WhenPageKeyIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        await ThemeTestHelpers.WaitForBundledThemeAsync(_apiFactory);
        HttpClient anonymousClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync("/api/v1/themes/editorial-paper/templates/");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
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
        Assert.Contains("PageKeyCannotBeEmpty", errors["General.Validation"]);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
