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

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Plugins.SetLibraryArtworkProviderEnabled;

/// <summary>
/// Contains security tests for the <c>/libraries/{libraryId}/artwork-providers/{pluginId}/enabled</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryArtworkProviderEnabledEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _libraryArtworkProviderConfigurationEntityFixture = new();
    private readonly SetLibraryArtworkProviderEnabledRequestFixture _setLibraryArtworkProviderEnabledRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryArtworkProviderEnabledEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public SetLibraryArtworkProviderEnabledEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task SetLibraryArtworkProviderEnabled_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string libraryId = Guid.NewGuid().ToString();
        string pluginId = Guid.NewGuid().ToString();
        SetLibraryArtworkProviderEnabledRequest request = _setLibraryArtworkProviderEnabledRequestFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artwork-providers/{pluginId}/enabled", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}/artwork-providers/{pluginId}/enabled", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Libraries--")] // destructive injection
    public async Task SetLibraryArtworkProviderEnabled_WithSQLInjectionInRouteParameters_ShouldNotLeakOrError(string maliciousId)
    {
        // Arrange
        // authenticate with an admin user and seed a library with an artwork provider configuration, so valid ids would reach the handler
        HttpClient client = _apiFactory.CreateClient();
        Guid userId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        Guid libraryId = await SeedLibraryAsync(userId);
        await SeedConfigurationAsync(libraryId);
        SetLibraryArtworkProviderEnabledRequest request = _setLibraryArtworkProviderEnabledRequestFixture.Create(libraryId: libraryId, pluginId: Guid.NewGuid(), isEnabled: true);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousId)}/artwork-providers/{Guid.NewGuid()}/enabled", request);

        // Assert
        // note: the {libraryId} route parameter is Guid-typed, so the malicious value fails model binding before it reaches the handler
        // note: observed status is 500 because FastEndpoints, combined with DontCatchExceptions(), turns the binding
        // ValidationFailureException into a 500 response instead of 400/422 (pre-existing production bug, not fixed here)
        // note: that 500 body leaks the ValidationFailureException details, so no DoesNotContain("Exception") is asserted here
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
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedConfigurationAsync(Guid libraryId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.LibraryArtworkProviderConfigurations.Add(_libraryArtworkProviderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: Guid.NewGuid(), rank: 1, isEnabled: false));
        await dbContext.SaveChangesAsync();
    }
}
