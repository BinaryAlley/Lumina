#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.Management.AddLibrary;

/// <summary>
/// Contains security tests for the <c>/libraries</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly AddLibraryRequestFixture _addLibraryRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddLibraryEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task AddLibrary_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: "New Library",
            libraryType: "EBook",
            contentLocations: ["C:/Media"],
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: true
        );

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/libraries", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/libraries", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Libraries--")] // destructive injection
    public async Task AddLibrary_WithSQLInjectionInTitle_ShouldNotLeakOrError(string maliciousTitle)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: maliciousTitle,
            libraryType: "EBook",
            contentLocations: ["C:/Media"],
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: true
        );

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/libraries", request);

        // Assert
        // the malicious title passes the authenticated handler, reaches the parameterized data access, and is persisted verbatim
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryEntity? storedLibrary = await dbContext.Libraries.FirstOrDefaultAsync(library => library.Title == maliciousTitle);
        Assert.NotNull(storedLibrary);
        Assert.Equal(maliciousTitle, storedLibrary!.Title);
    }
}
