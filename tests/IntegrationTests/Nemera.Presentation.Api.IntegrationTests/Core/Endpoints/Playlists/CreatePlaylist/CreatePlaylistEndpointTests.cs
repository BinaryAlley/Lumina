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
using Lumina.Presentation.Api.Core.Endpoints.Playlists.CreatePlaylist;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Playlists.CreatePlaylist;

/// <summary>
/// Contains integration tests for the <see cref="CreatePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    private readonly CreatePlaylistRequestFixture _createPlaylistRequestFixture = new();
    private readonly List<Guid> _seededUserIds = [];
    private readonly List<Guid> _seededPlaylistIds = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public CreatePlaylistEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task CreatePlaylist_WhenTheRequestIsValid_ShouldCreateThePlaylistForTheCallerAndReturnIt()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(
            name: "Evening listening", description: "Quiet tracks for the end of the day", type: PlaylistType.Music);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains("Authorization", response.Headers.Vary);
        PlaylistResponse? playlist = await DeserializeAsync<PlaylistResponse>(response);
        Assert.NotNull(playlist);
        Assert.NotEqual(Guid.Empty, playlist!.Id);
        Assert.Equal("Evening listening", playlist.Name);
        Assert.Equal("Quiet tracks for the end of the day", playlist.Description);
        Assert.Equal(PlaylistType.Music, playlist.Type);
        Assert.Empty(playlist.Entries.Data);
        Assert.Equal(0, playlist.Entries.Count);
        Assert.NotEqual(default, playlist.CreatedOnUtc);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PlaylistEntity storedPlaylist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlist.Id);
        Assert.Equal(userId, storedPlaylist.UserId);
        Assert.Equal(userId, storedPlaylist.CreatedBy);
        Assert.Equal("Evening listening", storedPlaylist.Name);
        Assert.Equal("Quiet tracks for the end of the day", storedPlaylist.Description);
        Assert.Equal(PlaylistType.Music, storedPlaylist.Type);
        Assert.Equal(0, storedPlaylist.EntryCount);
        Assert.Equal(0, storedPlaylist.Version);
        Assert.False(await dbContext.PlaylistEntries.AnyAsync(entry => entry.PlaylistId == playlist.Id));
    }

    [Fact]
    public async Task CreatePlaylist_WhenTheDescriptionIsOmitted_ShouldCreateThePlaylistWithoutADescription()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(name: "No description", includeDescription: false);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistResponse? playlist = await DeserializeAsync<PlaylistResponse>(response);
        Assert.NotNull(playlist);
        Assert.Null(playlist!.Description);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PlaylistEntity storedPlaylist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlist.Id);
        Assert.Null(storedPlaylist.Description);
    }

    [Fact]
    public async Task CreatePlaylist_WhenTheNameHasTheMaximumLength_ShouldCreateThePlaylist()
    {
        // Arrange
        string maximumLengthName = new('a', 255);
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(name: maximumLengthName);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistResponse? playlist = await DeserializeAsync<PlaylistResponse>(response);
        Assert.NotNull(playlist);
        Assert.Equal(maximumLengthName, playlist!.Name);
    }

    [Fact]
    public async Task CreatePlaylist_WhenTheNameIsMissing_ShouldReturnValidationProblem()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(includeName: false);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        await AssertValidationProblemAsync(response, PATH, "PlaylistNameCannotBeEmpty");
    }

    [Fact]
    public async Task CreatePlaylist_WhenTheNameIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(name: string.Empty);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        await AssertValidationProblemAsync(response, PATH, "PlaylistNameCannotBeEmpty");
    }

    [Fact]
    public async Task CreatePlaylist_WhenTheNameIsTooLong_ShouldReturnValidationProblem()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(name: new string('a', 256));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        await AssertValidationProblemAsync(response, PATH, "PlaylistNameMustBeMaximum255CharactersLong");
    }

    [Fact]
    public async Task CreatePlaylist_WhenTheDescriptionIsTooLong_ShouldReturnValidationProblem()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(description: new string('a', 2001));

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        await AssertValidationProblemAsync(response, PATH, "PlaylistDescriptionMustBeMaximum2000CharactersLong");
    }

    [Fact]
    public async Task CreatePlaylist_WhenTheTypeIsMissing_ShouldReturnValidationProblem()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(includeType: false);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        await AssertValidationProblemAsync(response, PATH, "PlaylistTypeIsInvalid");
    }

    [Fact]
    public async Task CreatePlaylist_WhenTheTypeIsNotADefinedMember_ShouldReturnValidationProblem()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(type: (PlaylistType)99);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        await AssertValidationProblemAsync(response, PATH, "PlaylistTypeIsInvalid");
    }

    [Fact]
    public async Task CreatePlaylist_WhenAnotherUserHasPlaylists_ShouldCreateThePlaylistOnlyForTheCaller()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid otherPlaylistId = await SeedPlaylistAsync(otherUserId, "Someone else's playlist");
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(name: "Mine");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(PATH, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PlaylistResponse? playlist = await DeserializeAsync<PlaylistResponse>(response);
        Assert.NotNull(playlist);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PlaylistEntity storedPlaylist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == playlist!.Id);
        Assert.Equal(GetCurrentUserId(), storedPlaylist.UserId);
        PlaylistEntity otherPlaylist = await dbContext.Playlists.AsNoTracking().FirstAsync(candidate => candidate.Id == otherPlaylistId);
        Assert.Equal(otherUserId, otherPlaylist.UserId);
        Assert.Equal("Someone else's playlist", otherPlaylist.Name);
    }

    /// <summary>
    /// Seeds a playlist of <paramref name="userId"/> and tracks it for cleanup.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the playlist.</param>
    /// <param name="name">The name of the playlist.</param>
    /// <returns>The Id of the seeded playlist.</returns>
    private async Task<Guid> SeedPlaylistAsync(Guid userId, string name)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid playlistId = Guid.NewGuid();
        dbContext.Playlists.Add(_playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            name: name,
            type: PlaylistType.Music,
            entryCount: 0,
            version: 0,
            createdBy: userId));
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
