#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.MusicBrainz.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="MusicBrainzArtistCreditResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzArtistCreditResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzArtistResponseFixture _musicBrainzArtistResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzArtistCreditResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional. The credited name of the artist.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="joinPhrase">Optional. The join phrase that follows the credit.</param>
    /// <param name="includeJoinPhrase">Whether the join phrase should be included, or forced to <see langword="null"/>.</param>
    /// <param name="artist">Optional. The artist the credit refers to.</param>
    /// <param name="includeArtist">Whether the artist should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzArtistCreditResponse"/> instance.</returns>
    public MusicBrainzArtistCreditResponse Create(
        string? name = null,
        bool includeName = true,
        string? joinPhrase = null,
        bool includeJoinPhrase = true,
        MusicBrainzArtistResponse? artist = null,
        bool includeArtist = true)
    {
        string? resolvedName = includeName ? name ?? _faker.Name.FullName() : null;
        return new MusicBrainzArtistCreditResponse
        {
            Name = resolvedName,
            JoinPhrase = includeJoinPhrase ? joinPhrase ?? _faker.PickRandom(" & ", " feat. ", ", ") : null,
            Artist = includeArtist ? artist ?? _musicBrainzArtistResponseFixture.Create(name: resolvedName) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzArtistCreditResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzArtistCreditResponse"/> instances.</returns>
    public List<MusicBrainzArtistCreditResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
