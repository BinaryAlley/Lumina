#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.UpdateTrack;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.UpdateTrack;

/// <summary>
/// Contains integration tests for the <see cref="UpdateTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateTrackEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private static readonly string s_contentRootPath = Path.Combine(Path.GetTempPath(), "lumina-update-track-tests");
    private static readonly string s_outsideContentRootPath = Path.Combine(Path.GetTempPath(), "lumina-update-track-outside");

    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly UpdateTrackRequestFixture _requestTrackFixture = new();
    private readonly LoginRequestFixture _loginRequestFixture = new();
    private readonly MusicTrackMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly MusicWorkDtoFixture _musicWorkDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly UserRoleEntityFixture _userRoleEntityFixture = new();
    private readonly PasswordHashService _hashService = new();
    private readonly List<Guid> _seededLibraryIds = [];
    private readonly List<Guid> _seededContributorIds = [];
    private readonly List<Guid> _seededAdminUserIds = [];
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateTrackEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task UpdateTrack_WhenCalledByOwnerWithValidData_ShouldUpdateTrackAndPersistChanges()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        (Guid artistId, Guid albumId, Guid trackId) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        string updatedPath = Path.Combine(s_contentRootPath, $"love-of-my-life-{Guid.NewGuid():N}.flac");
        UpdateTrackRequest request = _requestTrackFixture.Create(
            path: updatedPath,
            metadata: _audioMetadataDtoFixture.Create(title: "Somebody to Love"),
            moods: [],
            isrcs: [],
            contributors: [],
            ratings: [],
            includeDiscNumber: false,
            includeScript: false,
            includeKey: false,
            includeBpm: false,
            includeWork: false,
            includeMusicBrainzRecordingId: false,
            includeMusicBrainzTrackId: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        TrackResponse? trackResponse = JsonSerializer.Deserialize<TrackResponse>(content, _jsonOptions);
        Assert.NotNull(trackResponse);
        Assert.Equal(trackId, trackResponse!.Id);
        Assert.Equal(libraryId, trackResponse.LibraryId);
        Assert.Equal(updatedPath, trackResponse.Path);
        Assert.Equal("Somebody to Love", trackResponse.Metadata!.Title);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        TrackEntity storedTrack = dbContext.Tracks.Single(track => track.Id == trackId);
        Assert.Equal("Somebody to Love", storedTrack.Title);
        Assert.Equal(updatedPath, storedTrack.Path);
    }

    [Fact]
    public async Task UpdateTrack_WhenCalledByAdminOnAnotherUsersLibrary_ShouldUpdateTrack()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid libraryId = await SeedLibraryAsync(otherUserId);
        (Guid artistId, Guid albumId, Guid trackId) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        UpdateTrackRequest request = _requestTrackFixture.Create(
            path: Path.Combine(s_contentRootPath, "dont-stop-me-now.flac"),
            metadata: _audioMetadataDtoFixture.Create(title: "Don't Stop Me Now"),
            moods: [],
            isrcs: [],
            contributors: [],
            ratings: [],
            includeDiscNumber: false,
            includeScript: false,
            includeKey: false,
            includeBpm: false,
            includeWork: false,
            includeMusicBrainzRecordingId: false,
            includeMusicBrainzTrackId: false);
        (HttpClient adminClient, _) = await CreateAdminClientAsync();

        // Act
        HttpResponseMessage response = await adminClient.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTrack_WhenTrackBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid libraryId = await SeedLibraryAsync(otherUserId);
        (Guid artistId, Guid albumId, Guid trackId) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        UpdateTrackRequest request = _requestTrackFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Unauthorized", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("NotAuthorized", problemDetails.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UpdateTrack_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        UpdateTrackRequest request = _requestTrackFixture.Create();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTrack_WhenRouteIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        (Guid artistId, Guid _, Guid trackId) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        UpdateTrackRequest request = _requestTrackFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/not-a-guid/tracks/{trackId}", request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "AlbumIdCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        UpdateTrackRequest request = _requestTrackFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}", request);

        // Assert
        await AssertNotFoundAsync(response, "ArtistNotFound");
    }

    [Fact]
    public async Task UpdateTrack_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        (Guid artistId, Guid _, Guid trackId) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        UpdateTrackRequest request = _requestTrackFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{Guid.NewGuid()}/tracks/{trackId}", request);

        // Assert
        await AssertNotFoundAsync(response, "AlbumNotFound");
    }

    [Fact]
    public async Task UpdateTrack_WhenTrackDoesNotExist_ShouldReturnTrackNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        (Guid artistId, Guid albumId, Guid _) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        UpdateTrackRequest request = _requestTrackFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{Guid.NewGuid()}", request);

        // Assert
        await AssertNotFoundAsync(response, "TrackNotFound");
    }

    [Fact]
    public async Task UpdateTrack_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundProblem()
    {
        // Arrange
        Guid unseededLibraryId = Guid.NewGuid();
        _seededLibraryIds.Add(unseededLibraryId);
        (Guid artistId, Guid albumId, Guid trackId) = await SeedArtistGraphAsync(unseededLibraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        UpdateTrackRequest request = _requestTrackFixture.Create();
        (HttpClient adminClient, _) = await CreateAdminClientAsync();

        // Act
        HttpResponseMessage response = await adminClient.PutAsJsonAsync($"/api/v1/libraries/{unseededLibraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        await AssertNotFoundAsync(response, "LibraryNotFound");
    }

    [Fact]
    public async Task UpdateTrack_WhenPathIsOutsideLibraryContentLocations_ShouldReturnPathError()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        (Guid artistId, Guid albumId, Guid trackId) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        UpdateTrackRequest request = _requestTrackFixture.Create(path: Path.Combine(s_outsideContentRootPath, "under-pressure.flac"));

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TrackPathMustBeWithinLibraryContentLocations");
    }

    [Fact]
    public async Task UpdateTrack_WhenAReferencedContributorDoesNotExist_ShouldReturnMediaContributorNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        (Guid artistId, Guid albumId, Guid trackId) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        UpdateTrackRequest request = _requestTrackFixture.Create(
            path: Path.Combine(s_contentRootPath, "somebody-to-love.flac"),
            contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)]);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        await AssertNotFoundAsync(response, "MediaContributorNotFound");
    }

    [Fact]
    public async Task UpdateTrack_WhenTwoContributorsHaveTheSameIdWithDifferentRoles_ShouldCreateTwoTrackLinks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        (Guid artistId, Guid albumId, Guid trackId) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        Guid contributorId = Guid.NewGuid();
        List<MediaContributorReferenceDto> duplicatedContributors =
        [
            _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Vocals),
            _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Guitar)
        ];
        await SeedContributorsAsync(duplicatedContributors);
        UpdateTrackRequest request = _requestTrackFixture.Create(
            path: Path.Combine(s_contentRootPath, "another-one-bites-the-dust.flac"),
            moods: [],
            isrcs: [],
            contributors: duplicatedContributors,
            ratings: []);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<TrackContributorEntity> links = [.. dbContext.Tracks
            .Include(track => track.Contributors)
            .Single(track => track.Id == trackId)
            .Contributors];
        Assert.Equal(2, links.Count);
        Assert.All(links, link => Assert.Equal(contributorId, link.MediaContributorId));
        Assert.Contains(links, link => link.Role == MediaContributorRole.Vocals);
        Assert.Contains(links, link => link.Role == MediaContributorRole.Guitar);
    }

    [Fact]
    public async Task UpdateTrack_WhenPathIsNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(includePath: false);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TrackPathCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenPathExceeds2048Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(path: new Faker().Random.String2(2049));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TrackPathMustBeMaximum2048CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenMetadataIsNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(includeMetadata: false);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "MetadataCannotBeNull");
    }

    [Fact]
    public async Task UpdateTrack_WhenTitleIsNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeTitle: false));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TitleCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenTitleExceeds255Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(title: new Faker().Random.String2(256)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TitleMustBeMaximum255CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenOriginalTitleExceeds255Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(256)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "OriginalTitleMustBeMaximum255CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenDescriptionExceeds2000Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "DescriptionMustBeMaximum2000CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenReleaseInfoIsNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeReleaseInfo: false));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReleaseInfoCannotBeNull");
    }

    [Fact]
    public async Task UpdateTrack_WhenOriginalReleaseYearIsOutOfRange_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 0, includeReReleaseDate: false, includeReReleaseYear: false)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "OriginalReleaseYearMustBeBetween1And9999");
    }

    [Fact]
    public async Task UpdateTrack_WhenReReleaseYearIsOutOfRange_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, includeReReleaseDate: false, reReleaseYear: 0)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReReleaseYearMustBeBetween1And9999");
    }

    [Fact]
    public async Task UpdateTrack_WhenReleaseVersionExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(51))));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReleaseVersionMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenOriginalReleaseDateAndYearDoNotMatch_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2021, includeReReleaseDate: false, includeReReleaseYear: false)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "OriginalReleaseDateAndYearMustMatch");
    }

    [Fact]
    public async Task UpdateTrack_WhenReReleaseDateAndYearDoNotMatch_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, reReleaseDate: new DateOnly(2020, 1, 1), reReleaseYear: 2021)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReReleaseDateAndYearMustMatch");
    }

    [Fact]
    public async Task UpdateTrack_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 2001, includeReReleaseDate: false, reReleaseYear: 2000)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReReleaseYearCannotBeEarlierThanOriginalReleaseYear");
    }

    [Fact]
    public async Task UpdateTrack_WhenReReleaseDateIsBeforeOriginalReleaseDate_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), includeOriginalReleaseYear: false, reReleaseDate: new DateOnly(2000, 1, 1), includeReReleaseYear: false)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReReleaseDateCannotBeEarlierThanOriginalReleaseDate");
    }

    [Fact]
    public async Task UpdateTrack_WhenGenresAreNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeGenres: false));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "GenresListCannotBeNull");
    }

    [Fact]
    public async Task UpdateTrack_WhenGenreNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "GenreNameCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenGenreNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "GenreNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenTagsAreNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeTags: false));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TagsListCannotBeNull");
    }

    [Fact]
    public async Task UpdateTrack_WhenTagNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TagNameCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenTagNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TagNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenLanguageCodeIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageCodeCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenLanguageCodeIsNotTwoCharacters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageCodeMustBe2CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenLanguageNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageNameCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenLanguageNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenLanguageNativeNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageNativeNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenOriginalLanguageCodeIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageCodeCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenOriginalLanguageNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenTrackNumberIsNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(includeTrackNumber: false);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TrackNumberMustBeGreaterThanZero");
    }

    [Fact]
    public async Task UpdateTrack_WhenTrackNumberIsZero_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(trackNumber: 0);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TrackNumberMustBeGreaterThanZero");
    }

    [Fact]
    public async Task UpdateTrack_WhenDiscNumberIsZero_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(discNumber: 0);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "DiscNumberMustBeGreaterThanZero");
    }

    [Fact]
    public async Task UpdateTrack_WhenScriptExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(script: new Faker().Random.String2(51));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ScriptMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenKeyIsInvalidEnum_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(key: (MusicKey)9999);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "UnknownMusicKey");
    }

    [Fact]
    public async Task UpdateTrack_WhenBpmIsZero_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(bpm: 0);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "BpmMustBeGreaterThanZero");
    }

    [Fact]
    public async Task UpdateTrack_WhenWorkExceeds255Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(work: _musicWorkDtoFixture.Create(title: new Faker().Random.String2(256)));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "WorkMustBeMaximum255CharactersLong");
    }

    [Fact]
    public async Task UpdateTrack_WhenMusicBrainzRecordingIdIsEmptyGuid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(musicBrainzRecordingId: Guid.Empty);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "MusicBrainzIdInvalidFormat");
    }

    [Fact]
    public async Task UpdateTrack_WhenMusicBrainzTrackIdIsEmptyGuid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(musicBrainzTrackId: Guid.Empty);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "MusicBrainzIdInvalidFormat");
    }

    [Fact]
    public async Task UpdateTrack_WhenMusicBrainzWorkIdIsEmptyGuid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(work: _musicWorkDtoFixture.Create(musicBrainzWorkId: Guid.Empty));

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "MusicBrainzIdInvalidFormat");
    }

    [Fact]
    public async Task UpdateTrack_WhenMoodNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(moods: [_moodDtoFixture.Create(name: string.Empty)]);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "MoodNameCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenIsrcValueIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(isrcs: [_isrcDtoFixture.Create(value: string.Empty)]);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "IsrcValueCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenContributorsAreNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(includeContributors: false);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ContributorsListCannotBeNull");
    }

    [Fact]
    public async Task UpdateTrack_WhenContributorIdIsEmptyGuid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Vocals)]);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "MediaContributorIdCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateTrack_WhenContributorRoleIsInvalid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(role: (MediaContributorRole)9999)]);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "UnknownMediaContributorRole");
    }

    [Fact]
    public async Task UpdateTrack_WhenRatingsAreNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(includeRatings: false);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "RatingsListCannotBeNull");
    }

    [Fact]
    public async Task UpdateTrack_WhenRatingValueIsNotPositive_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 0, maxValue: 5)]);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "RatingValueMustBePositive");
    }

    [Fact]
    public async Task UpdateTrack_WhenRatingValueIsGreaterThanMaxValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)]);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "RatingValueCannotBeGreaterThanMaxValue");
    }

    [Fact]
    public async Task UpdateTrack_WhenRatingMaxValueIsNotPositive_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, maxValue: 0)]);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "RatingMaxValueMustBePositive");
    }

    [Fact]
    public async Task UpdateTrack_WhenRatingVoteCountIsNegative_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateTrackRequest request = _requestTrackFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: -1)]);

        // Act
        HttpResponseMessage response = await PutTrackAsync(request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "RatingVoteCountMustBePositive");
    }

    [Fact]
    public async Task UpdateTrack_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        (Guid artistId, Guid albumId, Guid trackId) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        UpdateTrackRequest request = _requestTrackFixture.Create(path: Path.Combine(s_contentRootPath, "the-show-must-go-on.flac"));
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
            await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request, cts.Token)
        );
        Assert.Null(exception);
    }

    /// <summary>
    /// Updates the track described by the provided request, seeding the media library, the artist, the album, the track and the referenced media contributors.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <returns>The HTTP response of the PUT request.</returns>
    private async Task<HttpResponseMessage> PutTrackAsync(UpdateTrackRequest request)
    {
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        (Guid artistId, Guid albumId, Guid trackId) = await SeedArtistGraphAsync(libraryId, Path.Combine(s_contentRootPath, "bohemian-rhapsody.flac"));
        await SeedContributorsAsync(request.Contributors);
        return await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", request);
    }

    /// <summary>
    /// Asserts that the response is a not found problem details carrying the expected detail.
    /// </summary>
    /// <param name="response">The HTTP response to assert.</param>
    /// <param name="expectedDetail">The expected detail of the problem details.</param>
    private static async Task AssertNotFoundAsync(HttpResponseMessage response, string expectedDetail)
    {
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.NotFound", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal(expectedDetail, problemDetails.RootElement.GetProperty("detail").GetString());
    }

    /// <summary>
    /// Asserts that the response is an unprocessable entity problem details carrying the expected validation error codes.
    /// </summary>
    /// <param name="response">The HTTP response to assert.</param>
    /// <param name="expectedErrorCodes">The validation error codes expected in the response.</param>
    private async Task AssertUnprocessableEntityWithValidationErrors(HttpResponseMessage response, params string[] expectedErrorCodes)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.All(expectedErrorCodes, code => Assert.Contains(code, errors["General.Validation"]));
    }

    /// <summary>
    /// Seeds a media library of type music owned by <paramref name="userId"/>, whose single content location is the test content root path.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the media library.</param>
    /// <returns>The Id of the seeded media library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: [s_contentRootPath]);
        dbContext.Libraries.Add(library);
        await dbContext.SaveChangesAsync();
        _seededLibraryIds.Add(libraryId);
        return libraryId;
    }

    /// <summary>
    /// Seeds an artist that owns a single album, which in turn owns a single track, all belonging to <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="trackPath">The file system path of the seeded track.</param>
    /// <returns>The Ids of the seeded artist, album and track.</returns>
    private async Task<(Guid artistId, Guid albumId, Guid trackId)> SeedArtistGraphAsync(Guid libraryId, string trackPath)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, path: trackPath, includeMetadata: false);
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, libraryId: libraryId, tracks: [track], includeMetadata: false);
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, albums: [album], includeContributors: false);
        dbContext.Artists.Add(artist);
        await dbContext.SaveChangesAsync();
        return (artist.Id, albumId, trackId);
    }

    /// <summary>
    /// Seeds the media contributors referenced by the provided request, so that the handler finds them already existing.
    /// </summary>
    /// <param name="contributors">The contributors of the track request.</param>
    private async Task SeedContributorsAsync(List<MediaContributorReferenceDto>? contributors)
    {
        if (contributors is null || contributors.Count == 0)
            return;

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        foreach (Guid contributorId in contributors.Select(contributor => contributor.ContributorId).Distinct())
        {
            if (contributorId == Guid.Empty || await dbContext.MediaContributors.AnyAsync(contributor => contributor.Id == contributorId))
                continue;
            dbContext.MediaContributors.Add(_mediaContributorEntityFixture.Create(id: contributorId, displayName: contributorId.ToString()));
            _seededContributorIds.Add(contributorId);
        }
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUsernames.Add(username);
        return userId;
    }

    /// <summary>
    /// Creates and authenticates an admin user, by seeding it a dedicated Admin role directly, and returns a client carrying its bearer token.
    /// </summary>
    /// <returns>The authenticated admin client, together with the Id of the seeded admin user.</returns>
    private async Task<(HttpClient client, Guid adminUserId)> CreateAdminClientAsync()
    {
        Guid userId = Guid.NewGuid();
        string username = $"adminuser_{Guid.NewGuid()}";
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            UserEntity user = _userEntityFixture.Create(id: userId, username: username, password: _hashService.HashString("TestPass123!"));
            user.TotpSecret = null;
            dbContext.Users.Add(user);
            Guid roleId = Guid.NewGuid();
            RoleEntity role = _roleEntityFixture.Create(id: roleId, roleName: "Admin", createdBy: userId, createdOnUtc: DateTime.UtcNow);
            dbContext.Roles.Add(role);
            dbContext.UserRoles.Add(_userRoleEntityFixture.Create(userId: userId, user: user, roleId: roleId, role: role));
            await dbContext.SaveChangesAsync();
        }

        HttpClient client = _apiFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.{Random.Shared.Next(0, 255)}.{Random.Shared.Next(0, 255)}.{Random.Shared.Next(0, 255)}");
        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", _loginRequestFixture.Create(username: username, password: "TestPass123!"));
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? loginResult = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);
        _seededAdminUserIds.Add(userId);
        _seededUsernames.Add(username);
        return (client, userId);
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
    /// Removes the user identified by <paramref name="username"/> from the database.
    /// </summary>
    /// <param name="username">The username of the user to remove.</param>
    private async Task RemoveUserAsync(string? username)
    {
        if (string.IsNullOrEmpty(username))
            return;

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity? user = dbContext.Users.FirstOrDefault(candidate => candidate.Username == username);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            List<ArtistEntity> artists = [.. dbContext.Artists.Where(artist => _seededLibraryIds.Contains(artist.LibraryId))];
            dbContext.Artists.RemoveRange(artists);
            List<LibraryEntity> libraries = [.. dbContext.Libraries.Where(library => _seededLibraryIds.Contains(library.Id))];
            dbContext.Libraries.RemoveRange(libraries);
            List<MediaContributorEntity> contributors = [.. dbContext.MediaContributors.Where(contributor => _seededContributorIds.Contains(contributor.Id))];
            dbContext.MediaContributors.RemoveRange(contributors);
            foreach (Guid adminUserId in _seededAdminUserIds)
            {
                UserRoleEntity? userRole = dbContext.UserRoles.FirstOrDefault(userRole => userRole.UserId == adminUserId);
                if (userRole is not null)
                {
                    RoleEntity? role = dbContext.Roles.FirstOrDefault(role => role.Id == userRole.RoleId);
                    if (role is not null)
                        dbContext.Roles.Remove(role);
                    dbContext.UserRoles.Remove(userRole);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        foreach (string username in _seededUsernames)
            await RemoveUserAsync(username);
        await _apiFactory.RemoveTestUserAsync();
    }
}
