#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the consumption settings of a media library.
/// </summary>
public sealed class LibraryConsumptionSettings : ValueObject
{
    /// <summary>
    /// Gets a value indicating whether the consumption progress of the media library items is tracked.
    /// </summary>
    public bool ShouldTrackConsumptionProgress { get; private set; }

    /// <summary>
    /// Gets the percentage of a media library item that must be consumed for its session to be considered completed. The value is absent when
    /// progress tracking is disabled, and for library types whose completion is only recorded explicitly.
    /// </summary>
    public Optional<int> CompletionThresholdPercentage { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryConsumptionSettings"/> class.
    /// </summary>
    /// <param name="shouldTrackConsumptionProgress">Whether the consumption progress of the media library items is tracked.</param>
    /// <param name="completionThresholdPercentage">The percentage of a media library item that must be consumed for its session to be considered completed.</param>
    private LibraryConsumptionSettings(bool shouldTrackConsumptionProgress, Optional<int> completionThresholdPercentage)
    {
        ShouldTrackConsumptionProgress = shouldTrackConsumptionProgress;
        // A completion threshold has no meaning when progress is not tracked, so it is never stored in that case.
        CompletionThresholdPercentage = shouldTrackConsumptionProgress ? completionThresholdPercentage : Optional<int>.None();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="LibraryConsumptionSettings"/> class.
    /// </summary>
    /// <param name="shouldTrackConsumptionProgress">Whether the consumption progress of the media library items is tracked.</param>
    /// <param name="completionThresholdPercentage">The percentage of a media library item that must be consumed for its session to be considered completed.</param>
    /// <returns>The created consumption settings.</returns>
    public static LibraryConsumptionSettings Create(bool shouldTrackConsumptionProgress, Optional<int> completionThresholdPercentage)
    {
        return new LibraryConsumptionSettings(shouldTrackConsumptionProgress, completionThresholdPercentage);
    }

    /// <summary>
    /// Creates the default consumption settings for <paramref name="libraryType"/>.
    /// </summary>
    /// <remarks>
    /// A library type without a default completion threshold is created with progress tracking off. If progress tracking is enabled explicitly for
    /// such a type without a threshold, completion is only recorded when it is reported explicitly.
    /// </remarks>
    /// <param name="libraryType">The type of the media library for which to create the default consumption settings.</param>
    /// <returns>The default consumption settings of the media library type.</returns>
    public static LibraryConsumptionSettings CreateDefault(LibraryType libraryType)
    {
        return libraryType switch
        {
            // Written content.
            LibraryType.Book or LibraryType.EBook or LibraryType.ComicBook or LibraryType.Magazine or LibraryType.Newspaper or LibraryType.Manga or
            LibraryType.GraphicNovel or LibraryType.AcademicPaper or LibraryType.SheetMusic
                => Create(shouldTrackConsumptionProgress: true, Optional<int>.Some(100)),
            // Video.
            LibraryType.TvShow or LibraryType.Movie or LibraryType.Documentary or LibraryType.Anime or LibraryType.ConcertVideo or LibraryType.TutorialVideo or
            LibraryType.HomeVideo or LibraryType.YouTubeVideo or LibraryType.MusicVideo or LibraryType.LiveRecordingVideo or LibraryType.InterviewVideo or
            LibraryType.CoverSongVideo
                => Create(shouldTrackConsumptionProgress: true, Optional<int>.Some(90)),
            // Audiobooks and podcasts.
            LibraryType.Audiobook or LibraryType.PodcastAudio or LibraryType.PodcastVideo
                => Create(shouldTrackConsumptionProgress: true, Optional<int>.Some(95)),
            // Music audio and photos.
            LibraryType.Music or LibraryType.Remix or LibraryType.SoundEffect or LibraryType.LiveRecordingAudio or LibraryType.InterviewAudio or
            LibraryType.CoverSongAudio or LibraryType.Photo
                => Create(shouldTrackConsumptionProgress: false, Optional<int>.None()),
            // Library types that are not media libraries, and any type added later, stay off until they are classified here.
            LibraryType.Playlist or LibraryType.Collection or LibraryType.Subtitles or LibraryType.Lyrics or LibraryType.Other
                => Create(shouldTrackConsumptionProgress: false, Optional<int>.None()),
            _ => Create(shouldTrackConsumptionProgress: false, Optional<int>.None())
        };
    }

    /// <summary>
    /// Creates the default consumption settings for <paramref name="mediaKind"/>.
    /// </summary>
    /// <remarks>
    /// A media kind without a default completion threshold is created with progress tracking off. If progress tracking is enabled explicitly for
    /// such a kind without a threshold, completion is only recorded when it is reported explicitly.
    /// </remarks>
    /// <param name="mediaKind">The kind of media for which to create the default consumption settings.</param>
    /// <returns>The default consumption settings of the media kind.</returns>
    public static LibraryConsumptionSettings CreateDefault(MediaKind mediaKind)
    {
        return mediaKind switch
        {
            MediaKind.Book => Create(shouldTrackConsumptionProgress: true, Optional<int>.Some(100)),
            MediaKind.Movie or MediaKind.TvEpisode or MediaKind.Video => Create(shouldTrackConsumptionProgress: true, Optional<int>.Some(90)),
            MediaKind.PodcastEpisode or MediaKind.Audiobook => Create(shouldTrackConsumptionProgress: true, Optional<int>.Some(95)),
            // Kinds that do not track progress, and any kind added later, stay off until they are classified here.
            MediaKind.None or MediaKind.MusicTrack or MediaKind.Photo => Create(shouldTrackConsumptionProgress: false, Optional<int>.None()),
            _ => Create(shouldTrackConsumptionProgress: false, Optional<int>.None())
        };
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ShouldTrackConsumptionProgress;
        yield return CompletionThresholdPercentage;
    }
}
