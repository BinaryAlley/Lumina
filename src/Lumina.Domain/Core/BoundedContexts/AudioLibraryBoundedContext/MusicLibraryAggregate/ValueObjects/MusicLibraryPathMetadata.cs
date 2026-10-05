#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the metadata derived from the file system path of a music library item.
/// </summary>
public class MusicLibraryPathMetadata : ValueObject
{
    /// <summary>
    /// Gets the name of the artist derived from the path, if applicable.
    /// </summary>
    public Optional<string> ArtistName { get; }

    /// <summary>
    /// Gets the type of the release derived from the path, if applicable.
    /// </summary>
    public Optional<MusicReleaseType> ReleaseType { get; }

    /// <summary>
    /// Gets the release year derived from the path, if applicable.
    /// </summary>
    public Optional<int> ReleaseYear { get; }

    /// <summary>
    /// Gets the name of the release derived from the path, if applicable.
    /// </summary>
    public Optional<string> ReleaseName { get; }

    /// <summary>
    /// Gets the number of the track derived from the path, if applicable.
    /// </summary>
    public Optional<int> TrackNumber { get; }

    /// <summary>
    /// Gets the number of the disc the track belongs to, derived from the path, if applicable.
    /// </summary>
    public Optional<int> DiscNumber { get; }

    /// <summary>
    /// Gets the title of the track derived from the path, if applicable.
    /// </summary>
    public Optional<string> TrackTitle { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicLibraryPathMetadata"/> class.
    /// </summary>
    /// <param name="artistName">The name of the artist derived from the path, if applicable.</param>
    /// <param name="releaseType">The type of the release derived from the path, if applicable.</param>
    /// <param name="releaseYear">The release year derived from the path, if applicable.</param>
    /// <param name="releaseName">The name of the release derived from the path, if applicable.</param>
    /// <param name="trackNumber">The number of the track derived from the path, if applicable.</param>
    /// <param name="discNumber">The number of the disc derived from the path, if applicable.</param>
    /// <param name="trackTitle">The title of the track derived from the path, if applicable.</param>
    private MusicLibraryPathMetadata(
        Optional<string> artistName,
        Optional<MusicReleaseType> releaseType,
        Optional<int> releaseYear,
        Optional<string> releaseName,
        Optional<int> trackNumber,
        Optional<int> discNumber,
        Optional<string> trackTitle)
    {
        ArtistName = artistName;
        ReleaseType = releaseType;
        ReleaseYear = releaseYear;
        ReleaseName = releaseName;
        TrackNumber = trackNumber;
        DiscNumber = discNumber;
        TrackTitle = trackTitle;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicLibraryPathMetadata"/> class from the values captured from a path template.
    /// </summary>
    /// <param name="parsedPath">The values captured from the path of the music library item.</param>
    /// <returns>The metadata derived from the captured values.</returns>
    public static MusicLibraryPathMetadata FromParsedLibraryPath(ParsedLibraryPath parsedPath)
    {
        Optional<MusicReleaseType> releaseType = Optional<MusicReleaseType>.None();
        Optional<string> releaseTypeValue = parsedPath.GetString(LibraryPathPartKind.ReleaseType);
        if (releaseTypeValue.HasValue)
        {
            // An unrecognized release type folder does not prevent the rest of the path from being derived, it is simply reported as the catch-all type.
            releaseType = Enum.TryParse(releaseTypeValue.Value, ignoreCase: true, out MusicReleaseType parsedReleaseType)
                ? Optional<MusicReleaseType>.Some(parsedReleaseType)
                : Optional<MusicReleaseType>.Some(MusicReleaseType.Other);
        }
        return new MusicLibraryPathMetadata(
            parsedPath.GetString(LibraryPathPartKind.Artist),
            releaseType,
            parsedPath.GetInt(LibraryPathPartKind.ReleaseYear),
            parsedPath.GetString(LibraryPathPartKind.ReleaseName),
            parsedPath.GetInt(LibraryPathPartKind.TrackNumber),
            parsedPath.GetInt(LibraryPathPartKind.DiscNumber),
            parsedPath.GetString(LibraryPathPartKind.TrackName));
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ArtistName;
        yield return ReleaseType;
        yield return ReleaseYear;
        yield return ReleaseName;
        yield return TrackNumber;
        yield return DiscNumber;
        yield return TrackTitle;
    }
}
