#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetMusicTrack;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetMusicTrack;

/// <summary>
/// Contains security tests for the <see cref="GetMusicTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicTrackEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly MusicTrackDtoFixture _musicTrackDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetMusicTrackEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetMusicTrack_WhenCalledWithoutAuthentication_ShouldRedirectToLoginWithoutCallingApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/audio-library/music-library/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/api-get-track?libraryId={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("libraries/", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task GetMusicTrack_WithSQLInjectionInRouteId_ShouldNormalizeTheIdWithoutLeakingData(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        MusicTrackDto expectedTrack = _musicTrackDtoFixture.Create();
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}/tracks/{normalizedId}", expectedTrack);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/artists/{Uri.EscapeDataString(maliciousId)}/albums/{Uri.EscapeDataString(maliciousId)}/tracks/{Uri.EscapeDataString(maliciousId)}/api-get-track?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The route values are normalized to Guids before they are substituted into the upstream URL, so a crafted route value can never escape
        // its URL segment or reach a different track lookup; the Web layer must not fail local binding, leak stack traces or database details.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (JsonDocument json = JsonDocument.Parse(content))
            Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousId, content, StringComparison.Ordinal);
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == $"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}/tracks/{normalizedId}");
    }
}
