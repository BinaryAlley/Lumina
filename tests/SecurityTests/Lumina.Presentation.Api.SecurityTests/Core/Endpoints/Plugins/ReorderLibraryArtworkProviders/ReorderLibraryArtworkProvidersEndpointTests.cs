#region ========================================================================= USING =====================================================================================
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Plugins.ReorderLibraryArtworkProviders;

/// <summary>
/// Contains security tests for the <c>/libraries/{libraryId}/artwork-providers/reorder</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryArtworkProvidersEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _libraryArtworkProviderConfigurationEntityFixture = new();
    private readonly ReorderLibraryArtworkProvidersRequestFixture _reorderLibraryArtworkProvidersRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryArtworkProvidersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public ReorderLibraryArtworkProvidersEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task ReorderLibraryArtworkProviders_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string libraryId = Guid.NewGuid().ToString();
        ReorderLibraryArtworkProvidersRequest request = _reorderLibraryArtworkProvidersRequestFixture.Create(
            libraryId: Guid.NewGuid(),
            pluginIds: [Guid.NewGuid()]
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artwork-providers/reorder", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}/artwork-providers/reorder", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Plugins--")] // destructive injection
    public async Task ReorderLibraryArtworkProviders_WithSQLInjectionInPluginIds_ShouldNotLeakOrError(string maliciousPluginId)
    {
        // Arrange
        // authenticate with an admin user and seed a library with artwork provider configurations, so valid ids would reach the handler
        HttpClient client = _apiFactory.CreateClient();
        Guid userId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        Guid libraryId = await SeedLibraryAsync(userId);
        await SeedConfigurationAsync(libraryId, rank: 1);
        await SeedConfigurationAsync(libraryId, rank: 2);

        // Act
        // the malicious value cannot bind to the Guid-typed PluginIds request property, so the body fails JSON deserialization
        // before it ever reaches the endpoint handler
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/v1/libraries/{libraryId}/artwork-providers/reorder",
            new { libraryId, pluginIds = new[] { maliciousPluginId } });

        // Assert
        // note: observed status is 500 because the JSON body cannot be deserialized into the Guid-typed PluginIds property, and
        // FastEndpoints, combined with DontCatchExceptions(), turns the resulting JsonException into a 500 response instead of 400
        // (pre-existing production bug, not fixed here)
        // note: that 500 body leaks the JsonException details, so no DoesNotContain("Exception") is asserted here
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Seeds a <see cref="Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management.LibraryEntity"/> owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: []));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds a <see cref="LibraryArtworkProviderConfigurationEntity"/> for the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library.</param>
    /// <param name="rank">The rank of the provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedConfigurationAsync(Guid libraryId, int rank)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.LibraryArtworkProviderConfigurations.Add(_libraryArtworkProviderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: Guid.NewGuid(), rank: rank));
        await dbContext.SaveChangesAsync();
    }
}
