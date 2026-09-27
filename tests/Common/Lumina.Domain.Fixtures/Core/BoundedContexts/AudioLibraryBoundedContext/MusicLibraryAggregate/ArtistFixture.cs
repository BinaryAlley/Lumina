#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
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
    private readonly Faker _faker = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly AlbumFixture _albumFixture = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="Artist"/> domain aggregate root.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library this artist belongs to.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <param name="website">Optional. The website of the artist.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="contributors">Optional. The media contributors that make up the artist.</param>
    /// <param name="albums">Optional. The albums of the artist. Defaults to a single random album, since an artist must have at least one.</param>
    /// <returns>The created <see cref="Artist"/> domain aggregate root.</returns>
    public Artist Create(
        LibraryId? libraryId = null,
        string? name = null,
        Optional<string>? website = null,
        Optional<MusicBrainzId>? musicBrainzArtistId = null,
        List<MusicMediaContributor>? contributors = null,
        List<Album>? albums = null)
    {
        Result<Artist> artistResult = Artist.Create(
            libraryId ?? _libraryIdFixture.Create(),
            name ?? _faker.Name.FullName(),
            website ?? Optional<string>.Some(_faker.Internet.Url()),
            musicBrainzArtistId ?? Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create()),
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
