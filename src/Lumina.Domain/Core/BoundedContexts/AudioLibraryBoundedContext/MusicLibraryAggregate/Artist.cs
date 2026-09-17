#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
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
/// are tracked as contributors.
/// </remarks>
[DebuggerDisplay("Id: {Id} Name: {Name}")]
public sealed class Artist : AggregateRoot<ArtistId>
{
    private readonly HashSet<MusicMediaContributor> _contributors;
    private readonly List<Album> _albums;

    /// <summary>
    /// Gets the Id of the media library this artist belongs to.
    /// </summary>
    public LibraryId LibraryId { get; private set; }

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
    /// Gets the list of the media contributors that make up the artist.
    /// </summary>
    public IReadOnlyCollection<MusicMediaContributor> Contributors => _contributors.AsReadOnly();

    /// <summary>
    /// Gets the list of albums of the artist.
    /// </summary>
    public IReadOnlyCollection<Album> Albums => _albums.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Artist"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the artist.</param>
    /// <param name="libraryId">The Id of the media library this artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <param name="contributors">The list of the media contributors that make up the artist.</param>
    /// <param name="albums">The list of albums of the artist.</param>
    /// <param name="createdOnUtc">The date and time when the entity was created.</param>
    /// <param name="updatedOnUtc">The date and time when the entity was last updated.</param>
    private Artist(
        ArtistId id,
        LibraryId libraryId,
        string name,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        List<MusicMediaContributor> contributors,
        List<Album> albums,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc) : base(id)
    {
        Id = id;
        LibraryId = libraryId;
        Name = name;
        Website = website;
        MusicBrainzArtistId = musicBrainzArtistId;
        _contributors = [.. contributors];
        _albums = albums;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc.HasValue ? updatedOnUtc.Value : null;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Artist"/> class.
    /// </summary>
    /// <param name="libraryId">The Id of the media library this artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <param name="contributors">The list of the media contributors that make up the artist.</param>
    /// <param name="albums">The list of albums of the artist.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> Create(
        LibraryId libraryId,
        string name,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        List<MusicMediaContributor> contributors,
        List<Album> albums)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Music.ArtistNameCannotBeEmpty;
        if (albums.Count is 0)
            return Errors.Music.ArtistMustHaveAtLeastOneAlbum;

        return new Artist(
            ArtistId.CreateUnique(),
            libraryId,
            name,
            website,
            musicBrainzArtistId,
            contributors,
            albums,
            DateTime.UtcNow, // TODO: should be IDateTimeProvider
            Optional<DateTime>.None());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Artist"/> class, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the artist.</param>
    /// <param name="libraryId">The Id of the media library this artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <param name="contributors">The list of the media contributors that make up the artist.</param>
    /// <param name="albums">The list of albums of the artist.</param>
    /// <param name="createdOnUtc">The date and time when the entity was created.</param>
    /// <param name="updatedOnUtc">The date and time when the entity was last updated.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> Create(
        ArtistId id,
        LibraryId libraryId,
        string name,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        List<MusicMediaContributor> contributors,
        List<Album> albums,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Music.ArtistNameCannotBeEmpty;
        if (albums.Count is 0)
            return Errors.Music.ArtistMustHaveAtLeastOneAlbum;

        return new Artist(
            id,
            libraryId,
            name,
            website,
            musicBrainzArtistId,
            contributors,
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
        if (_albums.Count is 1)
            return Errors.Music.ArtistMustHaveAtLeastOneAlbum; 
        _albums.Remove(album);
        return Result.Deleted;
    }

    /// <summary>
    /// Replaces the media contributors that make up the artist with the provided <paramref name="contributors"/>.
    /// </summary>
    /// <param name="contributors">The media contributors that make up the artist.</param>
    public void UpdateContributors(IReadOnlyCollection<MusicMediaContributor> contributors)
    {
        // replace the contents of the collection in place, preserving the readonly reference invariants of the aggregate
        _contributors.Clear();
        _contributors.UnionWith(contributors);
    }

    /// <summary>
    /// Updates the details of the artist, without touching the albums and the media contributors.
    /// </summary>
    /// <param name="name">The name of the artist.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Updated> UpdateDetails(
        string name,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Music.ArtistNameCannotBeEmpty;

        Name = name;
        Website = website;
        MusicBrainzArtistId = musicBrainzArtistId;
        UpdatedOnUtc = DateTime.UtcNow;
        return Result.Updated;
    }
}
