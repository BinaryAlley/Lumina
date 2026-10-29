#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.UpdatePlaylist;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Playlists.UpdatePlaylist;

/// <summary>
/// Contains integration tests for the <see cref="UpdatePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private const string PATH = "/api/v1/users/me/playlists";

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly UpdatePlaylistRequestFixture _updatePlaylistRequestFixture = new();
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededPlaylistIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdatePlaylistEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes the authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task UpdatePlaylist_WhenTheRequestIsValid_ShouldUpdateTheDetailsAndReturnThem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Old name", "Old description");
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(name: "New name", description: "New description");
        string url = $"{PATH}/{playlistId}";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains("Authorization", response.Headers.Vary);
        PlaylistDetailsResponse? playlist = await DeserializeAsync<PlaylistDetailsResponse>(response);
        Assert.NotNull(playlist);
        Assert.Equal(playlistId, playlist!.Id);
        Assert.Equal("New name", playlist.Name);
        Assert.Equal("New description", playlist.Description);
        Assert.Equal(PlaylistType.Music, playlist.Type);
        Assert.NotNull(playlist.UpdatedOnUtc);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PlaylistEntity storedPlaylist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Equal("New name", storedPlaylist.Name);
        Assert.Equal("New description", storedPlaylist.Description);
        Assert.Equal(userId, storedPlaylist.UpdatedBy);
        Assert.NotNull(storedPlaylist.UpdatedOnUtc);
        Assert.Equal(1, storedPlaylist.Version);
    }

    [Fact]
    public async Task UpdatePlaylist_WhenTheDescriptionIsOmitted_ShouldClearTheStoredDescription()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Named playlist", "Description to clear");
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(name: "Named playlist", includeDescription: false);
        string url = $"{PATH}/{playlistId}";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistDetailsResponse? playlist = await DeserializeAsync<PlaylistDetailsResponse>(response);
        Assert.NotNull(playlist);
        Assert.Null(playlist!.Description);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PlaylistEntity storedPlaylist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Null(storedPlaylist.Description);
    }

    [Fact]
    public async Task UpdatePlaylist_WhenTheNameHasTheMaximumLength_ShouldUpdateTheDetails()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Short name", null);
        string maximumLengthName = new('a', 255);
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(name: maximumLengthName);
        string url = $"{PATH}/{playlistId}";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistDetailsResponse? playlist = await DeserializeAsync<PlaylistDetailsResponse>(response);
        Assert.NotNull(playlist);
        Assert.Equal(maximumLengthName, playlist!.Name);
    }

    [Fact]
    public async Task UpdatePlaylist_WhenTheNameIsMissing_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Old name", null);
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(includeName: false);
        string url = $"{PATH}/{playlistId}";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistNameCannotBeEmpty");

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PlaylistEntity storedPlaylist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Equal("Old name", storedPlaylist.Name);
    }

    [Fact]
    public async Task UpdatePlaylist_WhenTheNameIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Old name", null);
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(name: string.Empty);
        string url = $"{PATH}/{playlistId}";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistNameCannotBeEmpty");
    }

    [Fact]
    public async Task UpdatePlaylist_WhenTheNameIsTooLong_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Old name", null);
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(name: new string('a', 256));
        string url = $"{PATH}/{playlistId}";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistNameMustBeMaximum255CharactersLong");
    }

    [Fact]
    public async Task UpdatePlaylist_WhenTheDescriptionIsTooLong_ShouldReturnValidationProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid playlistId = await SeedPlaylistAsync(userId, "Old name", null);
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(description: new string('a', 2001));
        string url = $"{PATH}/{playlistId}";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistDescriptionMustBeMaximum2000CharactersLong");
    }

    [Fact]
    public async Task UpdatePlaylist_WhenThePlaylistDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create();
        string url = $"{PATH}/{playlistId}";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", url);
    }

    [Fact]
    public async Task UpdatePlaylist_WhenThePlaylistBelongsToAnotherUser_ShouldReturnNotFoundAndKeepTheData()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherPlaylistId = await SeedPlaylistAsync(otherUserId, "Someone else's playlist", "Private");
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(name: "Hijacked", description: "Hijacked");
        string url = $"{PATH}/{otherPlaylistId}";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "General.NotFound", "PlaylistNotFound", url);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PlaylistEntity storedPlaylist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == otherPlaylistId);
        Assert.Equal("Someone else's playlist", storedPlaylist.Name);
        Assert.Equal("Private", storedPlaylist.Description);
        Assert.Equal(otherUserId, storedPlaylist.UserId);
    }

    [Fact]
    public async Task UpdatePlaylist_WhenThePlaylistIdIsNotAGuid_ShouldReturnValidationProblem()
    {
        // Arrange
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create();
        string url = $"{PATH}/not-a-guid";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistIdCannotBeEmpty");
    }

    [Fact]
    public async Task UpdatePlaylist_WhenThePlaylistIdIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create();
        string url = $"{PATH}/{Guid.Empty}";

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, request);

        // Assert
        await AssertValidationProblemAsync(response, url, "PlaylistIdCannotBeEmpty");
    }

    /// <summary>
    /// Seeds a playlist of <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the playlist.</param>
    /// <param name="name">The name of the playlist.</param>
    /// <param name="description">The description of the playlist, or <see langword="null"/>.</param>
    /// <returns>The Id of the seeded playlist.</returns>
    private async Task<Guid> SeedPlaylistAsync(Guid userId, string name, string? description)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid playlistId = Guid.NewGuid();
        dbContext.Playlists.Add(_playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            name: name,
            description: description ?? string.Empty,
            includeDescription: description is not null,
            type: PlaylistType.Music,
            entryCount: 0,
            version: 0,
            createdBy: userId,
            includeUpdatedOnUtc: true,
            updatedOnUtc: DateTime.UtcNow.AddHours(-1),
            updatedBy: userId));
        await dbContext.SaveChangesAsync();
        _seededPlaylistIds.Add(playlistId);
        return playlistId;
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and tracks it for cleanup.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: $"otheruser_{Guid.NewGuid()}", password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUserIds.Add(userId);
        return userId;
    }

    /// <summary>
    /// Gets the Id of the currently authenticated test user.
    /// </summary>
    /// <returns>The Id of the authenticated test user.</returns>
    private Guid GetCurrentUserId()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity user = dbContext.Users.First(candidate => candidate.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Deserializes the body of <paramref name="response"/> to <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the body to.</typeparam>
    /// <param name="response">The HTTP response to read.</param>
    /// <returns>The deserialized body.</returns>
    private async Task<TValue?> DeserializeAsync<TValue>(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TValue>(content, _jsonOptions);
    }

    /// <summary>
    /// Asserts that the response is a problem details document with the expected status, title and detail.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedStatus">The expected HTTP status.</param>
    /// <param name="expectedTitle">The expected problem title.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode expectedStatus, string expectedTitle, string expectedDetail, string expectedInstance)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)expectedStatus, problemDetails!["status"].GetInt32());
        Assert.Equal(expectedTitle, problemDetails["title"].GetString());
        Assert.Equal(expectedDetail, problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Asserts that the response is a validation problem details document carrying the expected error code.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCode">The expected validation error code.</param>
    private static async Task AssertValidationProblemAsync(HttpResponseMessage response, string expectedInstance, string expectedErrorCode)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        Assert.NotNull(problemDetails);
        Assert.Equal(422, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.False(string.IsNullOrEmpty(problemDetails["traceId"].GetString()));

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>();
        Assert.NotNull(errors);
        Assert.Contains(expectedErrorCode, errors!["General.Validation"]);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Removes the data seeded by the test and the API factory test user.
    /// </summary>
    public async Task DisposeAsync()
    {
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            List<Guid> playlistIds = [.. _seededPlaylistIds];
            dbContext.PlaylistEntries.RemoveRange(dbContext.PlaylistEntries.Where(entry => playlistIds.Contains(entry.PlaylistId)));
            dbContext.Playlists.RemoveRange(dbContext.Playlists.Where(playlist => playlistIds.Contains(playlist.Id)));
            dbContext.Users.RemoveRange(dbContext.Users.Where(user => _seededUserIds.Contains(user.Id)));
            await dbContext.SaveChangesAsync();
        }
        await _apiFactory.RemoveTestUserAsync();
    }
}
