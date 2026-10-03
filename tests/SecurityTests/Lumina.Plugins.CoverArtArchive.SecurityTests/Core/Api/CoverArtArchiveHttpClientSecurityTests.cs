#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.CoverArtArchive.Core;
using Lumina.Plugins.CoverArtArchive.Core.Api;
using Lumina.Plugins.CoverArtArchive.Core.Settings;
using Lumina.Plugins.CoverArtArchive.Fixtures.Common.Models.DTO.Settings;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.CoverArtArchive.SecurityTests.Core.Api;

/// <summary>
/// Contains security tests for the <see cref="CoverArtArchiveHttpClient"/> class, exercised through the requests it sends.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveHttpClientSecurityTests
{
    private static readonly Guid s_releaseId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly CoverArtArchiveSettingsDtoFixture _coverArtArchiveSettingsDtoFixture = new();

    [Theory]
    [InlineData("https://evil.example/index.json")] // an unrelated public host
    [InlineData("https://localhost/index.json")] // the loopback host name
    [InlineData("https://127.0.0.1/index.json")] // the IPv4 loopback address
    [InlineData("https://169.254.169.254/latest/meta-data/")] // the link local cloud metadata endpoint
    [InlineData("https://10.0.0.5/index.json")] // a private class A address
    [InlineData("https://192.168.1.10/index.json")] // a private class C address
    [InlineData("https://coverartarchive.org.evil.example/index.json")] // the trusted host used as a prefix of an attacker controlled host
    public async Task GetReleaseArtworkAsync_WhenTheRedirectPointsAtADisallowedHost_ShouldThrowAndNotReachIt(string location)
    {
        // Arrange
        CapturingHttpMessageHandler handler = new((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri(location);
            return redirect;
        });
        CoverArtArchiveHttpClient sut = CreateClient(handler);

        // Act
        Task Act()
        {
            return sut.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
        // No request is ever sent to the disallowed host, the client only ever talks to the Cover Art Archive.
        Assert.All(handler.Requests, request => Assert.Equal("coverartarchive.org", request.RequestUri!.Host));
    }

    [Theory]
    [InlineData("http://archive.org/index.json")] // a downgrade of the Internet Archive to plain HTTP
    [InlineData("http://coverartarchive.org/index.json")] // a downgrade of the Cover Art Archive to plain HTTP
    public async Task GetReleaseArtworkAsync_WhenTheRedirectDowngradesToPlainHttp_ShouldThrow(string location)
    {
        // Arrange
        CapturingHttpMessageHandler handler = new((request, callIndex) =>
        {
            HttpResponseMessage redirect = new(HttpStatusCode.TemporaryRedirect);
            redirect.Headers.Location = new Uri(location);
            return redirect;
        });
        CoverArtArchiveHttpClient sut = CreateClient(handler);

        // Act
        Task Act()
        {
            return sut.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(Act);
    }

    [Theory]
    [InlineData("user@example.com\r\nX-Injected: value")] // carriage return and line feed header injection
    [InlineData("user@example.com\nX-Injected: value")] // line feed header injection
    public async Task GetReleaseArtworkAsync_WhenThePersistedContactEmailAttemptsHeaderInjection_ShouldNotInjectAnyHeader(string maliciousEmail)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [CoverArtArchiveSettingsKeys.CONTACT_EMAIL] = maliciousEmail });
        CoverArtArchiveSettingsProvider settingsProvider = new(mockStore, CoverArtArchivePlugin.s_pluginId, _coverArtArchiveSettingsDtoFixture.Create(
            userAgent: "Lumina-Security/1.0",
            includeContactEmail: false,
            minimumRequestInterval: TimeSpan.Zero));
        CapturingHttpMessageHandler handler = new((request, callIndex) => CreateJsonResponse("""{"images":[]}"""));
        CoverArtArchiveHttpClient sut = new(new HttpClient(handler), settingsProvider, new CoverArtArchiveRequestThrottle(), new CoverArtArchiveResponseCache());

        // Act
        await sut.GetReleaseArtworkAsync(s_releaseId, CancellationToken.None);

        // Assert
        HttpRequestMessage request = Assert.Single(handler.Requests);
        Assert.Equal("Lumina-Security/1.0", request.Headers.UserAgent.ToString());
        Assert.False(request.Headers.Contains("X-Injected"));
    }

    /// <summary>
    /// Creates a <see cref="CoverArtArchiveHttpClient"/> that sends its requests through the provided handler, with no throttling.
    /// </summary>
    /// <param name="handler">The handler that answers the requests of the client.</param>
    /// <returns>The created client.</returns>
    private CoverArtArchiveHttpClient CreateClient(HttpMessageHandler handler)
    {
        CoverArtArchiveSettingsProvider settingsProvider = new(null, CoverArtArchivePlugin.s_pluginId, _coverArtArchiveSettingsDtoFixture.Create(
            userAgent: "Lumina-Security/1.0",
            includeContactEmail: false,
            minimumRequestInterval: TimeSpan.Zero));
        return new CoverArtArchiveHttpClient(new HttpClient(handler), settingsProvider, new CoverArtArchiveRequestThrottle(), new CoverArtArchiveResponseCache());
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

    /// <summary>
    /// HTTP message handler that answers every request through a factory and records the requests that were sent.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, int, HttpResponseMessage> _responseFactory;
        private int _callCount;

        /// <summary>
        /// Gets the requests that were sent through the handler.
        /// </summary>
        public List<HttpRequestMessage> Requests { get; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturingHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="responseFactory">The factory that produces the response of a request, given the request and its one based call index.</param>
        public CapturingHttpMessageHandler(Func<HttpRequestMessage, int, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        /// <summary>
        /// Answers the provided request through the configured factory.
        /// </summary>
        /// <param name="request">The request to answer.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
        /// <returns>The response to the request.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int callIndex = Interlocked.Increment(ref _callCount);
            Requests.Add(request);
            return Task.FromResult(_responseFactory(request, callIndex));
        }
    }
}
