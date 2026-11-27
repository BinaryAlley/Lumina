#region ========================================================================= USING =====================================================================================
using Nemera.Plugins.MusicBrainz.Core;
using Nemera.Plugins.MusicBrainz.Core.Api;
using Nemera.Plugins.MusicBrainz.Core.Settings;
using Nemera.Plugins.MusicBrainz.Fixtures.Common.Models.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Plugins.MusicBrainz.SecurityTests.Core.Api;

/// <summary>
/// Contains security tests for the <see cref="MusicBrainzHttpClient"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzHttpClientSecurityTests
{
    private readonly MusicBrainzSettingsDtoFixture _musicBrainzSettingsDtoFixture = new();

    [Theory]
    [InlineData("A & B")] // Ampersand query separator.
    [InlineData("Title #fragment")] // Fragment separator.
    [InlineData("A?B")] // Query separator.
    [InlineData("../../secret")] // Path traversal.
    [InlineData("A\" OR \"B")] // Quote escape.
    [InlineData("A\0B")] // Null byte truncation.
    public async Task SearchArtistsAsync_WhenTheQueryContainsInjectionCharacters_ShouldNotAlterTheRequestTarget(string maliciousQuery)
    {
        // Arrange
        CapturingHttpMessageHandler handler = new("""{"artists":[]}""");
        MusicBrainzHttpClient sut = CreateClient(handler);

        // Act
        await sut.SearchArtistsAsync(maliciousQuery, 10, CancellationToken.None);

        // Assert
        Uri requestUri = Assert.Single(handler.Requests).RequestUri!;
        Assert.Equal("musicbrainz.example", requestUri.Host);
        Assert.Equal("/ws/2/artist", requestUri.AbsolutePath);
        Assert.Equal(string.Empty, requestUri.Fragment);
    }

    [Theory]
    [InlineData("../release/secret")] // Path traversal.
    [InlineData("A&B")] // Query separator.
    [InlineData("A#B")] // Fragment separator.
    public async Task GetRecordingsByIsrcAsync_WhenTheIsrcContainsInjectionCharacters_ShouldNotAlterTheRequestTarget(string maliciousIsrc)
    {
        // Arrange
        CapturingHttpMessageHandler handler = new("""{"recordings":[]}""");
        MusicBrainzHttpClient sut = CreateClient(handler);

        // Act
        await sut.GetRecordingsByIsrcAsync(maliciousIsrc, CancellationToken.None);

        // Assert
        Uri requestUri = Assert.Single(handler.Requests).RequestUri!;
        Assert.Equal("musicbrainz.example", requestUri.Host);
        Assert.StartsWith("/ws/2/isrc/", requestUri.AbsolutePath);
        Assert.Equal(string.Empty, requestUri.Fragment);
    }

    /// <summary>
    /// Creates a <see cref="MusicBrainzHttpClient"/> that points at a test host and sends its requests through the provided handler.
    /// </summary>
    /// <param name="handler">The handler that answers the requests of the client.</param>
    /// <returns>The created client.</returns>
    private MusicBrainzHttpClient CreateClient(HttpMessageHandler handler)
    {
        MusicBrainzSettingsProvider settingsProvider = new(null, MusicBrainzPlugin.s_pluginId, _musicBrainzSettingsDtoFixture.Create(
            baseUrl: "http://musicbrainz.example/ws/2/",
            userAgent: "Nemera-Test/1.0",
            includeContactEmail: false,
            searchResultLimit: 10,
            releaseLookupLimit: 25,
            minimumRequestInterval: TimeSpan.Zero));
        return new MusicBrainzHttpClient(new HttpClient(handler), settingsProvider, new MusicBrainzRequestThrottle(), new MusicBrainzResponseCache());
    }

    /// <summary>
    /// HTTP message handler that answers every request with a canned JSON body and records the requests that were sent.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _json;

        /// <summary>
        /// Gets the requests that were sent through the handler.
        /// </summary>
        public List<HttpRequestMessage> Requests { get; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturingHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="json">The JSON body answered to every request.</param>
        public CapturingHttpMessageHandler(string json)
        {
            _json = json;
        }

        /// <summary>
        /// Answers the provided request with the canned JSON body.
        /// </summary>
        /// <param name="request">The request to answer.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
        /// <returns>The response to the request.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            });
        }
    }
}
