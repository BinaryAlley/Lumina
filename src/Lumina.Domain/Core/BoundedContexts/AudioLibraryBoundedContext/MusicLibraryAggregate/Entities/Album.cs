#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;

/// <summary>
/// Entity for an album, a release of an artist.
/// </summary>
[DebuggerDisplay("{Id}: {Metadata.Title}")]
public sealed class Album : Entity<AlbumId>
{
    private readonly List<MediaContributorCredit> _credits;
    private readonly List<AudioRating> _ratings;
    private readonly List<Track> _tracks;

    /// <summary>
    /// Gets the album metadata of the album.
    /// </summary>
    public AlbumMetadata Metadata { get; private set; }

    /// <summary>
    /// Gets the physical or digital medium of the album, if applicable.
    /// </summary>
    public Optional<MusicMediaFormat> MediaFormat { get; private set; }

    /// <summary>
    /// Gets the barcode of the album, if applicable.
    /// </summary>
    public Optional<Barcode> Barcode { get; private set; }

    /// <summary>
    /// Gets the catalog number of the album, if applicable.
    /// </summary>
    public Optional<string> CatalogNumber { get; private set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the release, if applicable.
    /// </summary>
    public Optional<MusicBrainzId> MusicBrainzReleaseId { get; private set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the release group, if applicable.
    /// </summary>
    public Optional<MusicBrainzId> MusicBrainzReleaseGroupId { get; private set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the release artist, if applicable.
    /// </summary>
    public Optional<MusicBrainzId> MusicBrainzReleaseArtistId { get; private set; }

    /// <summary>
    /// Gets the list of the credits of the media contributors of the album.
    /// </summary>
    public IReadOnlyCollection<MediaContributorCredit> Credits => _credits.AsReadOnly();

    /// <summary>
    /// Gets the list of ratings for this album.
    /// </summary>
    public IReadOnlyCollection<AudioRating> Ratings => _ratings.AsReadOnly();

    /// <summary>
    /// Gets the list of tracks of the album.
    /// </summary>
    public IReadOnlyCollection<Track> Tracks => _tracks.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Album"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the album.</param>
    /// <param name="metadata">The album metadata of the album.</param>
    /// <param name="mediaFormat">The optional physical or digital medium of the album.</param>
    /// <param name="barcode">The optional barcode of the album.</param>
    /// <param name="catalogNumber">The optional catalog number of the album.</param>
    /// <param name="musicBrainzReleaseId">The optional MusicBrainz identifier of the release.</param>
    /// <param name="musicBrainzReleaseGroupId">The optional MusicBrainz identifier of the release group.</param>
    /// <param name="musicBrainzReleaseArtistId">The optional MusicBrainz identifier of the release artist.</param>
    /// <param name="credits">The list of the credits of the media contributors of the album.</param>
    /// <param name="ratings">The list of ratings for the album.</param>
    /// <param name="tracks">The list of tracks of the album.</param>
    private Album(
        AlbumId id,
        AlbumMetadata metadata,
        Optional<MusicMediaFormat> mediaFormat,
        Optional<Barcode> barcode,
        Optional<string> catalogNumber,
        Optional<MusicBrainzId> musicBrainzReleaseId,
        Optional<MusicBrainzId> musicBrainzReleaseGroupId,
        Optional<MusicBrainzId> musicBrainzReleaseArtistId,
        List<MediaContributorCredit> credits,
        List<AudioRating> ratings,
        List<Track> tracks) : base(id)
    {
        Id = id;
        Metadata = metadata;
        MediaFormat = mediaFormat;
        Barcode = barcode;
        CatalogNumber = catalogNumber;
        MusicBrainzReleaseId = musicBrainzReleaseId;
        MusicBrainzReleaseGroupId = musicBrainzReleaseGroupId;
        MusicBrainzReleaseArtistId = musicBrainzReleaseArtistId;
        _credits = credits;
        _ratings = ratings;
        _tracks = tracks;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Album"/> class.
    /// </summary>
    /// <param name="metadata">The album metadata of the album.</param>
    /// <param name="mediaFormat">The optional physical or digital medium of the album.</param>
    /// <param name="barcode">The optional barcode of the album.</param>
    /// <param name="catalogNumber">The optional catalog number of the album.</param>
    /// <param name="musicBrainzReleaseId">The optional MusicBrainz identifier of the release.</param>
    /// <param name="musicBrainzReleaseGroupId">The optional MusicBrainz identifier of the release group.</param>
    /// <param name="musicBrainzReleaseArtistId">The optional MusicBrainz identifier of the release artist.</param>
    /// <param name="credits">The list of the credits of the media contributors of the album.</param>
    /// <param name="ratings">The list of ratings for the album.</param>
    /// <param name="tracks">The list of tracks of the album.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Album"/>, or an error message.
    /// </returns>
    public static Result<Album> Create(
        AlbumMetadata metadata,
        Optional<MusicMediaFormat> mediaFormat,
        Optional<Barcode> barcode,
        Optional<string> catalogNumber,
        Optional<MusicBrainzId> musicBrainzReleaseId,
        Optional<MusicBrainzId> musicBrainzReleaseGroupId,
        Optional<MusicBrainzId> musicBrainzReleaseArtistId,
        List<MediaContributorCredit> credits,
        List<AudioRating> ratings,
        List<Track> tracks)
    {
        return new Album(
            AlbumId.CreateUnique(),
            metadata,
            mediaFormat,
            barcode,
            catalogNumber,
            musicBrainzReleaseId,
            musicBrainzReleaseGroupId,
            musicBrainzReleaseArtistId,
            credits,
            ratings,
            tracks);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Album"/> class, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the album.</param>
    /// <param name="metadata">The album metadata of the album.</param>
    /// <param name="mediaFormat">The optional physical or digital medium of the album.</param>
    /// <param name="barcode">The optional barcode of the album.</param>
    /// <param name="catalogNumber">The optional catalog number of the album.</param>
    /// <param name="musicBrainzReleaseId">The optional MusicBrainz identifier of the release.</param>
    /// <param name="musicBrainzReleaseGroupId">The optional MusicBrainz identifier of the release group.</param>
    /// <param name="musicBrainzReleaseArtistId">The optional MusicBrainz identifier of the release artist.</param>
    /// <param name="credits">The list of the credits of the media contributors of the album.</param>
    /// <param name="ratings">The list of ratings for the album.</param>
    /// <param name="tracks">The list of tracks of the album.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Album"/>, or an error message.
    /// </returns>
    public static Result<Album> Create(
        AlbumId id,
        AlbumMetadata metadata,
        Optional<MusicMediaFormat> mediaFormat,
        Optional<Barcode> barcode,
        Optional<string> catalogNumber,
        Optional<MusicBrainzId> musicBrainzReleaseId,
        Optional<MusicBrainzId> musicBrainzReleaseGroupId,
        Optional<MusicBrainzId> musicBrainzReleaseArtistId,
        List<MediaContributorCredit> credits,
        List<AudioRating> ratings,
        List<Track> tracks)
    {
        return new Album(
            id,
            metadata,
            mediaFormat,
            barcode,
            catalogNumber,
            musicBrainzReleaseId,
            musicBrainzReleaseGroupId,
            musicBrainzReleaseArtistId,
            credits,
            ratings,
            tracks);
    }

    /// <summary>
    /// Adds a track to the album.
    /// </summary>
    /// <param name="track">The track to be added.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Created> AddTrack(Track track)
    {
        if (_tracks.Contains(track))
            return Errors.Music.TheTrackIsAlreadyInTheAlbum;
        _tracks.Add(track);
        return Result.Created;
    }

    /// <summary>
    /// Removes a track from the album.
    /// </summary>
    /// <param name="track">The track to be removed.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Deleted> RemoveTrack(Track track)
    {
        if (!_tracks.Contains(track))
            return Errors.Music.TheTrackIsNotInTheAlbum;
        _tracks.Remove(track);
        return Result.Deleted;
    }

    /// <summary>
    /// Replaces the credits of the media contributors of the album with the provided <paramref name="credits"/>.
    /// </summary>
    /// <param name="credits">The credits of the media contributors of the album.</param>
    public void UpdateCredits(IReadOnlyCollection<MediaContributorCredit> credits)
    {
        // replace the contents of the collection in place, preserving the readonly reference invariants of the entity
        _credits.Clear();
        _credits.AddRange(credits);
    }
}
