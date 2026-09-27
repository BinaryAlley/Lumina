#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.AddTrack;

/// <summary>
/// Contains integration tests for the <see cref="AddTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddTrackEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly AddTrackRequestFixture _requestTrackFixture = new();
    private readonly AudioMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly string _libraryContentLocation = Path.GetTempPath();
    private Guid _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddTrackEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client, along with a music library owned by the authenticated user that the added tracks belong to.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();

        _libraryId = Guid.NewGuid();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = dbContext.Users.Single(user => user.Username == _apiFactory.TestUsername).Id;
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: _libraryId, userId: userId, title: "Queen Library", libraryType: LibraryType.Music, contentLocations: [_libraryContentLocation]));
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task AddTrack_WhenCalledWithValidData_ShouldAddTrack()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        ReleaseInfoDto releaseInfo = _releaseInfoDtoFixture.Create(
            originalReleaseDate: new DateOnly(1975, 10, 31),
            originalReleaseYear: 1975,
            reReleaseDate: new DateOnly(2000, 1, 1),
            reReleaseYear: 2000,
            releaseCountry: ReleaseCountry.GB,
            releaseVersion: "Original");
        AudioMetadataDto metadata = _audioMetadataDtoFixture.Create(
            title: "Bohemian Rhapsody",
            originalTitle: "Bohemian Rhapsody",
            description: "A song by the British rock band Queen.",
            releaseInfo: releaseInfo,
            language: _languageInfoDtoFixture.Create(languageCode: "en", languageName: "English", nativeName: "English"),
            originalLanguage: _languageInfoDtoFixture.Create(languageCode: "en", languageName: "English", nativeName: "English"),
            tags: [_tagDtoFixture.Create(name: "classic"), _tagDtoFixture.Create(name: "epic")],
            genres: [_genreDtoFixture.Create(name: "Rock"), _genreDtoFixture.Create(name: "Progressive Rock")],
            durationInSeconds: 354,
            sampleRate: 44100,
            channels: 2,
            bitDepth: 16,
            audioCodec: "FLAC",
            bitrate: 980);
        AddTrackRequest trackRequest = _requestTrackFixture.Create(
            path: Path.Combine(_libraryContentLocation, $"{Guid.NewGuid():N}.flac"),
            metadata: metadata,
            trackNumber: 1,
            discNumber: 1,
            script: "Latn",
            key: MusicKey.CMajor,
            bpm: 72,
            work: "Bohemian Rhapsody",
            musicBrainzRecordingId: Guid.NewGuid(),
            musicBrainzTrackId: Guid.NewGuid(),
            musicBrainzWorkId: Guid.NewGuid(),
            moods: [_moodDtoFixture.Create(name: "dramatic"), _moodDtoFixture.Create(name: "anxious")],
            isrcs: [_isrcDtoFixture.Create(value: "GBUM71029604")],
            contributors:
            [
                _mediaContributorReferenceDtoFixture.Create(role: MediaContributorRole.Vocals),
                _mediaContributorReferenceDtoFixture.Create(role: MediaContributorRole.Guitar)
            ],
            ratings:
            [
                _audioRatingDtoFixture.Create(value: 4.5m, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 2345),
                _audioRatingDtoFixture.Create(value: 4.8m, maxValue: 5, source: AudioRatingSource.LastFm, voteCount: 1234)
            ]);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, trackRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        TrackResponse? trackResponse = await response.Content.ReadFromJsonAsync<TrackResponse>(_jsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(trackResponse);
        Assert.Equal(albumId, trackResponse!.AlbumId);
        Assert.Equal(_libraryId, trackResponse.LibraryId);
        Assert.Equal(trackRequest.Path, trackResponse.Path);
        Assert.Equal(trackRequest.TrackNumber, trackResponse.TrackNumber);
        Assert.Equal(trackRequest.DiscNumber, trackResponse.DiscNumber);
        Assert.Equal(trackRequest.Script, trackResponse.Script);
        Assert.Equal(trackRequest.Key, trackResponse.Key);
        Assert.Equal(trackRequest.Bpm, trackResponse.Bpm);
        Assert.Equal(trackRequest.Work, trackResponse.Work);
        Assert.Equal(trackRequest.MusicBrainzRecordingId, trackResponse.MusicBrainzRecordingId);
        Assert.Equal(trackRequest.MusicBrainzTrackId, trackResponse.MusicBrainzTrackId);
        Assert.Equal(trackRequest.MusicBrainzWorkId, trackResponse.MusicBrainzWorkId);

        // metadata checks
        Assert.Equal(metadata.Title, trackResponse.Metadata.Title);
        Assert.Equal(metadata.OriginalTitle, trackResponse.Metadata.OriginalTitle);
        Assert.Equal(metadata.Description, trackResponse.Metadata.Description);
        Assert.Equal(metadata.DurationInSeconds, trackResponse.Metadata.DurationInSeconds);
        Assert.Equal(metadata.SampleRate, trackResponse.Metadata.SampleRate);
        Assert.Equal(metadata.Channels, trackResponse.Metadata.Channels);
        Assert.Equal(metadata.BitDepth, trackResponse.Metadata.BitDepth);
        Assert.Equal(metadata.AudioCodec, trackResponse.Metadata.AudioCodec);
        Assert.Equal(metadata.Bitrate, trackResponse.Metadata.Bitrate);

        // release info checks
        Assert.Equal(releaseInfo.OriginalReleaseDate, trackResponse.Metadata.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(releaseInfo.OriginalReleaseYear, trackResponse.Metadata.ReleaseInfo.OriginalReleaseYear);
        Assert.Equal(releaseInfo.ReReleaseDate, trackResponse.Metadata.ReleaseInfo.ReReleaseDate);
        Assert.Equal(releaseInfo.ReReleaseYear, trackResponse.Metadata.ReleaseInfo.ReReleaseYear);
        Assert.Equal(releaseInfo.ReleaseCountry, trackResponse.Metadata.ReleaseInfo.ReleaseCountry);
        Assert.Equal(releaseInfo.ReleaseVersion, trackResponse.Metadata.ReleaseInfo.ReleaseVersion);

        // language checks
        Assert.Equal(metadata.Language!.LanguageCode, trackResponse.Metadata.Language!.LanguageCode);
        Assert.Equal(metadata.Language.LanguageName, trackResponse.Metadata.Language.LanguageName);
        Assert.Equal(metadata.Language.NativeName, trackResponse.Metadata.Language.NativeName);
        Assert.Equal(metadata.OriginalLanguage!.LanguageCode, trackResponse.Metadata.OriginalLanguage!.LanguageCode);
        Assert.Equal(metadata.OriginalLanguage.LanguageName, trackResponse.Metadata.OriginalLanguage.LanguageName);
        Assert.Equal(metadata.OriginalLanguage.NativeName, trackResponse.Metadata.OriginalLanguage.NativeName);

        // genres and tags checks
        Assert.Equal(
            metadata.Genres!.Select(genre => genre.Name).OrderBy(name => name),
            trackResponse.Metadata.Genres!.Select(genre => genre.Name).OrderBy(name => name));
        Assert.Equal(
            metadata.Tags!.Select(tag => tag.Name).OrderBy(name => name),
            trackResponse.Metadata.Tags!.Select(tag => tag.Name).OrderBy(name => name));

        // moods, ISRCs, contributors and ratings checks
        Assert.Equal(
            trackRequest.Moods!.Select(mood => mood.Name).OrderBy(name => name),
            trackResponse.Moods!.Select(mood => mood.Name).OrderBy(name => name));
        Assert.Equal(
            trackRequest.Isrcs!.Select(isrc => isrc.Value).OrderBy(value => value),
            trackResponse.Isrcs!.Select(isrc => isrc.Value).OrderBy(value => value));
        Assert.Equal(trackRequest.Contributors!.Count, trackResponse.Contributors!.Count);
        Assert.Contains(trackResponse.Contributors, contributor => contributor.Role == MediaContributorRole.Vocals);
        Assert.Contains(trackResponse.Contributors, contributor => contributor.Role == MediaContributorRole.Guitar);
        Assert.Equal(trackRequest.Ratings!.Count, trackResponse.Ratings!.Count);
        Assert.Contains(trackResponse.Ratings, rating => rating.Source == AudioRatingSource.MusicBrainz && rating.Value == 4.5m && rating.MaxValue == 5 && rating.VoteCount == 2345);
        Assert.Contains(trackResponse.Ratings, rating => rating.Source == AudioRatingSource.LastFm && rating.Value == 4.8m && rating.MaxValue == 5 && rating.VoteCount == 1234);

        // check Location header
        Assert.NotNull(response.Headers.Location);
        string locationUri = response.Headers.Location!.ToString();
        Assert.EndsWith($"/api/v1/libraries/{_libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackResponse.Id}", locationUri);
    }

    [Fact]
    public async Task AddTrack_WhenPathIsMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(includePath: false);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.TrackPathCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenPathIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(path: "/" + new Faker().Random.String2(2048) + ".flac");

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.TrackPathMustBeMaximum2048CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenMetadataIsMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(includeMetadata: false);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.MetadataCannotBeNull.Description);
    }

    [Fact]
    public async Task AddTrack_WhenTitleIsMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeTitle: false));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.TitleCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenTitleIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(title: new Faker().Random.String2(300)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.TitleMustBeMaximum255CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenOriginalTitleIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(300)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenDescriptionIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenReleaseInfoIsMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeReleaseInfo: false));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.ReleaseInfoCannotBeNull.Description);
    }

    [Fact]
    public async Task AddTrack_WhenOriginalReleaseYearIsInvalid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 10000)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999.Description);
    }

    [Fact]
    public async Task AddTrack_WhenReReleaseYearIsInvalid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 10000)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.ReReleaseYearMustBeBetween1And9999.Description);
    }

    [Fact]
    public async Task AddTrack_WhenReleaseVersionIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(100))));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenOriginalReleaseDateAndYearDoNotMatch_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2019)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.OriginalReleaseDateAndYearMustMatch.Description);
    }

    [Fact]
    public async Task AddTrack_WhenReReleaseDateAndYearDoNotMatch_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2020)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.ReReleaseDateAndYearMustMatch.Description);
    }

    [Fact]
    public async Task AddTrack_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2001, reReleaseYear: 2000, includeReReleaseDate: false)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear.Description);
    }

    [Fact]
    public async Task AddTrack_WhenReReleaseDateIsBeforeOriginalReleaseDate_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), reReleaseDate: new DateOnly(2000, 1, 1))));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Description);
    }

    [Fact]
    public async Task AddTrack_WhenGenresAreMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeGenres: false));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.GenresListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddTrack_WhenGenreNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenGenreNameIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.GenreNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenTagsAreMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeTags: false));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.TagsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddTrack_WhenTagNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.TagNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenTagNameIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.TagNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenLanguageCodeIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenLanguageCodeIsInvalid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(10))));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.LanguageCodeMustBe2CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenLanguageNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.LanguageNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenLanguageNameIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(100))));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenLanguageNativeNameIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(100))));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenOriginalLanguageCodeIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenOriginalLanguageCodeIsInvalid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(10))));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.LanguageCodeMustBe2CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenOriginalLanguageNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.LanguageNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenOriginalLanguageNameIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(100))));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenOriginalLanguageNativeNameIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(100))));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenMusicBrainzRecordingIdIsEmptyGuid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(musicBrainzRecordingId: Guid.Empty);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.MusicBrainzIdInvalidFormat.Description);
    }

    [Fact]
    public async Task AddTrack_WhenMusicBrainzTrackIdIsEmptyGuid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(musicBrainzTrackId: Guid.Empty);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.MusicBrainzIdInvalidFormat.Description);
    }

    [Fact]
    public async Task AddTrack_WhenMusicBrainzWorkIdIsEmptyGuid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(musicBrainzWorkId: Guid.Empty);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.MusicBrainzIdInvalidFormat.Description);
    }

    [Fact]
    public async Task AddTrack_WhenTrackNumberIsMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(includeTrackNumber: false);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.TrackNumberMustBeGreaterThanZero.Description);
    }

    [Fact]
    public async Task AddTrack_WhenTrackNumberIsNotPositive_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(trackNumber: 0);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.TrackNumberMustBeGreaterThanZero.Description);
    }

    [Fact]
    public async Task AddTrack_WhenDiscNumberIsNotPositive_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(discNumber: 0);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.DiscNumberMustBeGreaterThanZero.Description);
    }

    [Fact]
    public async Task AddTrack_WhenScriptIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(script: new Faker().Random.String2(100));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.ScriptMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenKeyIsNotDefined_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(key: (MusicKey)999);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.UnknownMusicKey.Description);
    }

    [Fact]
    public async Task AddTrack_WhenBpmIsNotPositive_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(bpm: 0);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.BpmMustBeGreaterThanZero.Description);
    }

    [Fact]
    public async Task AddTrack_WhenWorkIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(work: new Faker().Random.String2(300));

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.WorkMustBeMaximum255CharactersLong.Description);
    }

    [Fact]
    public async Task AddTrack_WhenMoodNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(moods: [_moodDtoFixture.Create(includeName: false)]);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.MoodNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenIsrcValueIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(isrcs: [_isrcDtoFixture.Create(includeValue: false)]);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.IsrcValueCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenContributorsAreMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(includeContributors: false);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.MediaContributor.ContributorsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddTrack_WhenContributorIdIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Vocals)]);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.MediaContributor.MediaContributorIdCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenContributorRoleIsNotDefined_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: (MediaContributorRole)999)]);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.MediaContributor.UnknownMediaContributorRole.Description);
    }

    [Fact]
    public async Task AddTrack_WhenRatingsAreMissing_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(includeRatings: false);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.RatingsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddTrack_WhenRatingValueIsNotPositive_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: -3, maxValue: 5)]);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.RatingValueMustBePositive.Description);
    }

    [Fact]
    public async Task AddTrack_WhenRatingValueIsGreaterThanMaxValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)]);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Description);
    }

    [Fact]
    public async Task AddTrack_WhenRatingMaxValueIsNotPositive_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, maxValue: -3)]);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.RatingMaxValueMustBePositive.Description);
    }

    [Fact]
    public async Task AddTrack_WhenRatingVoteCountIsNegative_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: -3)]);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Metadata.RatingVoteCountMustBePositive.Description);
    }

    [Fact]
    public async Task AddTrack_WhenTrackPathIsNotWithinTheLibraryContentLocations_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        string pathOutsideTheLibrary = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "..", "lumina-tracks-outside", $"{Guid.NewGuid():N}.flac"));
        AddTrackRequest request = _requestTrackFixture.Create(path: pathOutsideTheLibrary);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, BuildInstancePath(artistId, albumId), Errors.Music.TrackPathMustBeWithinLibraryContentLocations.Description);
    }

    [Fact]
    public async Task AddTrack_WhenArtistDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AddTrackRequest request = _requestTrackFixture.Create();

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, albumId, request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", Errors.Music.ArtistNotFound.Description, BuildInstancePath(artistId, albumId), "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task AddTrack_WhenArtistBelongsToAnotherLibrary_ShouldReturnArtistNotFoundWithoutDisclosingIt()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        Guid otherLibraryId = Guid.NewGuid();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{otherLibraryId}/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", Errors.Music.ArtistNotFound.Description, $"/api/v1/libraries/{otherLibraryId}/artists/{artistId}/albums/{albumId}/tracks", "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task AddTrack_WhenAlbumDoesNotExistInTheArtist_ShouldReturnNotFound()
    {
        // Arrange
        (Guid artistId, _) = await SeedArtistAndAlbumAsync();
        Guid missingAlbumId = Guid.NewGuid();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await PostTrackAsync(artistId, missingAlbumId, request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", Errors.Music.AlbumNotFound.Description, BuildInstancePath(artistId, missingAlbumId), "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task AddTrack_WhenTheLibraryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        // only an admin reaches the library lookup of a library that does not exist, because a regular user is rejected by the ownership policy before it
        HttpClient adminClient = await _apiFactory.CreateAuthenticatedAdminClientAsync();
        Guid missingLibraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        using (IServiceScope seedScope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext seedDbContext = seedScope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            seedDbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: missingLibraryId, name: "Queen", includeAlbums: false, includeContributors: false));
            seedDbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: missingLibraryId, title: "A Night at the Opera", includeTracks: false, includeMetadata: false));
            await seedDbContext.SaveChangesAsync();
        }
        AddTrackRequest request = _requestTrackFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await adminClient.PostAsJsonAsync($"/api/v1/libraries/{missingLibraryId}/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", Errors.Library.LibraryNotFound.Description, $"/api/v1/libraries/{missingLibraryId}/artists/{artistId}/albums/{albumId}/tracks", "https://tools.ietf.org/html/rfc9110#section-15.5.5");
        await _apiFactory.RemoveTestUserAsync();
    }

    [Fact]
    public async Task AddTrack_WhenAReferencedContributorDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create()]);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", Errors.MediaContributor.MediaContributorNotFound.Description, BuildInstancePath(artistId, albumId), "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    [Fact]
    public async Task AddTrack_WhenUserDoesNotOwnTheLibrary_ShouldReturnForbidden()
    {
        // Arrange
        (Guid ownerId, _) = await SeedOtherUserAsync();
        Guid otherLibraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        using (IServiceScope seedScope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext seedDbContext = seedScope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            seedDbContext.Libraries.Add(_libraryEntityFixture.Create(id: otherLibraryId, userId: ownerId, title: "Queen Reissues", libraryType: LibraryType.Music, contentLocations: [_libraryContentLocation]));
            seedDbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: otherLibraryId, name: "Queen", includeAlbums: false, includeContributors: false));
            seedDbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: otherLibraryId, title: "News of the World", includeTracks: false, includeMetadata: false));
            await seedDbContext.SaveChangesAsync();
        }
        AddTrackRequest request = _requestTrackFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{otherLibraryId}/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Forbidden, "General.Unauthorized", "NotAuthorized", $"/api/v1/libraries/{otherLibraryId}/artists/{artistId}/albums/{albumId}/tracks", "https://tools.ietf.org/html/rfc9110#section-15.5.4");
    }

    [Fact]
    public async Task AddTrack_WhenLibraryIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/not-a-guid/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, $"/api/v1/libraries/not-a-guid/artists/{artistId}/albums/{albumId}/tracks", Errors.Library.LibraryIdCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenArtistIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/artists/not-a-guid/albums/{albumId}/tracks", request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, $"/api/v1/libraries/{_libraryId}/artists/not-a-guid/albums/{albumId}/tracks", Errors.Music.ArtistIdCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenAlbumIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/artists/{artistId}/albums/not-a-guid/tracks", request);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, $"/api/v1/libraries/{_libraryId}/artists/{artistId}/albums/not-a-guid/tracks", Errors.Music.AlbumIdCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddTrack_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/artists/{artistId}/albums/{albumId}/tracks", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddTrack_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: []);
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
            await _client.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/artists/{artistId}/albums/{albumId}/tracks", request, cts.Token));
        Assert.Null(exception);
    }

    [Fact]
    public async Task AddTrack_WhenCancellationTokenIsCanceled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        (Guid artistId, Guid albumId) = await SeedArtistAndAlbumAsync();
        AddTrackRequest request = _requestTrackFixture.Create(contributors: []);
        using CancellationTokenSource cts = new();

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
        {
            cts.Cancel();
            await _client.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/artists/{artistId}/albums/{albumId}/tracks", request, cts.Token);
        });
        Assert.IsType<TaskCanceledException>(exception);
    }

    /// <summary>
    /// Posts a request to add a track to the album identified by the provided route identifiers, seeding the referenced media contributors.
    /// </summary>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track is added to.</param>
    /// <param name="request">The request to post.</param>
    /// <returns>The HTTP response of the POST request.</returns>
    private async Task<HttpResponseMessage> PostTrackAsync(Guid artistId, Guid albumId, AddTrackRequest request)
    {
        await SeedContributorsAsync(request.Contributors);
        return await _client.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/artists/{artistId}/albums/{albumId}/tracks", request);
    }

    /// <summary>
    /// Seeds an artist of the owned library, together with one of its albums.
    /// </summary>
    /// <returns>The Ids of the seeded artist and album.</returns>
    private async Task<(Guid artistId, Guid albumId)> SeedArtistAndAlbumAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        dbContext.Artists.Add(_artistEntityFixture.Create(id: artistId, libraryId: _libraryId, name: "Queen", includeAlbums: false, includeContributors: false));
        dbContext.Albums.Add(_albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: _libraryId, title: "A Night at the Opera", includeTracks: false, includeMetadata: false));
        await dbContext.SaveChangesAsync();
        return (artistId, albumId);
    }

    /// <summary>
    /// Seeds the media contributors referenced by the provided track contributors, so that the handler finds them already existing.
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
        }
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user.
    /// </summary>
    /// <returns>The Id and username of the seeded user.</returns>
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
    /// Builds the request instance path of the add track route for the owned library.
    /// </summary>
    /// <param name="artistId">The Id of the artist.</param>
    /// <param name="albumId">The Id of the album.</param>
    /// <returns>The request instance path.</returns>
    private string BuildInstancePath(Guid artistId, Guid albumId)
    {
        return $"/api/v1/libraries/{_libraryId}/artists/{artistId}/albums/{albumId}/tracks";
    }

    /// <summary>
    /// Asserts that the response is an unprocessable entity problem details carrying the expected validation error codes.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCodes">The validation error codes that must all be present.</param>
    private async Task AssertUnprocessableEntityWithValidationErrors(HttpResponseMessage response, string expectedInstance, params string[] expectedErrorCodes)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.All(expectedErrorCodes, code => Assert.Contains(code, errors["General.Validation"]));
    }

    /// <summary>
    /// Asserts the shape of a problem details response with the provided status, title, detail, instance and type.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="statusCode">The expected HTTP status code.</param>
    /// <param name="expectedTitle">The expected problem title.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedType">The expected problem type URI.</param>
    private async Task AssertProblemDetails(HttpResponseMessage response, HttpStatusCode statusCode, string expectedTitle, string expectedDetail, string expectedInstance, string expectedType)
    {
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)statusCode, problemDetails!["status"].GetInt32());
        Assert.Equal(expectedType, problemDetails["type"].GetString());
        Assert.Equal(expectedTitle, problemDetails["title"].GetString());
        Assert.Equal(expectedDetail, problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
