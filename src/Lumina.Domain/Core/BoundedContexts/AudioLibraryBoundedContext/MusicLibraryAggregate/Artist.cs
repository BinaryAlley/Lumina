#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;

/// <summary>
/// Aggregate root for an artist.
/// </summary>
/// <remarks>
/// An artist is the entity that produced a musical work, whatever or whoever that might be. An artist is a single person, like Freddie Mercury, a group of people, 
/// like Queen, or a collaboration between artists, like Queen and David Bowie. The media contributors that make up the artist, together with the roles they play, 
/// are tracked as credits, and the artist can also reference the single media contributor it corresponds to, so that an artist and the person behind it are never duplicated.
/// </remarks>
[DebuggerDisplay("Id: {Id} Name: {Name}")]
public sealed class Artist : AggregateRoot<ArtistId>
{
    private readonly List<MediaContributorCredit> _credits;
    private readonly List<Album> _albums;

    /// <summary>
    /// Gets the name of the artist.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the website of the artist, if applicable.
    /// </summary>
    public Optional<string> Website { get; private set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the artist, if applicable.
    /// </summary>
    public Optional<MusicBrainzId> MusicBrainzArtistId { get; private set; }

    /// <summary>
    /// Gets the Id of the media contributor the artist corresponds to, if applicable.
    /// </summary>
    public Optional<MediaContributorId> MediaContributorId { get; private set; }

    /// <summary>
    /// Gets the list of the credits of the media contributors that make up the artist.
    /// </summary>
    public IReadOnlyCollection<MediaContributorCredit> Credits => _credits.AsReadOnly();

    /// <summary>
    /// Gets the list of albums of the artist.
    /// </summary>
    public IReadOnlyCollection<Album> Albums => _albums.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Artist"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the artist.</param>
    /// <param name="name">The name of the artist.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <param name="mediaContributorId">The optional Id of the media contributor the artist corresponds to.</param>
    /// <param name="credits">The list of the credits of the media contributors that make up the artist.</param>
    /// <param name="albums">The list of albums of the artist.</param>
    /// <param name="createdOnUtc">The date and time when the entity was created.</param>
    /// <param name="updatedOnUtc">The date and time when the entity was last updated.</param>
    private Artist(
        ArtistId id,
        string name,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        Optional<MediaContributorId> mediaContributorId,
        List<MediaContributorCredit> credits,
        List<Album> albums,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc) : base(id)
    {
        Id = id;
        Name = name;
        Website = website;
        MusicBrainzArtistId = musicBrainzArtistId;
        MediaContributorId = mediaContributorId;
        _credits = credits;
        _albums = albums;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc.HasValue ? updatedOnUtc.Value : null;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Artist"/> class.
    /// </summary>
    /// <param name="name">The name of the artist.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <param name="mediaContributorId">The optional Id of the media contributor the artist corresponds to.</param>
    /// <param name="credits">The list of the credits of the media contributors that make up the artist.</param>
    /// <param name="albums">The list of albums of the artist.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> Create(
        string name,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        Optional<MediaContributorId> mediaContributorId,
        List<MediaContributorCredit> credits,
        List<Album> albums)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Music.ArtistNameCannotBeEmpty;

        return new Artist(
            ArtistId.CreateUnique(),
            name,
            website,
            musicBrainzArtistId,
            mediaContributorId,
            credits,
            albums,
            DateTime.UtcNow,
            Optional<DateTime>.None());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Artist"/> class, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the artist.</param>
    /// <param name="name">The name of the artist.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <param name="mediaContributorId">The optional Id of the media contributor the artist corresponds to.</param>
    /// <param name="credits">The list of the credits of the media contributors that make up the artist.</param>
    /// <param name="albums">The list of albums of the artist.</param>
    /// <param name="createdOnUtc">The date and time when the entity was created.</param>
    /// <param name="updatedOnUtc">The date and time when the entity was last updated.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> Create(
        ArtistId id,
        string name,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        Optional<MediaContributorId> mediaContributorId,
        List<MediaContributorCredit> credits,
        List<Album> albums,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Music.ArtistNameCannotBeEmpty;

        return new Artist(
            id,
            name,
            website,
            musicBrainzArtistId,
            mediaContributorId,
            credits,
            albums,
            createdOnUtc,
            updatedOnUtc);
    }

    /// <summary>
    /// Adds an album to the artist.
    /// </summary>
    /// <param name="album">The album to be added.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Created> AddAlbum(Album album)
    {
        if (_albums.Contains(album))
            return Errors.Music.TheArtistAlreadyHasTheAlbum;
        _albums.Add(album);
        return Result.Created;
    }

    /// <summary>
    /// Removes an album from the artist.
    /// </summary>
    /// <param name="album">The album to be removed.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Deleted> RemoveAlbum(Album album)
    {
        if (!_albums.Contains(album))
            return Errors.Music.TheArtistDoesNotHaveTheAlbum;
        _albums.Remove(album);
        return Result.Deleted;
    }

    /// <summary>
    /// Replaces the credits of the media contributors that make up the artist with the provided <paramref name="credits"/>.
    /// </summary>
    /// <param name="credits">The credits of the media contributors that make up the artist.</param>
    public void UpdateCredits(IReadOnlyCollection<MediaContributorCredit> credits)
    {
        // replace the contents of the collection in place, preserving the readonly reference invariants of the aggregate
        _credits.Clear();
        _credits.AddRange(credits);
    }
}
