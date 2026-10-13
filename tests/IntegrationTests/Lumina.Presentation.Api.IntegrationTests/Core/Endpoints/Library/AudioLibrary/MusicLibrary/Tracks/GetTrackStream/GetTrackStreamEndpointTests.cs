#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackStream;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackStream;

/// <summary>
/// Contains integration tests for the <see cref="GetTrackStreamEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackStreamEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly string _contentRootPath = Path.Combine(Path.GetTempPath(), $"lumina-track-stream-tests-{Guid.NewGuid():N}");
    private readonly List<string> _temporaryFiles = [];
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackStreamEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetTrackStreamEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes the authenticated API client and the temporary content root of the test.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
        Directory.CreateDirectory(_contentRootPath);
    }

    [Fact]
    public async Task GetTrackStream_WhenTrackBelongsToAnOwnedLibrary_ShouldStreamTheWholeAudioFile()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        byte[] audioBytes = CreateAudioBytes();
        string trackPath = WriteContentFile("bohemian-rhapsody.mp3", audioBytes);
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("audio/mpeg", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal((long)audioBytes.Length, response.Content.Headers.ContentLength!.Value);
        Assert.Contains("bytes", response.Headers.AcceptRanges);
        Assert.NotNull(response.Headers.ETag);
        Assert.NotNull(response.Content.Headers.LastModified);
        Assert.Contains("nosniff", response.Headers.GetValues("X-Content-Type-Options"));
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.Contains("Authorization", response.Headers.Vary);
        Assert.Equal(audioBytes, await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task GetTrackStream_WhenARangeIsRequested_ShouldStreamOnlyTheRequestedRange()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        byte[] audioBytes = CreateAudioBytes();
        string trackPath = WriteContentFile("love-of-my-life.mp3", audioBytes);
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);
        using HttpRequestMessage request = new(HttpMethod.Get, $"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream");
        request.Headers.Range = new RangeHeaderValue(100, 199);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal("audio/mpeg", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(100L, response.Content.Headers.ContentLength!.Value);
        Assert.Contains("bytes", response.Headers.AcceptRanges);
        Assert.Equal(100L, response.Content.Headers.ContentRange!.From!.Value);
        Assert.Equal(199L, response.Content.Headers.ContentRange!.To!.Value);
        Assert.Equal((long)audioBytes.Length, response.Content.Headers.ContentRange!.Length!.Value);
        Assert.Equal(audioBytes[100..200], await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task GetTrackStream_WhenTrackIsRequestedThroughDifferentAlbum_ShouldReturnTrackNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string trackPath = WriteContentFile("you-re-my-best-friend.mp3", CreateAudioBytes());
        (Guid libraryId, Guid artistId, _, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{Guid.NewGuid()}/tracks/{trackId}/stream");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = await ReadProblemDetailsAsync(response);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("TrackNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetTrackStream_WhenAudioFileIsMissing_ShouldReturnTrackFileNotFoundProblemWithoutLeakingThePath()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        string trackPath = Path.Combine(_contentRootPath, "missing-file.mp3");
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, trackPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(trackPath, content, StringComparison.OrdinalIgnoreCase);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("TrackFileNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetTrackStream_WhenAudioFileIsOutsideTheLibraryContentLocations_ShouldReturnTrackFileNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        byte[] audioBytes = CreateAudioBytes();
        string outsideFilePath = Path.Combine(Path.GetTempPath(), $"lumina-track-stream-outside-{Guid.NewGuid():N}.mp3");
        await File.WriteAllBytesAsync(outsideFilePath, audioBytes);
        _temporaryFiles.Add(outsideFilePath);
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(userId, outsideFilePath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(outsideFilePath, content, StringComparison.OrdinalIgnoreCase);
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("TrackFileNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task GetTrackStream_WhenTrackBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        string trackPath = WriteContentFile("news-of-the-world.mp3", CreateAudioBytes());
        (Guid libraryId, Guid artistId, Guid albumId, Guid trackId) = await SeedLibraryArtistAlbumAndTrackAsync(otherUserId, trackPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.Unauthorized", problemDetails!["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task GetTrackStream_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/stream");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Seeds a library owned by <paramref name="userId"/>, an artist of that library, an album of that artist and a track of that album.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="trackPath">The file system path of the seeded track.</param>
    /// <returns>The Ids of the seeded library, artist, album and track.</returns>
    private async Task<(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)> SeedLibraryArtistAlbumAndTrackAsync(Guid userId, string trackPath)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: [_contentRootPath]));
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: libraryId, name: "Queen"));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, title: "A Night at the Opera"));
        dbContext.Tracks.Add(_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: trackPath, title: "A Track", includeMetadata: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, artistId, albumId, trackId);
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded user and its username.</returns>
    private async Task<(Guid userId, string username)> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        return (userId, username);
    }

    /// <summary>
    /// Writes an audio file with the provided <paramref name="audioBytes"/> inside the content root of the test, and returns its full path.
    /// </summary>
    /// <param name="fileName">The name of the audio file to write.</param>
    /// <param name="audioBytes">The bytes of the audio file to write.</param>
    /// <returns>The full path of the written audio file.</returns>
    private string WriteContentFile(string fileName, byte[] audioBytes)
    {
        string filePath = Path.Combine(_contentRootPath, fileName);
        File.WriteAllBytes(filePath, audioBytes);
        return filePath;
    }

    /// <summary>
    /// Creates a deterministic byte array that stands in for the content of an audio file.
    /// </summary>
    /// <returns>The created bytes.</returns>
    private static byte[] CreateAudioBytes()
    {
        return [.. Enumerable.Range(0, 4096).Select(index => (byte)(index % 256))];
    }

    /// <summary>
    /// Reads the problem details of a response.
    /// </summary>
    /// <param name="response">The HTTP response to read.</param>
    /// <returns>The deserialized problem details.</returns>
    private async Task<Microsoft.AspNetCore.Mvc.ProblemDetails?> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
    }

    /// <summary>
    /// Gets the Id of the currently authenticated test user.
    /// </summary>
    /// <returns>The Id of the authenticated test user.</returns>
    private Guid GetCurrentUserId()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity user = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Disposes API factory resources and the temporary files of the test.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (Directory.Exists(_contentRootPath))
            Directory.Delete(_contentRootPath, recursive: true);
        foreach (string temporaryFile in _temporaryFiles)
        {
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }
        await _apiFactory.RemoveTestUserAsync();
    }
}
