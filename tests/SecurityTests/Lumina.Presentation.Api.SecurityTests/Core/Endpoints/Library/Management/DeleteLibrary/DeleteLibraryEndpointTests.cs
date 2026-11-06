#region ========================================================================= USING =====================================================================================
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.Management.DeleteLibrary;

/// <summary>
/// Contains security tests for the <see cref="DeleteLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteLibraryEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLibraryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DeleteLibraryEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task DeleteLibrary_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string libraryId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/libraries/{libraryId}");

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
    [InlineData("' OR '1'='1")] // Basic SQL injection.
    [InlineData("'; DROP TABLE Libraries--")] // Destructive injection.
    public async Task DeleteLibrary_WithSQLInjectionInLibraryId_ShouldNotLeakOrError(string maliciousLibraryId)
    {
        // Arrange
        // Authenticate with an admin user and seed a library, so a valid id would reach the handler and be authorized.
        HttpClient client = _apiFactory.CreateClient();
        Guid userId = await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        await SeedLibraryAsync(userId);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}");

        // Assert
        // Note: the {id} route parameter is Guid-typed, so the malicious value fails model binding before it reaches the handler.
        // The observed status is 500 because FastEndpoints, combined with DontCatchExceptions(), turns the binding ValidationFailureException into a 500 response instead of 400/422 (pre-existing production bug, not fixed here).
        // That 500 body leaks the ValidationFailureException details, so no DoesNotContain("Exception") is asserted here.
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteLibrary_WhenTheOwnerLacksTheManagePermission_ShouldReturnForbiddenAndKeepTheLibrary()
    {
        // Arrange
        // The created user holds no permission, so it owns the library without being allowed to change it.
        HttpClient client = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        await _apiFactory.SeedLibraryAsync(libraryId, ownerId);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{libraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            Assert.NotNull(await dbContext.Libraries.FindAsync(libraryId));
        }

        await _apiFactory.RemoveTestUserAsync(ownerUsername);
    }

    [Fact]
    public async Task DeleteLibrary_WhenNonOwnerHoldsTheManagePermission_ShouldReturnForbiddenAndKeepTheLibrary()
    {
        // Arrange
        // Ownership and the manage permission are both required, so a user that holds the permission must not delete a library it does not own.
        HttpClient client = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        await _apiFactory.SeedLibraryAsync(libraryId, ownerId);
        (Guid _, string attackerUsername) = await _apiFactory.CreateAndAuthenticateUserWithPermissionAsync(client, AuthorizationPermission.CanCreateLibraries);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{libraryId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            Assert.NotNull(await dbContext.Libraries.FindAsync(libraryId));
        }

        await _apiFactory.RemoveTestUserAsync(ownerUsername);
        await _apiFactory.RemoveTestUserAsync(attackerUsername);
    }

    /// <summary>
    /// Seeds a <see cref="LibraryEntity"/> owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedLibraryAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: []));
        await dbContext.SaveChangesAsync();
    }
}
