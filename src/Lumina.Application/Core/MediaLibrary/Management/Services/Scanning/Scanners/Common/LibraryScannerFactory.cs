#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Scanners.WrittenContent;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Scanners.Common;

/// <summary>
/// Defines a factory for creating media library scanners.
/// </summary>
internal class LibraryScannerFactory : ILibraryScannerFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScannerFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public LibraryScannerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a media library scanner for the specified library type.
    /// </summary>
    /// <param name="libraryType">The type of the media library.</param>
    /// <returns>A media library scanner for the specified library type.</returns>
    public IMediaTypeScanner CreateLibraryScanner(LibraryType libraryType)
    {
        return libraryType switch
        {
            // implemented scanners
            LibraryType.Book => _serviceProvider.GetRequiredService<IBookLibraryTypeScanner>(),
            LibraryType.EBook => throw new NotImplementedException("EBook scanner is not implemented."),
            LibraryType.ComicBook => throw new NotImplementedException("ComicBook scanner is not implemented."),
            LibraryType.Magazine => throw new NotImplementedException("Magazine scanner is not implemented."),
            LibraryType.Newspaper => throw new NotImplementedException("Newspaper scanner is not implemented."),
            LibraryType.Manga => throw new NotImplementedException("Manga scanner is not implemented."),
            LibraryType.GraphicNovel => throw new NotImplementedException("GraphicNovel scanner is not implemented."),
            LibraryType.AcademicPaper => throw new NotImplementedException("AcademicPaper scanner is not implemented."),
            LibraryType.SheetMusic => throw new NotImplementedException("SheetMusic scanner is not implemented."),
            LibraryType.Movie => throw new NotImplementedException("Movie scanner is not implemented."),
            LibraryType.TvShow => throw new NotImplementedException("TvShow scanner is not implemented."),
            LibraryType.Documentary => throw new NotImplementedException("Documentary scanner is not implemented."),
            LibraryType.Anime => throw new NotImplementedException("Anime scanner is not implemented."),
            LibraryType.ConcertVideo => throw new NotImplementedException("ConcertVideo scanner is not implemented."),
            LibraryType.TutorialVideo => throw new NotImplementedException("TutorialVideo scanner is not implemented."),
            LibraryType.HomeVideo => throw new NotImplementedException("HomeVideo scanner is not implemented."),
            LibraryType.YouTubeVideo => throw new NotImplementedException("YouTubeVideo scanner is not implemented."),
            LibraryType.MusicVideo => throw new NotImplementedException("MusicVideo scanner is not implemented."),
            LibraryType.LiveRecordingVideo => throw new NotImplementedException("LiveRecordingVideo scanner is not implemented."),
            LibraryType.InterviewVideo => throw new NotImplementedException("InterviewVideo scanner is not implemented."),
            LibraryType.CoverSongVideo => throw new NotImplementedException("CoverSongVideo scanner is not implemented."),
            LibraryType.PodcastVideo => throw new NotImplementedException("PodcastVideo scanner is not implemented."),
            LibraryType.Music => throw new NotImplementedException("Music scanner is not implemented."),
            LibraryType.Audiobook => throw new NotImplementedException("Audiobook scanner is not implemented."),
            LibraryType.LiveRecordingAudio => throw new NotImplementedException("LiveRecordingAudio scanner is not implemented."),
            LibraryType.InterviewAudio => throw new NotImplementedException("InterviewAudio scanner is not implemented."),
            LibraryType.CoverSongAudio => throw new NotImplementedException("CoverSongAudio scanner is not implemented."),
            LibraryType.Remix => throw new NotImplementedException("Remix scanner is not implemented."),
            LibraryType.SoundEffect => throw new NotImplementedException("SoundEffect scanner is not implemented."),
            LibraryType.PodcastAudio => throw new NotImplementedException("PodcastAudio scanner is not implemented."),
            LibraryType.Photo => throw new NotImplementedException("Photo scanner is not implemented."),
            LibraryType.Playlist => throw new NotImplementedException("Playlist scanner is not implemented."),
            LibraryType.Collection => throw new NotImplementedException("Collection scanner is not implemented."),
            LibraryType.Subtitles => throw new NotImplementedException("Subtitles scanner is not implemented."),
            LibraryType.Lyrics => throw new NotImplementedException("Lyrics scanner is not implemented."),
            // catch-all for unknown or unsupported types
            _ => throw new ArgumentOutOfRangeException(nameof(libraryType), $"No scanner exists for library type '{libraryType}'.")
        };
    }
}
