#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetLibraryPathTemplateParts;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Management.GetLibraryPathTemplateParts;

/// <summary>
/// Contains integration tests for the <see cref="GetLibraryPathTemplatePartsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryPathTemplatePartsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly LibraryPathPartDefinitionResponseFixture _libraryPathPartDefinitionResponseFixture = new();
    private readonly LibraryPathTemplatePartResponseFixture _libraryPathTemplatePartResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryPathTemplatePartsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetLibraryPathTemplatePartsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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

    [Fact]
    public async Task GetLibraryPathTemplateParts_WhenLibraryTypeIsMusic_ShouldReturnMusicCatalog()
    {
        // Arrange
        List<LibraryPathPartDefinitionResponse> expectedParts = GetExpectedMusicParts();
        List<LibraryPathTemplatePartResponse> expectedDefaultTemplateParts = GetExpectedMusicDefaultTemplateParts();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/libraries/path-template-parts/Music");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        LibraryPathTemplateCatalogResponse? catalog = await response.Content.ReadFromJsonAsync<LibraryPathTemplateCatalogResponse>(_jsonOptions);
        Assert.NotNull(catalog);
        Assert.Equal(10, catalog!.Parts.Count);
        Assert.Equal(16, catalog.DefaultTemplateParts.Count);
        Assert.Equal(expectedParts, catalog.Parts);
        Assert.Equal(expectedDefaultTemplateParts, catalog.DefaultTemplateParts);
    }

    [Fact]
    public async Task GetLibraryPathTemplateParts_WhenLibraryTypeIsBook_ShouldReturnBookCatalog()
    {
        // Arrange
        List<LibraryPathPartDefinitionResponse> expectedParts = GetExpectedBookParts();
        List<LibraryPathTemplatePartResponse> expectedDefaultTemplateParts = GetExpectedBookDefaultTemplateParts();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/libraries/path-template-parts/Book");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        LibraryPathTemplateCatalogResponse? catalog = await response.Content.ReadFromJsonAsync<LibraryPathTemplateCatalogResponse>(_jsonOptions);
        Assert.NotNull(catalog);
        Assert.Equal(8, catalog!.Parts.Count);
        Assert.Equal(12, catalog.DefaultTemplateParts.Count);
        Assert.Equal(expectedParts, catalog.Parts);
        Assert.Equal(expectedDefaultTemplateParts, catalog.DefaultTemplateParts);
    }

    [Fact]
    public async Task GetLibraryPathTemplateParts_WhenLibraryTypeHasNoCatalog_ShouldReturnEmptyCatalog()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/libraries/path-template-parts/Movie");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        LibraryPathTemplateCatalogResponse? catalog = await response.Content.ReadFromJsonAsync<LibraryPathTemplateCatalogResponse>(_jsonOptions);
        Assert.NotNull(catalog);
        Assert.Empty(catalog!.Parts);
        Assert.Empty(catalog.DefaultTemplateParts);
    }

    [Fact]
    public async Task GetLibraryPathTemplateParts_WhenLibraryTypeIsUnknown_ShouldReturnProblemResult()
    {
        // Arrange
        string libraryType = "NotARealLibraryType";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/path-template-parts/{libraryType}");

        // Assert
        // An unrecognized library type is a value that is not accepted, so it is reported as forbidden.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(6, problemDetails!.Count);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Forbidden", problemDetails["title"].GetString());
        Assert.Equal("UnknownLibraryType", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/libraries/path-template-parts/{libraryType}", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    /// <summary>
    /// Builds the exact path parts expected for a music library, as defined by <c>MusicLibraryPathPartCatalog</c>.
    /// </summary>
    /// <returns>The expected path parts, in catalog order.</returns>
    private List<LibraryPathPartDefinitionResponse> GetExpectedMusicParts()
    {
        return
        [
            _libraryPathPartDefinitionResponseFixture.Create(kind: "Literal", valueType: "None", defaultRepresentation: string.Empty, isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "Separator", valueType: "None", defaultRepresentation: string.Empty, isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "Artist", valueType: "Text", defaultRepresentation: "{0}", isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "ReleaseType", valueType: "Enum", defaultRepresentation: "{0}", isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "ReleaseYear", valueType: "Year", defaultRepresentation: "{0}", isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "ReleaseName", valueType: "Text", defaultRepresentation: "{0}", isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "TrackNumber", valueType: "Integer", defaultRepresentation: "{0:00}", isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "TrackName", valueType: "Text", defaultRepresentation: "{0}", isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "Extension", valueType: "Text", defaultRepresentation: "{0}", isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "DiscNumber", valueType: "Integer", defaultRepresentation: "{0:00}", isOptionalByDefault: true)
        ];
    }

    /// <summary>
    /// Builds the exact default template parts expected for a music library, as defined by <c>MusicLibraryPathPartCatalog</c>.
    /// </summary>
    /// <returns>The expected default template parts, in template order.</returns>
    private List<LibraryPathTemplatePartResponse> GetExpectedMusicDefaultTemplateParts()
    {
        return
        [
            _libraryPathTemplatePartResponseFixture.Create(kind: "Artist", representation: "{0}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Separator", representation: string.Empty, isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "ReleaseType", representation: "{0}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Separator", representation: string.Empty, isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "ReleaseYear", representation: "{0}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Literal", representation: " - ", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "ReleaseName", representation: "{0}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Separator", representation: string.Empty, isOptional: true),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Literal", representation: "Disk ", isOptional: true),
            _libraryPathTemplatePartResponseFixture.Create(kind: "DiscNumber", representation: "{0:00}", isOptional: true),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Separator", representation: string.Empty, isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "TrackNumber", representation: "{0:00}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Literal", representation: " - ", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "TrackName", representation: "{0}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Literal", representation: ".", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Extension", representation: "{0}", isOptional: false)
        ];
    }

    /// <summary>
    /// Builds the exact path parts expected for a book library, as defined by <c>BookLibraryPathPartCatalog</c>.
    /// </summary>
    /// <returns>The expected path parts, in catalog order.</returns>
    private List<LibraryPathPartDefinitionResponse> GetExpectedBookParts()
    {
        return
        [
            _libraryPathPartDefinitionResponseFixture.Create(kind: "Literal", valueType: "None", defaultRepresentation: string.Empty, isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "Separator", valueType: "None", defaultRepresentation: string.Empty, isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "Author", valueType: "Text", defaultRepresentation: "{0}", isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "Title", valueType: "Text", defaultRepresentation: "{0}", isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "Series", valueType: "Text", defaultRepresentation: "{0}", isOptionalByDefault: true),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "SeriesNumber", valueType: "Integer", defaultRepresentation: "{0}", isOptionalByDefault: true),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "BookId", valueType: "Integer", defaultRepresentation: "{0}", isOptionalByDefault: false),
            _libraryPathPartDefinitionResponseFixture.Create(kind: "Extension", valueType: "Text", defaultRepresentation: "{0}", isOptionalByDefault: false)
        ];
    }

    /// <summary>
    /// Builds the exact default template parts expected for a book library, as defined by <c>BookLibraryPathPartCatalog</c>.
    /// </summary>
    /// <returns>The expected default template parts, in template order.</returns>
    private List<LibraryPathTemplatePartResponse> GetExpectedBookDefaultTemplateParts()
    {
        return
        [
            _libraryPathTemplatePartResponseFixture.Create(kind: "Author", representation: "{0}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Separator", representation: string.Empty, isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Title", representation: "{0}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Literal", representation: " (", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "BookId", representation: "{0}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Literal", representation: ")", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Separator", representation: string.Empty, isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Title", representation: "{0}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Literal", representation: " - ", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Author", representation: "{0}", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Literal", representation: ".", isOptional: false),
            _libraryPathTemplatePartResponseFixture.Create(kind: "Extension", representation: "{0}", isOptional: false)
        ];
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
