#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.CoverArtArchive.Common.Models.DTO.Settings;
using Lumina.Plugins.CoverArtArchive.Core.Settings;
using Lumina.Plugins.CoverArtArchive.Fixtures.Common.Models.DTO.Settings;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.CoverArtArchive.UnitTests.Core.Settings;

/// <summary>
/// Contains unit tests for the <see cref="CoverArtArchiveSettingsProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveSettingsProviderTests
{
    private readonly CoverArtArchiveSettingsDtoFixture _coverArtArchiveSettingsDtoFixture = new();

    [Fact]
    public async Task GetAsync_WhenNoStoreIsAvailable_ShouldReturnTheDefaults()
    {
        // Arrange
        CoverArtArchiveSettingsDto defaults = CreateDefaults();
        CoverArtArchiveSettingsProvider sut = new(null, Guid.NewGuid(), defaults);

        // Act
        CoverArtArchiveSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.NotSame(defaults, result);
        Assert.Equal("Lumina-CoverArtArchive/1.0", result.UserAgent);
        Assert.Equal("default@example.com", result.ContactEmail);
        Assert.Equal(TimeSpan.FromSeconds(1), result.MinimumRequestInterval);
    }

    [Fact]
    public async Task GetAsync_WhenTheStoreReturnsNoSettings_ShouldReturnTheDefaults()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((IReadOnlyDictionary<string, string>?)null);
        CoverArtArchiveSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        CoverArtArchiveSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal("Lumina-CoverArtArchive/1.0", result.UserAgent);
        Assert.Equal("default@example.com", result.ContactEmail);
        Assert.Equal(TimeSpan.FromSeconds(1), result.MinimumRequestInterval);
    }

    [Fact]
    public async Task GetAsync_WhenEverySettingIsPersisted_ShouldOverlayThemOverTheDefaults()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string>
            {
                [CoverArtArchiveSettingsKeys.CONTACT_EMAIL] = "  contact@example.com  ",
                [CoverArtArchiveSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS] = "2.5"
            });
        CoverArtArchiveSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        CoverArtArchiveSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal("contact@example.com", result.ContactEmail);
        Assert.Equal(TimeSpan.FromSeconds(2.5), result.MinimumRequestInterval);
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
            new Dictionary<string, string> { [CoverArtArchiveSettingsKeys.CONTACT_EMAIL] = storedEmail });
        CoverArtArchiveSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        CoverArtArchiveSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Null(result.ContactEmail);
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
            new Dictionary<string, string> { [CoverArtArchiveSettingsKeys.CONTACT_EMAIL] = storedEmail });
        CoverArtArchiveSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        CoverArtArchiveSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedEmail, result.ContactEmail);
    }

    [Theory]
    [InlineData("2.5", 2.5)]
    [InlineData("0", 0)] // an interval of zero is allowed
    [InlineData("-3", 0)] // a negative interval is clamped to zero
    [InlineData("not-a-number", 1)] // an unparsable value keeps the default
    [InlineData("NaN", 1)] // a non finite value keeps the default
    [InlineData("Infinity", 1)] // a non finite value keeps the default
    [InlineData("-Infinity", 1)] // a non finite value keeps the default
    [InlineData("1e400", 1)] // a value that overflows the double range keeps the default
    [InlineData("1e308", 60)] // a huge but finite value is clamped to the maximum
    [InlineData("1000", 60)] // a value above the maximum is clamped
    [InlineData("60", 60)] // the maximum itself is kept
    [InlineData("59", 59)] // a value within the range is kept
    public async Task GetAsync_WhenThePersistedMinimumRequestIntervalIsPresent_ShouldParseAndClampIt(string storedInterval, double expectedSeconds)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [CoverArtArchiveSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS] = storedInterval });
        CoverArtArchiveSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        CoverArtArchiveSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), result.MinimumRequestInterval);
    }

    [Fact]
    public async Task GetAsync_WhenCalledTwice_ShouldReadTheStoreOnlyOnce()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<string, string>());
        CoverArtArchiveSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        CoverArtArchiveSettingsDto first = await sut.GetAsync(CancellationToken.None);
        CoverArtArchiveSettingsDto second = await sut.GetAsync(CancellationToken.None);

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
        CoverArtArchiveSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults());

        // Act
        CoverArtArchiveSettingsDto[] results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => sut.GetAsync(CancellationToken.None)));

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
            CoverArtArchiveSettingsProvider sut = new(null, Guid.NewGuid(), CreateDefaults());

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
    private CoverArtArchiveSettingsDto CreateDefaults()
    {
        return _coverArtArchiveSettingsDtoFixture.Create(
            userAgent: "Lumina-CoverArtArchive/1.0",
            contactEmail: "default@example.com",
            minimumRequestInterval: TimeSpan.FromSeconds(1));
    }
}
