#region ========================================================================= USING =====================================================================================
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
public class MediaLibraryScannerFactoryTests
{
    [Fact]
    public void CreateLibraryScanner_WhenLibraryTypeIsBook_ShouldReturnBookLibraryTypeScanner()
    {
        // Arrange
        IBookLibraryTypeScanner expectedScanner = Substitute.For<IBookLibraryTypeScanner>();
        ServiceCollection services = new();
        services.AddSingleton(expectedScanner);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        MediaLibraryScannerFactory sut = new(serviceProvider);

        // Act
        IMediaTypeScanner scanner = sut.CreateLibraryScanner(LibraryType.Book);

        // Assert
        Assert.Same(expectedScanner, scanner);
    }

    [Theory]
    [InlineData(LibraryType.EBook)] // eBook scanner is not implemented
    [InlineData(LibraryType.ComicBook)] // comic book scanner is not implemented
    [InlineData(LibraryType.Magazine)] // magazine scanner is not implemented
    [InlineData(LibraryType.Newspaper)] // newspaper scanner is not implemented
    [InlineData(LibraryType.Manga)] // manga scanner is not implemented
    [InlineData(LibraryType.GraphicNovel)] // graphic novel scanner is not implemented
    [InlineData(LibraryType.AcademicPaper)] // academic paper scanner is not implemented
    [InlineData(LibraryType.SheetMusic)] // sheet music scanner is not implemented
    [InlineData(LibraryType.Movie)] // movie scanner is not implemented
    [InlineData(LibraryType.TvShow)] // TV show scanner is not implemented
    [InlineData(LibraryType.Documentary)] // documentary scanner is not implemented
    [InlineData(LibraryType.Anime)] // anime scanner is not implemented
    [InlineData(LibraryType.ConcertVideo)] // concert video scanner is not implemented
    [InlineData(LibraryType.TutorialVideo)] // tutorial video scanner is not implemented
    [InlineData(LibraryType.HomeVideo)] // home video scanner is not implemented
    [InlineData(LibraryType.YouTubeVideo)] // YouTube video scanner is not implemented
    [InlineData(LibraryType.MusicVideo)] // music video scanner is not implemented
    [InlineData(LibraryType.LiveRecordingVideo)] // live recording video scanner is not implemented
    [InlineData(LibraryType.InterviewVideo)] // interview video scanner is not implemented
    [InlineData(LibraryType.CoverSongVideo)] // cover song video scanner is not implemented
    [InlineData(LibraryType.PodcastVideo)] // podcast video scanner is not implemented
    [InlineData(LibraryType.Music)] // music scanner is not implemented
    [InlineData(LibraryType.Audiobook)] // audiobook scanner is not implemented
    [InlineData(LibraryType.LiveRecordingAudio)] // live recording audio scanner is not implemented
    [InlineData(LibraryType.InterviewAudio)] // interview audio scanner is not implemented
    [InlineData(LibraryType.CoverSongAudio)] // cover song audio scanner is not implemented
    [InlineData(LibraryType.Remix)] // remix scanner is not implemented
    [InlineData(LibraryType.SoundEffect)] // sound effect scanner is not implemented
    [InlineData(LibraryType.PodcastAudio)] // podcast audio scanner is not implemented
    [InlineData(LibraryType.Photo)] // photo scanner is not implemented
    [InlineData(LibraryType.Playlist)] // playlist scanner is not implemented
    [InlineData(LibraryType.Collection)] // collection scanner is not implemented
    [InlineData(LibraryType.Subtitles)] // subtitles scanner is not implemented
    [InlineData(LibraryType.Lyrics)] // lyrics scanner is not implemented
    public void CreateLibraryScanner_WhenLibraryTypeIsNotImplemented_ShouldThrowNotImplementedException(LibraryType libraryType)
    {
        // Arrange
        ServiceCollection services = new();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        MediaLibraryScannerFactory sut = new(serviceProvider);

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => sut.CreateLibraryScanner(libraryType));
    }

    [Fact]
    public void CreateLibraryScanner_WhenLibraryTypeIsUnknown_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        ServiceCollection services = new();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        MediaLibraryScannerFactory sut = new(serviceProvider);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.CreateLibraryScanner((LibraryType)999));
    }
}
