#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;

/// <summary>
/// Fixture class for the <see cref="Album"/> domain entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumFixture
{
    private readonly Faker _faker = new();
    private readonly AlbumMetadataFixture _albumMetadataFixture = new();
    private readonly BarcodeFixture _barcodeFixture = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();
    private readonly AudioRatingFixture _audioRatingFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="Album"/> domain entity.
    /// </summary>
    /// <param name="metadata">Optional. The album metadata of the album.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the album.</param>
    /// <param name="mediaFormat">Optional. The physical or digital medium of the album.</param>
    /// <param name="packaging">Optional. The outermost physical packaging of the album.</param>
    /// <param name="script">Optional. The script used by the language of the release of the album.</param>
    /// <param name="barcode">Optional. The barcode of the album.</param>
    /// <param name="catalogNumber">Optional. The catalog number of the album.</param>
    /// <param name="label">Optional. The name of the label that issued the album.</param>
    /// <param name="asin">Optional. The ASIN of the album.</param>
    /// <param name="musicBrainzReleaseId">Optional. The MusicBrainz identifier of the release.</param>
    /// <param name="musicBrainzReleaseGroupId">Optional. The MusicBrainz identifier of the release group.</param>
    /// <param name="musicBrainzReleaseArtistId">Optional. The MusicBrainz identifier of the release artist.</param>
    /// <param name="contributors">Optional. The media contributors of the album.</param>
    /// <param name="ratings">Optional. The ratings of the album.</param>
    /// <param name="tracks">Optional. The tracks of the album. Defaults to an empty list so callers can control the initial tracks.</param>
    /// <returns>The created <see cref="Album"/> domain entity.</returns>
    public Album Create(
        AlbumMetadata? metadata = null,
        Optional<string>? disambiguation = null,
        Optional<MusicMediaFormat>? mediaFormat = null,
        Optional<MusicReleasePackaging>? packaging = null,
        Optional<string>? script = null,
        Optional<Barcode>? barcode = null,
        List<string>? catalogNumbers = null,
        Optional<string>? label = null,
        Optional<string>? asin = null,
        Optional<MusicBrainzId>? musicBrainzReleaseId = null,
        Optional<MusicBrainzId>? musicBrainzReleaseGroupId = null,
        Optional<MusicBrainzId>? musicBrainzReleaseArtistId = null,
        List<MusicMediaContributor>? contributors = null,
        List<AudioRating>? ratings = null,
        List<Track>? tracks = null)
    {
        Result<Album> albumResult = Album.Create(
            metadata ?? _albumMetadataFixture.Create(),
            disambiguation ?? Optional<string>.Some(_faker.Lorem.Sentence()),
            mediaFormat ?? Optional<MusicMediaFormat>.Some(_faker.PickRandom<MusicMediaFormat>()),
            packaging ?? Optional<MusicReleasePackaging>.Some(_faker.PickRandom<MusicReleasePackaging>()),
            script ?? Optional<string>.Some(_faker.Random.String2(4)),
            barcode ?? Optional<Barcode>.Some(_barcodeFixture.Create()),
            catalogNumbers ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _faker.Random.String2(_faker.Random.Int(5, 20)))],
            label ?? Optional<string>.Some(_faker.Company.CompanyName()),
            asin ?? Optional<string>.Some(_faker.Random.AlphaNumeric(10)),
            musicBrainzReleaseId ?? Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create()),
            musicBrainzReleaseGroupId ?? Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create()),
            musicBrainzReleaseArtistId ?? Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create()),
            contributors ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 3)).Select(_ => _musicMediaContributorFixture.Create())],
            ratings ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _audioRatingFixture.Create())],
            tracks ?? []);

        if (albumResult.IsFailure)
            throw new InvalidOperationException("Failed to create Album: " + string.Join(", ", albumResult.Errors));
        return albumResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Album"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Album"/> instances.</returns>
    public List<Album> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
