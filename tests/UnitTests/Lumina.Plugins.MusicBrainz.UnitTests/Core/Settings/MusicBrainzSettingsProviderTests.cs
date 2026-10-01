#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.MusicBrainz.Common.Models.DTO.Settings;
using Lumina.Plugins.MusicBrainz.Core.Settings;
using Lumina.Plugins.MusicBrainz.Fixtures.Common.Models.DTO.Settings;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Core.Settings;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzSettingsProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzSettingsProviderTests
{
    private readonly MusicBrainzSettingsDtoFixture _musicBrainzSettingsDtoFixture = new();

    [Fact]
    public async Task GetAsync_WhenNoStoreIsAvailable_ShouldReturnTheDefaults()
    {
        // Arrange
        MusicBrainzSettingsDto defaults = CreateDefaults();
        MusicBrainzSettingsProvider sut = new(null, Guid.NewGuid(), defaults);

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.NotSame(defaults, result);
        Assert.Equal("https://musicbrainz.org/ws/2/", result.BaseUrl);
        Assert.False(result.DoesAllowPrivateBaseUrl);
        Assert.Equal("Lumina-MusicBrainz/1.0", result.UserAgent);
        Assert.Equal("default@example.com", result.ContactEmail);
        Assert.Equal(10, result.SearchResultLimit);
        Assert.Equal(25, result.ReleaseLookupLimit);
        Assert.Equal(TimeSpan.FromSeconds(1), result.MinimumRequestInterval);
    }

    [Fact]
    public async Task GetAsync_WhenTheStoreReturnsNoSettings_ShouldReturnTheDefaults()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((IReadOnlyDictionary<string, string>?)null);
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal("https://musicbrainz.org/ws/2/", result.BaseUrl);
        Assert.Equal(10, result.SearchResultLimit);
    }

    [Fact]
    public async Task GetAsync_WhenEverySettingIsPersisted_ShouldOverlayThemOverTheDefaults()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string>
            {
                [MusicBrainzSettingsKeys.BASE_URL] = "https://mirror.example/ws/2/",
                [MusicBrainzSettingsKeys.DOES_ALLOW_PRIVATE_BASE_URL] = "true",
                [MusicBrainzSettingsKeys.CONTACT_EMAIL] = "  contact@example.com  ",
                [MusicBrainzSettingsKeys.SEARCH_RESULT_LIMIT] = "42",
                [MusicBrainzSettingsKeys.RELEASE_LOOKUP_LIMIT] = "30",
                [MusicBrainzSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS] = "2.5"
            });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal("https://mirror.example/ws/2/", result.BaseUrl);
        Assert.True(result.DoesAllowPrivateBaseUrl);
        Assert.Equal("contact@example.com", result.ContactEmail);
        Assert.Equal(42, result.SearchResultLimit);
        Assert.Equal(30, result.ReleaseLookupLimit);
        Assert.Equal(TimeSpan.FromSeconds(2.5), result.MinimumRequestInterval);
    }

    [Theory]
    [InlineData("not-a-url")] // a value that is not an URI at all
    [InlineData("ftp://mirror.example/")] // an absolute URI with an unsupported scheme
    [InlineData("relative/path")] // a relative URI
    [InlineData("   ")] // only white space
    public async Task GetAsync_WhenThePersistedBaseUrlIsInvalid_ShouldKeepTheDefaultBaseUrl(string baseUrl)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [MusicBrainzSettingsKeys.BASE_URL] = baseUrl });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal("https://musicbrainz.org/ws/2/", result.BaseUrl);
    }

    [Theory]
    [InlineData("http://127.0.0.1/ws/2/")] // IPv4 loopback
    [InlineData("http://localhost/ws/2/")] // loopback host name
    [InlineData("http://[::1]/ws/2/")] // IPv6 loopback
    [InlineData("http://169.254.169.254/latest/meta-data/")] // link local cloud metadata endpoint
    [InlineData("http://10.0.0.5/ws/2/")] // private class A range
    [InlineData("http://172.16.0.1/ws/2/")] // private class B range
    [InlineData("http://192.168.1.10/ws/2/")] // private class C range
    [InlineData("http://0.0.0.0/ws/2/")] // unspecified address
    [InlineData("http://[fc00::1]/ws/2/")] // IPv6 unique local range
    public async Task GetAsync_WhenThePersistedBaseUrlPointsAtALocalOrPrivateHost_ShouldKeepTheDefaultBaseUrl(string baseUrl)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [MusicBrainzSettingsKeys.BASE_URL] = baseUrl });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal("https://musicbrainz.org/ws/2/", result.BaseUrl);
    }

    [Theory]
    [InlineData("http://192.168.1.10/ws/2/")] // private class C range, a typical self hosted mirror
    [InlineData("http://localhost/ws/2/")] // loopback host name
    [InlineData("http://[fc00::1]/ws/2/")] // IPv6 unique local range
    public async Task GetAsync_WhenPrivateBaseUrlsAreExplicitlyAllowed_ShouldApplyThem(string baseUrl)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string>
            {
                [MusicBrainzSettingsKeys.DOES_ALLOW_PRIVATE_BASE_URL] = "true",
                [MusicBrainzSettingsKeys.BASE_URL] = baseUrl
            });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(baseUrl, result.BaseUrl);
    }

    [Theory]
    [InlineData("user\r\n@example.com")] // carriage return and line feed injection
    [InlineData("user@example.com\r\nX-Injected: value")] // header injection attempt
    [InlineData("user(comment)@example.com")] // parentheses break the user agent comment
    [InlineData("user\"quoted\"@example.com")] // quotes break the header value
    [InlineData("user example@example.com")] // an inner space is not a valid token character
    public async Task GetAsync_WhenThePersistedContactEmailContainsUnsafeCharacters_ShouldDropIt(string storedEmail)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [MusicBrainzSettingsKeys.CONTACT_EMAIL] = storedEmail });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Null(result.ContactEmail);
    }

    [Fact]
    public async Task GetAsync_WhenThePersistedBaseUrlIsAnAbsoluteHttpUrl_ShouldApplyIt()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [MusicBrainzSettingsKeys.BASE_URL] = "http://mirror.example/ws/2/" });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal("http://mirror.example/ws/2/", result.BaseUrl);
    }

    [Fact]
    public async Task GetAsync_WhenThePersistedBaseUrlIsNull_ShouldKeepTheDefaultBaseUrl()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [MusicBrainzSettingsKeys.BASE_URL] = null! });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal("https://musicbrainz.org/ws/2/", result.BaseUrl);
    }

    [Theory]
    [InlineData("", null)] // an empty value clears the email
    [InlineData("   ", null)] // a white space value clears the email
    [InlineData("  user@example.com  ", "user@example.com")] // a value with surrounding white space is trimmed
    public async Task GetAsync_WhenThePersistedContactEmailIsPresent_ShouldSetOrClearIt(string storedEmail, string? expectedEmail)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [MusicBrainzSettingsKeys.CONTACT_EMAIL] = storedEmail });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedEmail, result.ContactEmail);
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("0", 1)] // a limit of zero is clamped to one
    [InlineData("-5", 1)] // a negative limit is clamped to one
    [InlineData("not-a-number", 10)] // an unparsable value keeps the default
    public async Task GetAsync_WhenThePersistedSearchResultLimitIsPresent_ShouldParseAndClampIt(string storedLimit, int expectedLimit)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [MusicBrainzSettingsKeys.SEARCH_RESULT_LIMIT] = storedLimit });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedLimit, result.SearchResultLimit);
    }

    [Theory]
    [InlineData("30", 30)]
    [InlineData("0", 1)] // a limit of zero is clamped to one
    [InlineData("-5", 1)] // a negative limit is clamped to one
    [InlineData("not-a-number", 25)] // an unparsable value keeps the default
    public async Task GetAsync_WhenThePersistedReleaseLookupLimitIsPresent_ShouldParseAndClampIt(string storedLimit, int expectedLimit)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [MusicBrainzSettingsKeys.RELEASE_LOOKUP_LIMIT] = storedLimit });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedLimit, result.ReleaseLookupLimit);
    }

    [Theory]
    [InlineData("2.5", 2.5)]
    [InlineData("0", 0)] // an interval of zero is allowed
    [InlineData("-3", 0)] // a negative interval is clamped to zero
    [InlineData("not-a-number", 1)] // an unparsable value keeps the default
    public async Task GetAsync_WhenThePersistedMinimumRequestIntervalIsPresent_ShouldParseAndClampIt(string storedInterval, double expectedSeconds)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [MusicBrainzSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS] = storedInterval });
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), result.MinimumRequestInterval);
    }

    [Fact]
    public async Task GetAsync_WhenCalledTwice_ShouldReadTheStoreOnlyOnce()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<string, string>());
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto first = await sut.GetAsync(CancellationToken.None);
        MusicBrainzSettingsDto second = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Same(first, second);
        await mockStore.Received(1).GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_WhenCalledConcurrently_ShouldReadTheStoreOnlyOnce()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<string, string>());
        MusicBrainzSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        MusicBrainzSettingsDto[] results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => sut.GetAsync(CancellationToken.None)));

        // Assert
        Assert.All(results, result => Assert.Same(results[0], result));
        await mockStore.Received(1).GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_WhenTheCancellationTokenIsAlreadyCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using (CancellationTokenSource cancellationTokenSource = new())
        {
            cancellationTokenSource.Cancel();
            MusicBrainzSettingsProvider sut = new(null, Guid.NewGuid(), CreateDefaults());

            // Act
            Task Act()
            {
                return sut.GetAsync(cancellationTokenSource.Token);
            }

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
        }
    }

    /// <summary>
    /// Creates the default settings used by the tests of the provider.
    /// </summary>
    /// <returns>The default settings.</returns>
    private MusicBrainzSettingsDto CreateDefaults()
    {
        return _musicBrainzSettingsDtoFixture.Create(
            baseUrl: "https://musicbrainz.org/ws/2/",
            doesAllowPrivateBaseUrl: false,
            userAgent: "Lumina-MusicBrainz/1.0",
            contactEmail: "default@example.com",
            searchResultLimit: 10,
            releaseLookupLimit: 25,
            minimumRequestInterval: TimeSpan.FromSeconds(1));
    }
}
