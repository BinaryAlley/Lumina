#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;

/// <summary>
/// Fixture class for the <see cref="Artist"/> domain aggregate root.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistFixture
{
    private const int MINIMUM_LIFESPAN_YEAR = 1900;
    private const int MAXIMUM_LIFESPAN_YEAR = 2026;

    private readonly Faker _faker = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly AlbumFixture _albumFixture = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();
    private readonly MusicAreaFixture _musicAreaFixture = new();
    private readonly MusicArtistAliasFixture _musicArtistAliasFixture = new();
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly AudioRatingFixture _audioRatingFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="Artist"/> domain aggregate root.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library this artist belongs to.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <param name="sortName">Optional. The sort name of the artist.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the artist.</param>
    /// <param name="type">Optional. The type of the artist.</param>
    /// <param name="gender">Optional. The gender of the artist.</param>
    /// <param name="country">Optional. The country of the artist.</param>
    /// <param name="area">Optional. The area the artist is primarily identified with.</param>
    /// <param name="beginArea">Optional. The area the artist began in.</param>
    /// <param name="endArea">Optional. The area the artist ended in.</param>
    /// <param name="lifeSpanBegin">Optional. The date the artist started existing.</param>
    /// <param name="lifeSpanEnd">Optional. The date the artist stopped existing.</param>
    /// <param name="isEnded">Optional. Whether the artist no longer exists.</param>
    /// <param name="website">Optional. The website of the artist.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="ipis">Optional. The IPI codes of the artist.</param>
    /// <param name="isnis">Optional. The ISNI codes of the artist.</param>
    /// <param name="aliases">Optional. The alternative names of the artist.</param>
    /// <param name="genres">Optional. The genres associated with the artist.</param>
    /// <param name="tags">Optional. The tags associated with the artist.</param>
    /// <param name="ratings">Optional. The ratings of the artist.</param>
    /// <param name="contributors">Optional. The media contributors that make up the artist.</param>
    /// <param name="albums">Optional. The albums of the artist. Defaults to a single random album, since an artist must have at least one.</param>
    /// <returns>The created <see cref="Artist"/> domain aggregate root.</returns>
    public Artist Create(
        LibraryId? libraryId = null,
        string? name = null,
        Optional<string>? sortName = null,
        Optional<string>? disambiguation = null,
        Optional<MusicArtistType>? type = null,
        Optional<MusicArtistGender>? gender = null,
        Optional<string>? country = null,
        Optional<MusicArea>? area = null,
        Optional<MusicArea>? beginArea = null,
        Optional<MusicArea>? endArea = null,
        Optional<DateOnly>? lifeSpanBegin = null,
        Optional<DateOnly>? lifeSpanEnd = null,
        bool? isEnded = null,
        Optional<string>? website = null,
        Optional<MusicBrainzId>? musicBrainzArtistId = null,
        List<string>? ipis = null,
        List<string>? isnis = null,
        List<MusicArtistAlias>? aliases = null,
        List<Genre>? genres = null,
        List<Tag>? tags = null,
        List<AudioRating>? ratings = null,
        List<MusicMediaContributor>? contributors = null,
        List<Album>? albums = null)
    {
        int lifeSpanBeginYear = Random.Shared.Next(MINIMUM_LIFESPAN_YEAR, MAXIMUM_LIFESPAN_YEAR);
        int lifeSpanEndYear = Random.Shared.Next(lifeSpanBeginYear, Math.Max(lifeSpanBeginYear + 1, MAXIMUM_LIFESPAN_YEAR));
        Result<Artist> artistResult = Artist.Create(
            libraryId ?? _libraryIdFixture.Create(),
            name ?? _faker.Name.FullName(),
            sortName ?? Optional<string>.Some(_faker.Name.FullName()),
            disambiguation ?? Optional<string>.Some(_faker.Lorem.Sentence()),
            type ?? Optional<MusicArtistType>.Some(_faker.PickRandom<MusicArtistType>()),
            gender ?? Optional<MusicArtistGender>.Some(_faker.PickRandom<MusicArtistGender>()),
            country ?? Optional<string>.Some(_faker.Address.CountryCode()),
            area ?? Optional<MusicArea>.Some(_musicAreaFixture.Create()),
            beginArea ?? Optional<MusicArea>.Some(_musicAreaFixture.Create()),
            endArea ?? Optional<MusicArea>.Some(_musicAreaFixture.Create()),
            lifeSpanBegin ?? Optional<DateOnly>.Some(new DateOnly(lifeSpanBeginYear, 1, 1)),
            lifeSpanEnd ?? Optional<DateOnly>.Some(new DateOnly(lifeSpanEndYear, 1, 1)),
            isEnded ?? _faker.Random.Bool(),
            website ?? Optional<string>.Some(_faker.Internet.Url()),
            musicBrainzArtistId ?? Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create()),
            ipis ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _faker.Random.AlphaNumeric(9))],
            isnis ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _faker.Random.AlphaNumeric(16))],
            aliases ?? _musicArtistAliasFixture.CreateMany(_faker.Random.Int(1, 2)),
            genres ?? _genreFixture.CreateMany(_faker.Random.Int(1, 2)),
            tags ?? _tagFixture.CreateMany(_faker.Random.Int(1, 2)),
            ratings ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _audioRatingFixture.Create())],
            contributors ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 3)).Select(_ => _musicMediaContributorFixture.Create())],
            albums ?? [_albumFixture.Create()]);

        if (artistResult.IsFailure)
            throw new InvalidOperationException("Failed to create Artist: " + string.Join(", ", artistResult.Errors));
        return artistResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Artist"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Artist"/> instances.</returns>
    public List<Artist> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
