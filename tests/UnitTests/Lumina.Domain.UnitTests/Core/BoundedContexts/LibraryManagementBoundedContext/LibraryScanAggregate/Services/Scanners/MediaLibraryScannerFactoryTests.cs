#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.Scanners;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Scanners;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScannerFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScannerFactoryTests : IDisposable
{
    private readonly IBookLibraryTypeScanner _mockBookLibraryTypeScanner;
    private readonly IMusicLibraryTypeScanner _mockMusicLibraryTypeScanner;
    private readonly ServiceProvider _serviceProvider;
    private readonly MediaLibraryScannerFactory _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScannerFactoryTests"/> class.
    /// </summary>
    public MediaLibraryScannerFactoryTests()
    {
        _mockBookLibraryTypeScanner = Substitute.For<IBookLibraryTypeScanner>();
        _mockMusicLibraryTypeScanner = Substitute.For<IMusicLibraryTypeScanner>();

        ServiceCollection services = new();
        services.AddSingleton(_mockBookLibraryTypeScanner);
        services.AddSingleton(_mockMusicLibraryTypeScanner);
        _serviceProvider = services.BuildServiceProvider();
        _sut = new MediaLibraryScannerFactory(_serviceProvider);
    }

    [Fact]
    public void CreateLibraryScanner_WhenLibraryTypeIsBook_ShouldReturnBookLibraryTypeScanner()
    {
        // Act
        Result<IMediaTypeScanner> result = _sut.CreateLibraryScanner(LibraryType.Book);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(_mockBookLibraryTypeScanner, result.Value);
    }

    [Fact]
    public void CreateLibraryScanner_WhenLibraryTypeIsMusic_ShouldReturnMusicLibraryTypeScanner()
    {
        // Act
        Result<IMediaTypeScanner> result = _sut.CreateLibraryScanner(LibraryType.Music);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(_mockMusicLibraryTypeScanner, result.Value);
    }

    [Theory]
    [InlineData(LibraryType.EBook)] // EBook scanner is not implemented.
    [InlineData(LibraryType.ComicBook)] // Comic book scanner is not implemented.
    [InlineData(LibraryType.Magazine)] // Magazine scanner is not implemented.
    [InlineData(LibraryType.Newspaper)] // Newspaper scanner is not implemented.
    [InlineData(LibraryType.Manga)] // Manga scanner is not implemented.
    [InlineData(LibraryType.GraphicNovel)] // Graphic novel scanner is not implemented.
    [InlineData(LibraryType.AcademicPaper)] // Academic paper scanner is not implemented.
    [InlineData(LibraryType.SheetMusic)] // Sheet music scanner is not implemented.
    [InlineData(LibraryType.Movie)] // Movie scanner is not implemented.
    [InlineData(LibraryType.TvShow)] // TV show scanner is not implemented.
    [InlineData(LibraryType.Documentary)] // Documentary scanner is not implemented.
    [InlineData(LibraryType.Anime)] // Anime scanner is not implemented.
    [InlineData(LibraryType.ConcertVideo)] // Concert video scanner is not implemented.
    [InlineData(LibraryType.TutorialVideo)] // Tutorial video scanner is not implemented.
    [InlineData(LibraryType.HomeVideo)] // Home video scanner is not implemented.
    [InlineData(LibraryType.YouTubeVideo)] // YouTube video scanner is not implemented.
    [InlineData(LibraryType.MusicVideo)] // Music video scanner is not implemented.
    [InlineData(LibraryType.LiveRecordingVideo)] // Live recording video scanner is not implemented.
    [InlineData(LibraryType.InterviewVideo)] // Interview video scanner is not implemented.
    [InlineData(LibraryType.CoverSongVideo)] // Cover song video scanner is not implemented.
    [InlineData(LibraryType.PodcastVideo)] // Podcast video scanner is not implemented.
    [InlineData(LibraryType.Audiobook)] // Audiobook scanner is not implemented.
    [InlineData(LibraryType.LiveRecordingAudio)] // Live recording audio scanner is not implemented.
    [InlineData(LibraryType.InterviewAudio)] // Interview audio scanner is not implemented.
    [InlineData(LibraryType.CoverSongAudio)] // Cover song audio scanner is not implemented.
    [InlineData(LibraryType.Remix)] // Remix scanner is not implemented.
    [InlineData(LibraryType.SoundEffect)] // Sound effect scanner is not implemented.
    [InlineData(LibraryType.PodcastAudio)] // Podcast audio scanner is not implemented.
    [InlineData(LibraryType.Photo)] // Photo scanner is not implemented.
    [InlineData(LibraryType.Playlist)] // Playlist scanner is not implemented.
    [InlineData(LibraryType.Collection)] // Collection scanner is not implemented.
    [InlineData(LibraryType.Subtitles)] // Subtitles scanner is not implemented.
    [InlineData(LibraryType.Lyrics)] // Lyrics scanner is not implemented.
    public void CreateLibraryScanner_WhenLibraryTypeIsNotImplemented_ShouldReturnNoScannerError(LibraryType libraryType)
    {
        // Act
        Result<IMediaTypeScanner> result = _sut.CreateLibraryScanner(libraryType);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.NoScannerForLibraryType, result.FirstError);
        Assert.IsType<NotImplementedError>(result.FirstError);
    }

    [Fact]
    public void CreateLibraryScanner_WhenLibraryTypeIsNotImplemented_ShouldNotThrow()
    {
        // Act
        Exception? exception = Record.Exception(() => _sut.CreateLibraryScanner(LibraryType.Movie));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void CreateLibraryScanner_WhenLibraryTypeIsUnknown_ShouldReturnNoScannerError()
    {
        // Act
        Result<IMediaTypeScanner> result = _sut.CreateLibraryScanner((LibraryType)999);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.NoScannerForLibraryType, result.FirstError);
        Assert.IsType<NotImplementedError>(result.FirstError);
    }

    /// <summary>
    /// Disposes the test dependencies.
    /// </summary>
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
}
