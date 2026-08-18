#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.Management.UpdateLibrary;

/// <summary>
/// Contains security tests for the <c>/libraries/{id}</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateLibraryEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLibraryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateLibraryEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task UpdateLibrary_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string libraryId = Guid.NewGuid().ToString();
        UpdateLibraryRequest request = new(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Title: "Updated Library",
            LibraryType: "EBook",
            ContentLocations: ["C:/Media"],
            CoverImage: null,
            IsEnabled: true,
            IsLocked: false,
            DownloadMetadataFromWeb: true,
            ShouldSaveMetadataInMediaDirectories: false,
            ShouldSkipUnchangedDirectoriesDuringScan: true
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Libraries--")] // destructive injection
    public async Task UpdateLibrary_WithSQLInjectionInTitle_ShouldNotLeakOrError(string maliciousTitle)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        Guid userId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        UpdateLibraryRequest request = new(
            Id: libraryId,
            UserId: userId,
            Title: maliciousTitle,
            LibraryType: "EBook",
            ContentLocations: ["C:/Media"],
            CoverImage: null,
            IsEnabled: true,
            IsLocked: false,
            DownloadMetadataFromWeb: true,
            ShouldSaveMetadataInMediaDirectories: false,
            ShouldSkipUnchangedDirectoriesDuringScan: true
        );

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}", request);

        // Assert
        // the malicious title passes the authenticated handler, reaches the parameterized data access, and is persisted verbatim
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryEntity? storedLibrary = await dbContext.Libraries.FirstOrDefaultAsync(library => library.Id == libraryId);
        Assert.NotNull(storedLibrary);
        Assert.Equal(maliciousTitle, storedLibrary!.Title);
    }

    /// <summary>
    /// Seeds a <see cref="LibraryEntity"/> owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library to seed.</param>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedLibraryAsync(Guid libraryId, Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Libraries.Add(new LibraryEntity
        {
            Id = libraryId,
            UserId = userId,
            Title = "Test Library",
            LibraryType = LibraryType.EBook,
            ContentLocations = [],
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedBy = null
        });
        await dbContext.SaveChangesAsync();
    }
}
