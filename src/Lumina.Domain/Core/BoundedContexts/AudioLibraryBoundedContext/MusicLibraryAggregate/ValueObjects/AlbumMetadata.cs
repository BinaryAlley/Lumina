#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the metadata of an album.
/// </summary>
[DebuggerDisplay("{Title}")]
public class AlbumMetadata : BaseMetadata
{
    /// <summary>
    /// Gets the type of the release, if applicable.
    /// </summary>
    public Optional<MusicReleaseType> ReleaseType { get; }

    /// <summary>
    /// Gets the status of the release, if applicable.
    /// </summary>
    public Optional<MusicReleaseStatus> ReleaseStatus { get; }

    /// <summary>
    /// Gets the number of discs of the release, if applicable.
    /// </summary>
    public Optional<int> TotalDiscs { get; }

    /// <summary>
    /// Gets the number of tracks of the release.
    /// </summary>
    public int TotalTracks { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the album.</param>
    /// <param name="originalTitle">The optional original title of the album, if applicable.</param>
    /// <param name="description">The description of the album, if applicable.</param>
    /// <param name="releaseInfo">The release information of the album.</param>
    /// <param name="genres">The genres of the album.</param>
    /// <param name="tags">The tags associated with the album.</param>
    /// <param name="language">The language of the album, if applicable.</param>
    /// <param name="originalLanguage">The optional original language of the album, if applicable.</param>
    /// <param name="releaseType">The optional type of the release.</param>
    /// <param name="releaseStatus">The optional status of the release.</param>
    /// <param name="totalDiscs">The optional number of discs of the release.</param>
    /// <param name="totalTracks">The number of tracks of the release.</param>
    private AlbumMetadata(
        string title,
        Optional<string> originalTitle,
        Optional<string> description,
        ReleaseInfo releaseInfo,
        List<Genre> genres,
        List<Tag> tags,
        Optional<LanguageInfo> language,
        Optional<LanguageInfo> originalLanguage,
        Optional<MusicReleaseType> releaseType,
        Optional<MusicReleaseStatus> releaseStatus,
        Optional<int> totalDiscs,
        int totalTracks)
        : base(title, originalTitle, description, releaseInfo, genres, tags, language, originalLanguage)
    {
        ReleaseType = releaseType;
        ReleaseStatus = releaseStatus;
        TotalDiscs = totalDiscs;
        TotalTracks = totalTracks;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AlbumMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the album.</param>
    /// <param name="originalTitle">The optional original title of the album, if applicable.</param>
    /// <param name="description">The description of the album, if applicable.</param>
    /// <param name="releaseInfo">The release information of the album.</param>
    /// <param name="genres">The genres of the album.</param>
    /// <param name="tags">The tags associated with the album.</param>
    /// <param name="language">The language of the album, if applicable.</param>
    /// <param name="originalLanguage">The optional original language of the album, if applicable.</param>
    /// <param name="releaseType">The optional type of the release.</param>
    /// <param name="releaseStatus">The optional status of the release.</param>
    /// <param name="totalDiscs">The optional number of discs of the release.</param>
    /// <param name="totalTracks">The number of tracks of the release.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="AlbumMetadata"/>, or an error message.
    /// </returns>
    public static Result<AlbumMetadata> Create(
        string title,
        Optional<string> originalTitle,
        Optional<string> description,
        ReleaseInfo releaseInfo,
        List<Genre> genres,
        List<Tag> tags,
        Optional<LanguageInfo> language,
        Optional<LanguageInfo> originalLanguage,
        Optional<MusicReleaseType> releaseType,
        Optional<MusicReleaseStatus> releaseStatus,
        Optional<int> totalDiscs,
        int totalTracks)
    {
        return new AlbumMetadata(
            title,
            originalTitle,
            description,
            releaseInfo,
            genres,
            tags,
            language,
            originalLanguage,
            releaseType,
            releaseStatus,
            totalDiscs,
            totalTracks);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        foreach (object component in base.GetEqualityComponents())
            yield return component;
        yield return ReleaseType;
        yield return ReleaseStatus;
        yield return TotalDiscs;
        yield return TotalTracks;
    }
}
