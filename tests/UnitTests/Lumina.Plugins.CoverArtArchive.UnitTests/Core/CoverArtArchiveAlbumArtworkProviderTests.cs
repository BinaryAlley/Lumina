#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.CoverArtArchive.Core;
using Lumina.Plugins.CoverArtArchive.Core.Api;
using Lumina.Plugins.CoverArtArchive.Core.Settings;
using Lumina.Plugins.CoverArtArchive.Fixtures.Common.Models.DTO.Settings;
using Lumina.Plugins.CoverArtArchive.UnitTests.Common.TestHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.CoverArtArchive.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="CoverArtArchiveAlbumArtworkProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveAlbumArtworkProviderTests
{
    private const string RELEASE_ARTWORK_JSON = """
    {
        "images": [
            { "image": "https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/front.jpg", "front": true, "types": [ "Front" ] },
            { "image": "https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/back.jpg", "back": true, "types": [ "Back" ] }
        ]
    }
    """;

    private const string RELEASE_GROUP_ARTWORK_JSON = """
    {
        "images": [
            { "image": "https://coverartarchive.org/release-group/22222222-2222-2222-2222-222222222222/front.jpg", "front": true, "types": [ "Front" ] }
        ]
    }
    """;

    private static readonly Guid s_releaseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid s_releaseGroupId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly AlbumMetadataLookupDtoFixture _albumMetadataLookupDtoFixture = new();
    private readonly CoverArtArchiveSettingsDtoFixture _coverArtArchiveSettingsDtoFixture = new();

    [Fact]
    public void Name_WhenCalled_ShouldReturnTheProviderDisplayName()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, _) = CreateProvider((request, callIndex) => CreateJsonResponse(RELEASE_ARTWORK_JSON));

        // Act
        string result = sut.Name;

        // Assert
        Assert.Equal("Cover Art Archive", result);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenCalled_ShouldReturnMusic()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, _) = CreateProvider((request, callIndex) => CreateJsonResponse(RELEASE_ARTWORK_JSON));

        // Act
        IReadOnlyList<LibraryType> result = sut.SupportedLibraryTypes;

        // Assert
        Assert.Equal([LibraryType.Music], result);
    }

    [Fact]
    public void RequiresWebAccess_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, _) = CreateProvider((request, callIndex) => CreateJsonResponse(RELEASE_ARTWORK_JSON));

        // Act
        bool result = sut.RequiresWebAccess;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheReleaseIdentifierIsPresent_ShouldFetchAndMapTheReleaseArtwork()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => CreateJsonResponse(RELEASE_ARTWORK_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(ArtworkType.Cover, result[0].Type);
        Assert.Equal(0, result[0].Ordinal);
        Assert.Null(result[0].LocalPath);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/front.jpg", result[0].RemoteUrl);
        Assert.Equal(ArtworkType.Back, result[1].Type);
        Assert.Equal(0, result[1].Ordinal);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/back.jpg", result[1].RemoteUrl);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal($"/release/{s_releaseId:D}", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenBothIdentifiersArePresent_ShouldPreferTheRelease()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => CreateJsonResponse(
            request.RequestUri!.AbsolutePath.Contains("/release-group/") ? RELEASE_GROUP_ARTWORK_JSON : RELEASE_ARTWORK_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, musicBrainzReleaseGroupId: s_releaseGroupId);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal($"/release/{s_releaseId:D}", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheReleaseHasNoArtwork_ShouldFallBackToTheReleaseGroup()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => CreateJsonResponse(
            request.RequestUri!.AbsolutePath.Contains("/release-group/") ? RELEASE_GROUP_ARTWORK_JSON : """{"images":[]}"""));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, musicBrainzReleaseGroupId: s_releaseGroupId);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Cover, artwork.Type);
        Assert.Equal("https://coverartarchive.org/release-group/22222222-2222-2222-2222-222222222222/front.jpg", artwork.RemoteUrl);
        Assert.Equal(2, handler.CallCount);
        Assert.Equal($"/release/{s_releaseId:D}", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Equal($"/release-group/{s_releaseGroupId:D}", handler.Requests[1].RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheReleaseIsNotFound_ShouldFallBackToTheReleaseGroup()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) =>
            request.RequestUri!.AbsolutePath.Contains("/release-group/")
                ? CreateJsonResponse(RELEASE_GROUP_ARTWORK_JSON)
                : new HttpResponseMessage(HttpStatusCode.NotFound));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId, musicBrainzReleaseGroupId: s_releaseGroupId);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheReleaseHasNoArtworkAndThereIsNoReleaseGroupIdentifier_ShouldReturnEmpty()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => CreateJsonResponse("""{"images":[]}"""));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheReleaseGroupIdentifierIsPresent_ShouldFetchTheReleaseGroupArtwork()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => CreateJsonResponse(RELEASE_GROUP_ARTWORK_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal($"/release-group/{s_releaseGroupId:D}", handler.Requests[0].RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheReleaseGroupIsNotFound_ShouldReturnEmpty()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => new HttpResponseMessage(HttpStatusCode.NotFound));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseGroupId: s_releaseGroupId);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenThereIsNoIdentifierAtAll_ShouldReturnEmptyWithoutAnyRequest()
    {
        // Arrange
        (CoverArtArchiveAlbumArtworkProvider sut, StubHttpMessageHandler handler) = CreateProvider((request, callIndex) => CreateJsonResponse(RELEASE_ARTWORK_JSON));
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path");

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheCancellationTokenIsAlreadyCancelled_ShouldCancelTheOperation()
    {
        // Arrange
        using (CancellationTokenSource cancellationTokenSource = new())
        {
            cancellationTokenSource.Cancel();
            (CoverArtArchiveAlbumArtworkProvider sut, _) = CreateProvider((request, callIndex) => CreateJsonResponse(RELEASE_ARTWORK_JSON));
            AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: "path", musicBrainzReleaseId: s_releaseId);

            // Act
            Task Act()
            {
                return sut.GetArtworkAsync(lookup, cancellationTokenSource.Token);
            }

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
        }
    }

    /// <summary>
    /// Creates a <see cref="CoverArtArchiveAlbumArtworkProvider"/> whose requests are answered by the provided stub handler.
    /// </summary>
    /// <param name="responseFactory">The factory that produces the response of a request.</param>
    /// <returns>The created provider and its stub handler.</returns>
    private (CoverArtArchiveAlbumArtworkProvider Provider, StubHttpMessageHandler Handler) CreateProvider(Func<HttpRequestMessage, int, HttpResponseMessage> responseFactory)
    {
        StubHttpMessageHandler handler = new(responseFactory);
        CoverArtArchiveSettingsProvider settingsProvider = new(null, CoverArtArchivePlugin.s_pluginId, _coverArtArchiveSettingsDtoFixture.Create(
            userAgent: "Lumina-Test/1.0",
            includeContactEmail: false,
            minimumRequestInterval: TimeSpan.Zero));
        CoverArtArchiveHttpClient httpClient = new(new HttpClient(handler), settingsProvider, new CoverArtArchiveRequestThrottle(), new CoverArtArchiveResponseCache());
        return (new CoverArtArchiveAlbumArtworkProvider(httpClient), handler);
    }

    /// <summary>
    /// Creates an OK response with the provided JSON body.
    /// </summary>
    /// <param name="json">The JSON body of the response.</param>
    /// <returns>The created response.</returns>
    private static HttpResponseMessage CreateJsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }
}
