#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;

/// <summary>
/// Aggregate root for an artist.
/// </summary>
/// <remarks>
/// An artist is the entity that produced a musical work, whatever or whoever that might be. An artist is a single person, like Freddie Mercury, a group of people,
/// like Queen, or a collaboration between artists, like Queen and David Bowie. The media contributors that make up the artist, together with the roles they play,
/// are tracked as contributors.
/// The albums of the artist, and the tracks of those albums, are entities inside this aggregate, not aggregate roots, so they are referenced by object, as DDD prescribes
/// for entities within the same consistency boundary. The methods that mutate them therefore take the album, respectively the track, itself, rather than its id.
/// </remarks>
[DebuggerDisplay("Id: {Id} Name: {Name}")]
public sealed class Artist : AggregateRoot<ArtistId>
{
    private readonly HashSet<MusicMediaContributor> _contributors;
    private readonly List<MusicArtistAlias> _aliases;
    private readonly List<string> _ipis;
    private readonly List<string> _isnis;
    private readonly List<Genre> _genres;
    private readonly List<Tag> _tags;
    private readonly List<AudioRating> _ratings;
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
    /// Gets the sort name of the artist, a variant of the name used when sorting artists, if applicable.
    /// </summary>
    public Optional<string> SortName { get; private set; }

    /// <summary>
    /// Gets the disambiguation comment of the artist, used to distinguish artists with the same name, if applicable.
    /// </summary>
    public Optional<string> Disambiguation { get; private set; }

    /// <summary>
    /// Gets the type of the artist, describing whether it is a person, a group, or something else, if applicable.
    /// </summary>
    public Optional<MusicArtistType> Type { get; private set; }

    /// <summary>
    /// Gets the gender of the artist, applicable only to artists that are persons or characters, if applicable.
    /// </summary>
    public Optional<MusicArtistGender> Gender { get; private set; }

    /// <summary>
    /// Gets the ISO 3166-1 alpha-2 code of the country the artist is associated with, if applicable.
    /// </summary>
    public Optional<string> Country { get; private set; }

    /// <summary>
    /// Gets the area the artist is primarily identified with, if applicable.
    /// </summary>
    public Optional<MusicArea> Area { get; private set; }

    /// <summary>
    /// Gets the area the artist began in, if applicable.
    /// </summary>
    public Optional<MusicArea> BeginArea { get; private set; }

    /// <summary>
    /// Gets the area the artist ended in, if applicable.
    /// </summary>
    public Optional<MusicArea> EndArea { get; private set; }

    /// <summary>
    /// Gets the date the artist started existing, if applicable.
    /// </summary>
    public Optional<DateOnly> LifeSpanBegin { get; private set; }

    /// <summary>
    /// Gets the date the artist stopped existing, if applicable.
    /// </summary>
    public Optional<DateOnly> LifeSpanEnd { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the artist no longer exists.
    /// </summary>
    public bool IsEnded { get; private set; }

    /// <summary>
    /// Gets the website of the artist, if applicable.
    /// </summary>
    public Optional<string> Website { get; private set; }

    /// <summary>
    /// Gets the MusicBrainz identifier of the artist, if applicable.
    /// </summary>
    public Optional<MusicBrainzId> MusicBrainzArtistId { get; private set; }

    /// <summary>
    /// Gets the list of IPI (Interested Party Information) codes of the artist.
    /// </summary>
    public IReadOnlyCollection<string> Ipis => _ipis.AsReadOnly();

    /// <summary>
    /// Gets the list of ISNI (International Standard Name Identifier) codes of the artist.
    /// </summary>
    public IReadOnlyCollection<string> Isnis => _isnis.AsReadOnly();

    /// <summary>
    /// Gets the list of alternative names of the artist.
    /// </summary>
    public IReadOnlyCollection<MusicArtistAlias> Aliases => _aliases.AsReadOnly();

    /// <summary>
    /// Gets the list of genres associated with the artist.
    /// </summary>
    public IReadOnlyCollection<Genre> Genres => _genres.AsReadOnly();

    /// <summary>
    /// Gets the list of tags associated with the artist.
    /// </summary>
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    /// <summary>
    /// Gets the list of ratings of the artist.
    /// </summary>
    public IReadOnlyCollection<AudioRating> Ratings => _ratings.AsReadOnly();

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
    /// <param name="sortName">The optional sort name of the artist.</param>
    /// <param name="disambiguation">The optional disambiguation comment of the artist.</param>
    /// <param name="type">The optional type of the artist.</param>
    /// <param name="gender">The optional gender of the artist.</param>
    /// <param name="country">The optional ISO 3166-1 alpha-2 code of the country of the artist.</param>
    /// <param name="area">The optional area the artist is primarily identified with.</param>
    /// <param name="beginArea">The optional area the artist began in.</param>
    /// <param name="endArea">The optional area the artist ended in.</param>
    /// <param name="lifeSpanBegin">The optional date the artist started existing.</param>
    /// <param name="lifeSpanEnd">The optional date the artist stopped existing.</param>
    /// <param name="isEnded">Whether the artist no longer exists.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <param name="ipis">The list of IPI codes of the artist.</param>
    /// <param name="isnis">The list of ISNI codes of the artist.</param>
    /// <param name="aliases">The list of alternative names of the artist.</param>
    /// <param name="genres">The list of genres associated with the artist.</param>
    /// <param name="tags">The list of tags associated with the artist.</param>
    /// <param name="ratings">The list of ratings of the artist.</param>
    /// <param name="contributors">The list of the media contributors that make up the artist.</param>
    /// <param name="albums">The list of albums of the artist.</param>
    /// <param name="createdOnUtc">The date and time when the entity was created.</param>
    /// <param name="updatedOnUtc">The date and time when the entity was last updated.</param>
    private Artist(
        ArtistId id,
        LibraryId libraryId,
        string name,
        Optional<string> sortName,
        Optional<string> disambiguation,
        Optional<MusicArtistType> type,
        Optional<MusicArtistGender> gender,
        Optional<string> country,
        Optional<MusicArea> area,
        Optional<MusicArea> beginArea,
        Optional<MusicArea> endArea,
        Optional<DateOnly> lifeSpanBegin,
        Optional<DateOnly> lifeSpanEnd,
        bool isEnded,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        List<string> ipis,
        List<string> isnis,
        List<MusicArtistAlias> aliases,
        List<Genre> genres,
        List<Tag> tags,
        List<AudioRating> ratings,
        List<MusicMediaContributor> contributors,
        List<Album> albums,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc) : base(id)
    {
        Id = id;
        LibraryId = libraryId;
        Name = name;
        SortName = sortName;
        Disambiguation = disambiguation;
        Type = type;
        Gender = gender;
        Country = country;
        Area = area;
        BeginArea = beginArea;
        EndArea = endArea;
        LifeSpanBegin = lifeSpanBegin;
        LifeSpanEnd = lifeSpanEnd;
        IsEnded = isEnded;
        Website = website;
        MusicBrainzArtistId = musicBrainzArtistId;
        _ipis = ipis;
        _isnis = isnis;
        _aliases = aliases;
        _genres = genres;
        _tags = tags;
        _ratings = ratings;
        _contributors = [.. contributors];
        _albums = albums;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Artist"/> class.
    /// </summary>
    /// <param name="libraryId">The Id of the media library this artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <param name="sortName">The optional sort name of the artist.</param>
    /// <param name="disambiguation">The optional disambiguation comment of the artist.</param>
    /// <param name="type">The optional type of the artist.</param>
    /// <param name="gender">The optional gender of the artist.</param>
    /// <param name="country">The optional ISO 3166-1 alpha-2 code of the country of the artist.</param>
    /// <param name="area">The optional area the artist is primarily identified with.</param>
    /// <param name="beginArea">The optional area the artist began in.</param>
    /// <param name="endArea">The optional area the artist ended in.</param>
    /// <param name="lifeSpanBegin">The optional date the artist started existing.</param>
    /// <param name="lifeSpanEnd">The optional date the artist stopped existing.</param>
    /// <param name="isEnded">Whether the artist no longer exists.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <param name="ipis">The list of IPI codes of the artist.</param>
    /// <param name="isnis">The list of ISNI codes of the artist.</param>
    /// <param name="aliases">The list of alternative names of the artist.</param>
    /// <param name="genres">The list of genres associated with the artist.</param>
    /// <param name="tags">The list of tags associated with the artist.</param>
    /// <param name="ratings">The list of ratings of the artist.</param>
    /// <param name="contributors">The list of the media contributors that make up the artist.</param>
    /// <param name="albums">The list of albums of the artist.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> Create(
        LibraryId libraryId,
        string name,
        Optional<string> sortName,
        Optional<string> disambiguation,
        Optional<MusicArtistType> type,
        Optional<MusicArtistGender> gender,
        Optional<string> country,
        Optional<MusicArea> area,
        Optional<MusicArea> beginArea,
        Optional<MusicArea> endArea,
        Optional<DateOnly> lifeSpanBegin,
        Optional<DateOnly> lifeSpanEnd,
        bool isEnded,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        List<string> ipis,
        List<string> isnis,
        List<MusicArtistAlias> aliases,
        List<Genre> genres,
        List<Tag> tags,
        List<AudioRating> ratings,
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
            sortName,
            disambiguation,
            type,
            gender,
            country,
            area,
            beginArea,
            endArea,
            lifeSpanBegin,
            lifeSpanEnd,
            isEnded,
            website,
            musicBrainzArtistId,
            ipis,
            isnis,
            aliases,
            genres,
            tags,
            ratings,
            contributors,
            albums,
            DateTime.UtcNow, // TODO: should be IDateTimeProvider.
            Optional<DateTime>.None());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Artist"/> class, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the artist.</param>
    /// <param name="libraryId">The Id of the media library this artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <param name="sortName">The optional sort name of the artist.</param>
    /// <param name="disambiguation">The optional disambiguation comment of the artist.</param>
    /// <param name="type">The optional type of the artist.</param>
    /// <param name="gender">The optional gender of the artist.</param>
    /// <param name="country">The optional ISO 3166-1 alpha-2 code of the country of the artist.</param>
    /// <param name="area">The optional area the artist is primarily identified with.</param>
    /// <param name="beginArea">The optional area the artist began in.</param>
    /// <param name="endArea">The optional area the artist ended in.</param>
    /// <param name="lifeSpanBegin">The optional date the artist started existing.</param>
    /// <param name="lifeSpanEnd">The optional date the artist stopped existing.</param>
    /// <param name="isEnded">Whether the artist no longer exists.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <param name="ipis">The list of IPI codes of the artist.</param>
    /// <param name="isnis">The list of ISNI codes of the artist.</param>
    /// <param name="aliases">The list of alternative names of the artist.</param>
    /// <param name="genres">The list of genres associated with the artist.</param>
    /// <param name="tags">The list of tags associated with the artist.</param>
    /// <param name="ratings">The list of ratings of the artist.</param>
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
        Optional<string> sortName,
        Optional<string> disambiguation,
        Optional<MusicArtistType> type,
        Optional<MusicArtistGender> gender,
        Optional<string> country,
        Optional<MusicArea> area,
        Optional<MusicArea> beginArea,
        Optional<MusicArea> endArea,
        Optional<DateOnly> lifeSpanBegin,
        Optional<DateOnly> lifeSpanEnd,
        bool isEnded,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        List<string> ipis,
        List<string> isnis,
        List<MusicArtistAlias> aliases,
        List<Genre> genres,
        List<Tag> tags,
        List<AudioRating> ratings,
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
            sortName,
            disambiguation,
            type,
            gender,
            country,
            area,
            beginArea,
            endArea,
            lifeSpanBegin,
            lifeSpanEnd,
            isEnded,
            website,
            musicBrainzArtistId,
            ipis,
            isnis,
            aliases,
            genres,
            tags,
            ratings,
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
        // Replace the contents of the collection in place, preserving the readonly reference invariants of the aggregate.
        _contributors.Clear();
        _contributors.UnionWith(contributors);
    }

    /// <summary>
    /// Replaces the aliases of the artist with the provided <paramref name="aliases"/>.
    /// </summary>
    /// <param name="aliases">The alternative names of the artist.</param>
    public void UpdateAliases(IReadOnlyCollection<MusicArtistAlias> aliases)
    {
        // Replace the contents of the collection in place, preserving the readonly reference invariants of the aggregate.
        _aliases.Clear();
        _aliases.AddRange(aliases);
    }

    /// <summary>
    /// Replaces the IPI codes of the artist with the provided <paramref name="ipis"/>.
    /// </summary>
    /// <param name="ipis">The IPI codes of the artist.</param>
    public void UpdateIpis(IReadOnlyCollection<string> ipis)
    {
        // Replace the contents of the collection in place, preserving the readonly reference invariants of the aggregate.
        _ipis.Clear();
        _ipis.AddRange(ipis);
    }

    /// <summary>
    /// Replaces the ISNI codes of the artist with the provided <paramref name="isnis"/>.
    /// </summary>
    /// <param name="isnis">The ISNI codes of the artist.</param>
    public void UpdateIsnis(IReadOnlyCollection<string> isnis)
    {
        // Replace the contents of the collection in place, preserving the readonly reference invariants of the aggregate.
        _isnis.Clear();
        _isnis.AddRange(isnis);
    }

    /// <summary>
    /// Replaces the genres of the artist with the provided <paramref name="genres"/>.
    /// </summary>
    /// <param name="genres">The genres of the artist.</param>
    public void UpdateGenres(IReadOnlyCollection<Genre> genres)
    {
        // Replace the contents of the collection in place, preserving the readonly reference invariants of the aggregate.
        _genres.Clear();
        _genres.AddRange(genres);
    }

    /// <summary>
    /// Replaces the tags of the artist with the provided <paramref name="tags"/>.
    /// </summary>
    /// <param name="tags">The tags of the artist.</param>
    public void UpdateTags(IReadOnlyCollection<Tag> tags)
    {
        // Replace the contents of the collection in place, preserving the readonly reference invariants of the aggregate.
        _tags.Clear();
        _tags.AddRange(tags);
    }

    /// <summary>
    /// Replaces the ratings of the artist with the provided <paramref name="ratings"/>.
    /// </summary>
    /// <param name="ratings">The ratings of the artist.</param>
    public void UpdateRatings(IReadOnlyCollection<AudioRating> ratings)
    {
        // Replace the contents of the collection in place, preserving the readonly reference invariants of the aggregate.
        _ratings.Clear();
        _ratings.AddRange(ratings);
    }

    /// <summary>
    /// Updates the details of the artist, without touching the albums, the contributors, and the media library metadata collections.
    /// </summary>
    /// <param name="name">The name of the artist.</param>
    /// <param name="sortName">The optional sort name of the artist.</param>
    /// <param name="disambiguation">The optional disambiguation comment of the artist.</param>
    /// <param name="type">The optional type of the artist.</param>
    /// <param name="gender">The optional gender of the artist.</param>
    /// <param name="country">The optional ISO 3166-1 alpha-2 code of the country of the artist.</param>
    /// <param name="area">The optional area the artist is primarily identified with.</param>
    /// <param name="beginArea">The optional area the artist began in.</param>
    /// <param name="endArea">The optional area the artist ended in.</param>
    /// <param name="lifeSpanBegin">The optional date the artist started existing.</param>
    /// <param name="lifeSpanEnd">The optional date the artist stopped existing.</param>
    /// <param name="isEnded">Whether the artist no longer exists.</param>
    /// <param name="website">The optional website of the artist.</param>
    /// <param name="musicBrainzArtistId">The optional MusicBrainz identifier of the artist.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Updated> UpdateDetails(
        string name,
        Optional<string> sortName,
        Optional<string> disambiguation,
        Optional<MusicArtistType> type,
        Optional<MusicArtistGender> gender,
        Optional<string> country,
        Optional<MusicArea> area,
        Optional<MusicArea> beginArea,
        Optional<MusicArea> endArea,
        Optional<DateOnly> lifeSpanBegin,
        Optional<DateOnly> lifeSpanEnd,
        bool isEnded,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Music.ArtistNameCannotBeEmpty;

        Name = name;
        SortName = sortName;
        Disambiguation = disambiguation;
        Type = type;
        Gender = gender;
        Country = country;
        Area = area;
        BeginArea = beginArea;
        EndArea = endArea;
        LifeSpanBegin = lifeSpanBegin;
        LifeSpanEnd = lifeSpanEnd;
        IsEnded = isEnded;
        Website = website;
        MusicBrainzArtistId = musicBrainzArtistId;
        UpdatedOnUtc = Optional<DateTime>.Some(DateTime.UtcNow);
        return Result.Updated;
    }

    /// <summary>
    /// Updates an album of the artist, together with its media contributors and ratings, without touching its tracks.
    /// </summary>
    /// <remarks>
    /// An album is a child entity of the artist aggregate, not an aggregate root, so it is mutated through its owning artist,
    /// which is the consistency boundary. Book is itself an aggregate root, so its update flow reconstitutes it directly
    /// with its own Create method instead. This difference is intentional.
    /// </remarks>
    /// <param name="album">The album to update.</param>
    /// <param name="metadata">The album metadata of the album.</param>
    /// <param name="disambiguation">The optional disambiguation comment of the album.</param>
    /// <param name="mediaFormat">The optional physical or digital medium of the album.</param>
    /// <param name="packaging">The optional outermost physical packaging of the album.</param>
    /// <param name="script">The optional script used by the language of the release of the album.</param>
    /// <param name="barcode">The optional barcode of the album.</param>
    /// <param name="catalogNumbers">The catalog numbers of the album.</param>
    /// <param name="label">The optional name of the label that issued the album.</param>
    /// <param name="asin">The optional ASIN of the album.</param>
    /// <param name="musicBrainzReleaseId">The optional MusicBrainz identifier of the release.</param>
    /// <param name="musicBrainzReleaseGroupId">The optional MusicBrainz identifier of the release group.</param>
    /// <param name="musicBrainzReleaseArtistId">The optional MusicBrainz identifier of the release artist.</param>
    /// <param name="contributors">The media contributors of the album.</param>
    /// <param name="ratings">The ratings of the album.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Updated> UpdateAlbum(
        Album album,
        AlbumMetadata metadata,
        Optional<string> disambiguation,
        Optional<MusicMediaFormat> mediaFormat,
        Optional<MusicReleasePackaging> packaging,
        Optional<string> script,
        Optional<Barcode> barcode,
        List<string> catalogNumbers,
        Optional<string> label,
        Optional<string> asin,
        Optional<MusicBrainzId> musicBrainzReleaseId,
        Optional<MusicBrainzId> musicBrainzReleaseGroupId,
        Optional<MusicBrainzId> musicBrainzReleaseArtistId,
        IReadOnlyCollection<MusicMediaContributor> contributors,
        IReadOnlyCollection<AudioRating> ratings)
    {
        // The album is an entity inside this aggregate, referenced by object, so it must be one of the albums of the artist.
        if (!_albums.Contains(album))
            return Errors.Music.AlbumNotFound;

        album.UpdateDetails(metadata, disambiguation, mediaFormat, packaging, script, barcode, catalogNumbers, label, asin, musicBrainzReleaseId, musicBrainzReleaseGroupId, musicBrainzReleaseArtistId);
        album.UpdateContributors(contributors);
        album.UpdateRatings(ratings);
        UpdatedOnUtc = Optional<DateTime>.Some(DateTime.UtcNow);
        return Result.Updated;
    }

    /// <summary>
    /// Updates a track of an album of the artist, together with its media contributors, ratings, moods and ISRCs.
    /// </summary>
    /// <remarks>
    /// A track is a child entity of the artist aggregate, not an aggregate root, so it is mutated through its owning artist,
    /// which is the consistency boundary. Book is itself an aggregate root, so its update flow reconstitutes it directly
    /// with its own Create method instead. This difference is intentional.
    /// </remarks>
    /// <param name="album">The album the track belongs to.</param>
    /// <param name="track">The track to update.</param>
    /// <param name="path">The file system path of the track.</param>
    /// <param name="metadata">The audio metadata of the track.</param>
    /// <param name="trackNumber">The number of the track on its disc.</param>
    /// <param name="discNumber">The optional number of the disc the track belongs to.</param>
    /// <param name="script">The optional script used by the language of the track.</param>
    /// <param name="key">The optional musical key of the track.</param>
    /// <param name="bpm">The optional tempo of the track in beats per minute.</param>
    /// <param name="isVideo">Whether the recording of the track is a video recording.</param>
    /// <param name="work">The optional work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">The optional MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">The optional MusicBrainz identifier of the track.</param>
    /// <param name="moods">The moods of the track.</param>
    /// <param name="isrcs">The ISRCs of the track.</param>
    /// <param name="contributors">The media contributors of the track.</param>
    /// <param name="ratings">The ratings of the track.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Updated> UpdateTrackInAlbum(
        Album album,
        Track track,
        string path,
        AudioMetadata metadata,
        int trackNumber,
        Optional<int> discNumber,
        Optional<string> script,
        Optional<MusicKey> key,
        Optional<int> bpm,
        bool isVideo,
        Optional<MusicWork> work,
        Optional<MusicBrainzId> musicBrainzRecordingId,
        Optional<MusicBrainzId> musicBrainzTrackId,
        IReadOnlyCollection<Mood> moods,
        IReadOnlyCollection<Isrc> isrcs,
        IReadOnlyCollection<MusicMediaContributor> contributors,
        IReadOnlyCollection<AudioRating> ratings)
    {
        // The album and the track are entities inside this aggregate, referenced by object, so they must belong to the artist, respectively to the album.
        if (!_albums.Contains(album))
            return Errors.Music.AlbumNotFound;
        if (!album.Tracks.Contains(track))
            return Errors.Music.TrackNotFound;

        track.UpdateDetails(
            path,
            metadata,
            trackNumber,
            discNumber,
            script,
            key,
            bpm,
            isVideo,
            work,
            musicBrainzRecordingId,
            musicBrainzTrackId);
        track.UpdateMoods(moods);
        track.UpdateIsrcs(isrcs);
        track.UpdateContributors(contributors);
        track.UpdateRatings(ratings);
        UpdatedOnUtc = Optional<DateTime>.Some(DateTime.UtcNow);
        return Result.Updated;
    }

    /// <summary>
    /// Adds a track to an album of the artist.
    /// </summary>
    /// <param name="album">The album the track is added to.</param>
    /// <param name="track">The track to be added.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    /// <remarks>
    /// A track is a child entity of the artist aggregate, not an aggregate root, so it is added through its owning artist,
    /// which is the consistency boundary. Book and Artist are themselves aggregate roots, so they are created directly instead.
    /// This difference is intentional.
    /// </remarks>
    public Result<Created> AddTrackToAlbum(Album album, Track track)
    {
        // The album is an entity inside this aggregate, referenced by object, so it must be one of the albums of the artist.
        if (!_albums.Contains(album))
            return Errors.Music.AlbumNotFound;

        Result<Created> addResult = album.AddTrack(track);
        if (addResult.IsFailure)
            return addResult.Errors;

        return Result.Created;
    }

    /// <summary>
    /// Removes a track from an album of the artist.
    /// </summary>
    /// <param name="album">The album the track is removed from.</param>
    /// <param name="track">The track to remove.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    /// <remarks>
    /// A track is a child entity of the artist aggregate, not an aggregate root, so it is removed through its owning artist,
    /// which is the consistency boundary. This difference is intentional.
    /// </remarks>
    public Result<Deleted> RemoveTrackFromAlbum(Album album, Track track)
    {
        // The album and the track are entities inside this aggregate, referenced by object, so they must belong to the artist, respectively to the album.
        if (!_albums.Contains(album))
            return Errors.Music.AlbumNotFound;
        if (!album.Tracks.Contains(track))
            return Errors.Music.TrackNotFound;

        return album.RemoveTrack(track);
    }
}
