#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtists;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtists;

/// <summary>
/// Contains security tests for the <see cref="GetMusicArtistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicArtistsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PaginatedDtoFixture<MusicArtistLiteDto> _paginatedMusicArtistLiteDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicArtistsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetMusicArtistsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetMusicArtists_WhenCalledWithoutAuthentication_ShouldRedirectToLoginWithoutCallingApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/audio-library/music-library/artists/api-get-artists?libraryId={Guid.NewGuid()}&shouldIgnoreThePrefixForAlphaPicker=false");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("libraries/", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists--")] // destructive SQL injection
    [InlineData("' OR '1'='1")] // basic SQL injection
    public async Task GetMusicArtists_WhenCalledWithInjectionInSearchTerm_ShouldRemainSecure(string maliciousSearchTerm)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string expectedEndpoint = $"libraries/{libraryId}/artists/lite?searchTerm={Uri.EscapeDataString(maliciousSearchTerm)}&shouldIgnoreThePrefixForAlphaPicker=False";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _paginatedMusicArtistLiteDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/audio-library/music-library/artists/api-get-artists?libraryId={libraryId}&searchTerm={Uri.EscapeDataString(maliciousSearchTerm)}&shouldIgnoreThePrefixForAlphaPicker=false");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousSearchTerm, content, StringComparison.Ordinal);
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists--")] // destructive SQL injection
    [InlineData("' OR '1'='1")] // basic SQL injection
    public async Task GetMusicArtists_WhenCalledWithInjectionInFilterAlphaKey_ShouldRemainSecure(string maliciousAlphaKey)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string expectedEndpoint = $"libraries/{libraryId}/artists/lite?filterAlphaKey={Uri.EscapeDataString(maliciousAlphaKey)}&shouldIgnoreThePrefixForAlphaPicker=False";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _paginatedMusicArtistLiteDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/audio-library/music-library/artists/api-get-artists?libraryId={libraryId}&filterAlphaKey={Uri.EscapeDataString(maliciousAlphaKey)}&shouldIgnoreThePrefixForAlphaPicker=false");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousAlphaKey, content, StringComparison.Ordinal);
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists--")] // destructive SQL injection
    [InlineData("' OR '1'='1")] // basic SQL injection
    public async Task GetMusicArtists_WhenCalledWithInjectionInSortBy_ShouldRemainSecure(string maliciousSortBy)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string expectedEndpoint = $"libraries/{libraryId}/artists/lite?shouldIgnoreThePrefixForAlphaPicker=False&sortBy={Uri.EscapeDataString(maliciousSortBy)}";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _paginatedMusicArtistLiteDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/audio-library/music-library/artists/api-get-artists?libraryId={libraryId}&sortBy={Uri.EscapeDataString(maliciousSortBy)}&shouldIgnoreThePrefixForAlphaPicker=false");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousSortBy, content, StringComparison.Ordinal);
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
