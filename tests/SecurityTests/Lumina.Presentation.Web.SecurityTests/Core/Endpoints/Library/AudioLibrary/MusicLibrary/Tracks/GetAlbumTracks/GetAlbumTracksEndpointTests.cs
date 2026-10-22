#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetAlbumTracks;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetAlbumTracks;

/// <summary>
/// Contains security tests for the <see cref="GetAlbumTracksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumTracksEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PaginatedDtoFixture<MusicTrackLiteDto> _paginatedMusicTrackLiteDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumTracksEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetAlbumTracksEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetAlbumTracks_WhenCalledWithoutAuthentication_ShouldRedirectToLoginWithoutCallingApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/audio-library/music-library/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/api-get-tracks?libraryId={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("libraries/", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task GetAlbumTracks_WithSQLInjectionInArtistId_ShouldNormalizeTheIdWithoutLeakingData(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{normalizedId}/albums/{albumId}/tracks/lite", _paginatedMusicTrackLiteDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/artists/{Uri.EscapeDataString(maliciousId)}/albums/{albumId}/tracks/api-get-tracks?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (JsonDocument json = JsonDocument.Parse(content))
            Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousId, content, StringComparison.Ordinal);
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == $"libraries/{libraryId}/artists/{normalizedId}/albums/{albumId}/tracks/lite");
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task GetAlbumTracks_WithSQLInjectionInAlbumId_ShouldNormalizeTheIdWithoutLeakingData(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{artistId}/albums/{normalizedId}/tracks/lite", _paginatedMusicTrackLiteDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{Uri.EscapeDataString(maliciousId)}/tracks/api-get-tracks?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (JsonDocument json = JsonDocument.Parse(content))
            Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousId, content, StringComparison.Ordinal);
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == $"libraries/{libraryId}/artists/{artistId}/albums/{normalizedId}/tracks/lite");
    }
}
