#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.UpdateArtistCover;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.UpdateArtistCover;

/// <summary>
/// Contains security tests for the <see cref="UpdateArtistCoverEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCoverEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistCoverEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateArtistCoverEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdateArtistCover_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        MultipartFormDataContent form = CreateCoverForm("cover.jpg");

        // Act
        HttpResponseMessage response = await client.PutAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/cover", form);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}/artists/{artistId}/cover", problemDetails["instance"].GetProperty("value").GetString());
    }

    [Fact]
    public async Task UpdateArtistCover_WhenUnauthorized_ShouldNotLeakSensitiveData()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
        MultipartFormDataContent form = CreateCoverForm("cover.jpg");

        // Act
        HttpResponseMessage response = await client.PutAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/cover", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateArtistCover_WithSQLInjectionInLibraryRouteId_ShouldRemainSecure(string maliciousLibraryId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        MultipartFormDataContent form = CreateCoverForm("cover.jpg");

        // Act
        HttpResponseMessage response = await client.PutAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/artists/{Guid.NewGuid()}/cover", form);

        // Assert
        await AssertCleanValidationErrorAsync(response, maliciousLibraryId, "LibraryIdCannotBeEmpty");
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateArtistCover_WithSQLInjectionInArtistRouteId_ShouldRemainSecure(string maliciousArtistId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        MultipartFormDataContent form = CreateCoverForm("cover.jpg");

        // Act
        HttpResponseMessage response = await client.PutAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Uri.EscapeDataString(maliciousArtistId)}/cover", form);

        // Assert
        await AssertCleanValidationErrorAsync(response, maliciousArtistId, "ArtistIdCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateArtistCover_WhenArtistDoesNotExist_ShouldReturnNotFoundWithoutLeakingInternals()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        MultipartFormDataContent form = CreateCoverForm("cover.jpg");

        // Act
        HttpResponseMessage response = await client.PutAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/cover", form);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.NotFound", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("ArtistNotFound", problemDetails.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UpdateArtistCover_WithMaliciousFileName_ShouldNotCorruptOrLeakData()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        MultipartFormDataContent form = CreateCoverForm("..\\..\\evil.jpg");

        // Act
        HttpResponseMessage response = await client.PutAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/cover", form);

        // Assert
        // The artist does not exist, so the handler returns a clean not found response before the uploaded file name is used.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateArtistCover_WhenUserDoesNotOwnTheLibrary_ShouldReturnForbidden()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        _seededUsernames.Add(ownerUsername);
        (Guid libraryId, Guid artistId) = await SeedLibraryAndArtistAsync(ownerId);

        HttpClient requesterClient = _apiFactory.CreateClient();
        (Guid _, string requesterUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(requesterClient);
        _seededUsernames.Add(requesterUsername);
        MultipartFormDataContent form = CreateCoverForm("cover.jpg");

        // Act
        HttpResponseMessage response = await requesterClient.PutAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/cover", form);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task UpdateArtistCover_WhenArtistBelongsToAnotherUsersLibrary_ShouldReturnNotFoundWithoutLeakingOrModifyingIt()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid actingUserId, string actingUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(actingUsername);
        (Guid ownLibraryId, _) = await SeedLibraryAndArtistAsync(actingUserId);

        // The artist lives in a library owned by another user, and is referenced through the acting user's own library route.
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        (_, Guid victimArtistId) = await SeedLibraryAndArtistAsync(victimUserId);
        MultipartFormDataContent form = CreateCoverForm("cover.jpg");

        // Act
        HttpResponseMessage response = await client.PutAsync($"/api/v1/libraries/{ownLibraryId}/artists/{victimArtistId}/cover", form);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.NotFound", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("ArtistNotFound", problemDetails.RootElement.GetProperty("detail").GetString());

        // The other user's artist is untouched.
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        ArtistEntity? storedArtist = await dbContext.Artists.FirstOrDefaultAsync(artist => artist.Id == victimArtistId);
        Assert.NotNull(storedArtist);
        Assert.Equal("Queen", storedArtist.Name);
    }

    /// <summary>
    /// Asserts that the response is a clean validation problem details that neither leaks internals nor echoes the injected payload.
    /// </summary>
    /// <param name="response">The HTTP response to assert.</param>
    /// <param name="injectedPayload">The injected payload that must never be echoed back.</param>
    /// <param name="expectedErrorCode">The validation error code expected in the response.</param>
    private static async Task AssertCleanValidationErrorAsync(HttpResponseMessage response, string injectedPayload, string expectedErrorCode)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(injectedPayload, content, StringComparison.Ordinal);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains(expectedErrorCode, content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a multipart form carrying a minimal cover payload with the specified file name.
    /// </summary>
    /// <param name="fileName">The name of the uploaded file.</param>
    /// <returns>A configured <see cref="MultipartFormDataContent"/> instance.</returns>
    private static MultipartFormDataContent CreateCoverForm(string fileName)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(Encoding.UTF8.GetBytes("fake cover payload"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "cover", fileName);
        return form;
    }

    /// <summary>
    /// Seeds a music library owned by <paramref name="userId"/>, together with an artist.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library and artist.</returns>
    private async Task<(Guid libraryId, Guid artistId)> SeedLibraryAndArtistAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Security Audio Library", libraryType: LibraryType.Music, contentLocations: []));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public void Dispose()
    {
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
