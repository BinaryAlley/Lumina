#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.LocalMusicArtwork.Common.Models.DTO.Settings;
using Lumina.Plugins.LocalMusicArtwork.Core.Settings;
using Lumina.Plugins.LocalMusicArtwork.Fixtures.Common.Models.DTO.Settings;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.UnitTests.Core.Settings;

/// <summary>
/// Contains unit tests for the <see cref="LocalMusicArtworkSettingsProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LocalMusicArtworkSettingsProviderTests
{
    private readonly LocalMusicArtworkSettingsDtoFixture _localMusicArtworkSettingsDtoFixture = new();

    [Fact]
    public async Task GetAsync_WhenNoStoreIsAvailable_ShouldReturnTheDefaults()
    {
        // Arrange
        LocalMusicArtworkSettingsDto defaults = CreateDefaults(shouldExtractEmbeddedCover: true);
        LocalMusicArtworkSettingsProvider sut = new(null, Guid.NewGuid(), defaults);

        // Act
        LocalMusicArtworkSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.NotSame(defaults, result);
        Assert.True(result.ShouldExtractEmbeddedCover);
    }

    [Fact]
    public async Task GetAsync_WhenNoStoreIsAvailableAndTheDefaultsDisableExtraction_ShouldReturnTheDisabledDefault()
    {
        // Arrange
        LocalMusicArtworkSettingsDto defaults = CreateDefaults(shouldExtractEmbeddedCover: false);
        LocalMusicArtworkSettingsProvider sut = new(null, Guid.NewGuid(), defaults);

        // Act
        LocalMusicArtworkSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.False(result.ShouldExtractEmbeddedCover);
    }

    [Fact]
    public async Task GetAsync_WhenTheStoreReturnsNoSettings_ShouldReturnTheDefaults()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((IReadOnlyDictionary<string, string>?)null);
        LocalMusicArtworkSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults(shouldExtractEmbeddedCover: true));

        // Act
        LocalMusicArtworkSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.True(result.ShouldExtractEmbeddedCover);
    }

    [Theory]
    [InlineData("false", true, false)] // a stored value disables extraction over a default that enables it
    [InlineData("true", false, true)] // a stored value enables extraction over a default that disables it
    [InlineData("True", false, true)] // the stored value is parsed without regard to casing
    [InlineData("FALSE", true, false)] // the stored value is parsed without regard to casing
    public async Task GetAsync_WhenTheSettingIsPersisted_ShouldOverlayItOverTheDefaults(string storedValue, bool defaultValue, bool expectedValue)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [LocalMusicArtworkSettingsKeys.SHOULD_EXTRACT_EMBEDDED_COVER] = storedValue });
        LocalMusicArtworkSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults(shouldExtractEmbeddedCover: defaultValue));

        // Act
        LocalMusicArtworkSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedValue, result.ShouldExtractEmbeddedCover);
    }

    [Theory]
    [InlineData("not-a-boolean")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("yes")]
    public async Task GetAsync_WhenThePersistedSettingIsNotABoolean_ShouldKeepTheDefault(string storedValue)
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [LocalMusicArtworkSettingsKeys.SHOULD_EXTRACT_EMBEDDED_COVER] = storedValue });
        LocalMusicArtworkSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults(shouldExtractEmbeddedCover: true));

        // Act
        LocalMusicArtworkSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.True(result.ShouldExtractEmbeddedCover);
    }

    [Fact]
    public async Task GetAsync_WhenThePersistedSettingsDoNotContainTheKey_ShouldKeepTheDefault()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { ["SomeOtherKey"] = "false" });
        LocalMusicArtworkSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults(shouldExtractEmbeddedCover: true));

        // Act
        LocalMusicArtworkSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.True(result.ShouldExtractEmbeddedCover);
    }

    [Fact]
    public async Task GetAsync_WhenThePersistedKeyHasDifferentCasing_ShouldKeepTheDefault()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { ["shouldextractembeddedcover"] = "false" });
        LocalMusicArtworkSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults(shouldExtractEmbeddedCover: true));

        // Act
        LocalMusicArtworkSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.True(result.ShouldExtractEmbeddedCover);
    }

    [Fact]
    public async Task GetAsync_WhenThePersistedSettingOverridesTheDefaults_ShouldNotMutateTheDefaults()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Dictionary<string, string> { [LocalMusicArtworkSettingsKeys.SHOULD_EXTRACT_EMBEDDED_COVER] = "false" });
        LocalMusicArtworkSettingsDto defaults = CreateDefaults(shouldExtractEmbeddedCover: true);
        LocalMusicArtworkSettingsProvider sut = new(mockStore, Guid.NewGuid(), defaults);

        // Act
        LocalMusicArtworkSettingsDto result = await sut.GetAsync(CancellationToken.None);

        // Assert
        Assert.False(result.ShouldExtractEmbeddedCover);
        Assert.True(defaults.ShouldExtractEmbeddedCover);
    }

    [Fact]
    public async Task GetAsync_WhenCalledTwice_ShouldReadTheStoreOnlyOnce()
    {
        // Arrange
        IPluginSettingsStore mockStore = Substitute.For<IPluginSettingsStore>();
        mockStore.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<string, string>());
        LocalMusicArtworkSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults(shouldExtractEmbeddedCover: true));

        // Act
        LocalMusicArtworkSettingsDto first = await sut.GetAsync(CancellationToken.None);
        LocalMusicArtworkSettingsDto second = await sut.GetAsync(CancellationToken.None);

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
        LocalMusicArtworkSettingsProvider sut = new(mockStore, Guid.NewGuid(), CreateDefaults(shouldExtractEmbeddedCover: true));

        // Act
        LocalMusicArtworkSettingsDto[] results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => sut.GetAsync(CancellationToken.None)));

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
            LocalMusicArtworkSettingsProvider sut = new(null, Guid.NewGuid(), CreateDefaults(shouldExtractEmbeddedCover: true));

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
    /// <param name="shouldExtractEmbeddedCover">Whether the cover embedded in the audio files is extracted when the album has no cover image on disk.</param>
    /// <returns>The default settings.</returns>
    private LocalMusicArtworkSettingsDto CreateDefaults(bool shouldExtractEmbeddedCover)
    {
        return _localMusicArtworkSettingsDtoFixture.Create(shouldExtractEmbeddedCover: shouldExtractEmbeddedCover);
    }
}
