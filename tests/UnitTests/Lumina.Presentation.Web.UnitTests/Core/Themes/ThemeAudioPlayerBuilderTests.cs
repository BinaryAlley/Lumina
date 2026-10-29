#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeAudioPlayerBuilder"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeAudioPlayerBuilderTests
{
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly ThemeAudioPlayerBuilder _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeAudioPlayerBuilderTests"/> class.
    /// </summary>
    public ThemeAudioPlayerBuilderTests()
    {
        _mockStringLocalizer = CreateLocalizer();
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _sut = new ThemeAudioPlayerBuilder(_mockStringLocalizerFactory, _mockHttpContextAccessor);
    }

    [Fact]
    public void Constructor_WhenCalled_ShouldRequestTheLayoutResourcesFromTheWebAssembly()
    {
        // Arrange
        _mockStringLocalizerFactory.ClearReceivedCalls();

        // Act
        _ = new ThemeAudioPlayerBuilder(_mockStringLocalizerFactory, _mockHttpContextAccessor);

        // Assert
        _mockStringLocalizerFactory.Received(1).Create("Lumina.Presentation.Web.Views.Shared._Layout", "Lumina.Presentation.Web");
    }

    [Theory]
    [InlineData("toggleExpanded", "ToggleExpanded")] // the expanded player toggle
    [InlineData("previous", "AudioPlayerPrevious")] // the previous track control
    [InlineData("next", "AudioPlayerNext")] // the next track control
    [InlineData("stop", "AudioPlayerStop")] // the stop control
    [InlineData("repeat", "AudioPlayerRepeat")] // the repeat control
    [InlineData("repeatOff", "AudioPlayerRepeatOff")] // repeat turned off
    [InlineData("repeatAll", "AudioPlayerRepeatAll")] // repeat of the whole queue
    [InlineData("repeatOne", "AudioPlayerRepeatOne")] // repeat of a single track
    [InlineData("queue", "AudioPlayerQueue")] // the play queue title
    [InlineData("albumArtwork", "AudioPlayerAlbumArtwork")] // the album artwork placeholder
    [InlineData("addQueueToPlaylist", "AudioPlayerAddQueueToPlaylist")] // adding the queue to a playlist
    [InlineData("addToPlaylist", "PlaylistAddToPlaylist")] // adding a single track to a playlist
    [InlineData("selectPlaylist", "AudioPlayerSelectPlaylist")] // the playlist selector
    [InlineData("noPlaylists", "AudioPlayerNoPlaylists")] // no saved playlist is available
    [InlineData("add", "AudioPlayerAdd")] // the add confirmation
    [InlineData("playlistQueueAdded", "AudioPlayerPlaylistQueueAdded")] // the queue was added to a playlist
    [InlineData("playlistQueueAddFailed", "AudioPlayerPlaylistQueueAddFailed")] // adding the queue to a playlist failed
    public void Build_WhenCalled_ShouldLocalizeThePlayerString(string modelKey, string resourceKey)
    {
        // Act
        ThemeAudioPlayerDto model = _sut.Build("/theme-assets/editorial/assets", string.Empty, string.Empty);

        // Assert
        Assert.True(model.Strings.TryGetValue(modelKey, out object? value), $"The '{modelKey}' string is missing.");
        Assert.Equal($"Localized-{resourceKey}", Assert.IsType<string>(value));
        _ = _mockStringLocalizer.Received(1)[resourceKey];
    }

    [Fact]
    public void Build_WhenCalled_ShouldPassThroughTheAssetBaseAndTheSubTemplates()
    {
        // Arrange
        string queueItemTemplate = "<li class=\"queue-item\">{{title}}</li>";
        string playlistItemTemplate = "<li class=\"playlist-item\">{{name}}</li>";

        // Act
        ThemeAudioPlayerDto model = _sut.Build("/theme-assets/editorial/assets", queueItemTemplate, playlistItemTemplate);

        // Assert
        Assert.Equal("/theme-assets/editorial/assets", model.AssetBase);
        Assert.Equal(queueItemTemplate, model.QueueItemTemplate);
        Assert.Equal(playlistItemTemplate, model.PlaylistItemTemplate);
    }

    [Fact]
    public void Build_WhenSubTemplatesAreEmpty_ShouldPreserveThemUnchanged()
    {
        // Act
        ThemeAudioPlayerDto model = _sut.Build("/theme-assets/editorial/assets", string.Empty, string.Empty);

        // Assert
        Assert.Equal(string.Empty, model.QueueItemTemplate);
        Assert.Equal(string.Empty, model.PlaylistItemTemplate);
    }

    [Fact]
    public void Build_WhenRequestHasAPathBase_ShouldResolveAppBaseWithATrailingSlash()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost", 5001);
        httpContext.Request.PathBase = new PathString("/lumina");
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        ThemeAudioPlayerDto model = _sut.Build("/theme-assets/editorial/assets", string.Empty, string.Empty);

        // Assert
        Assert.Equal("https://localhost:5001/lumina/", model.AppBase);
    }

    [Fact]
    public void Build_WhenRequestHasNoPathBase_ShouldResolveAppBaseWithATrailingSlash()
    {
        // Arrange
        DefaultHttpContext httpContext = TestHttpContextFactory.Create();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost", 5001);
        _mockHttpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        ThemeAudioPlayerDto model = _sut.Build("/theme-assets/editorial/assets", string.Empty, string.Empty);

        // Assert
        Assert.Equal("https://localhost:5001/", model.AppBase);
    }

    [Fact]
    public void Build_WhenRequestIsUnavailable_ShouldReturnAnEmptyAppBase()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        ThemeAudioPlayerDto model = _sut.Build("/theme-assets/editorial/assets", string.Empty, string.Empty);

        // Assert
        Assert.Equal(string.Empty, model.AppBase);
    }

    /// <summary>
    /// Creates a substitute string localizer that returns a string prefixed with "Localized-".
    /// </summary>
    /// <returns>The created substitute string localizer.</returns>
    private static IStringLocalizer CreateLocalizer()
    {
        IStringLocalizer localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), $"Localized-{callInfo.Arg<string>()}"));
        return localizer;
    }
}
