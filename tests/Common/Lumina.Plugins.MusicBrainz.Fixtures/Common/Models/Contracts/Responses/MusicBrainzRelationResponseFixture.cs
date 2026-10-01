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
/// Fixture class for the <see cref="MusicBrainzRelationResponse"/> record.
/// The nested artist, work and url targets are optional, because a relation usually targets only one of them.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzRelationResponseFixture
{
    private readonly Faker _faker = new();

    // The artist and work fixtures each compose a relation fixture of their own, so holding them eagerly here would recurse forever; they are
    // therefore created on first use. The URL fixture has no such dependency, so it is held directly.
    private readonly Lazy<MusicBrainzArtistResponseFixture> _musicBrainzArtistResponseFixture = new(() => new MusicBrainzArtistResponseFixture());
    private readonly Lazy<MusicBrainzWorkResponseFixture> _musicBrainzWorkResponseFixture = new(() => new MusicBrainzWorkResponseFixture());
    private readonly MusicBrainzUrlResponseFixture _musicBrainzUrlResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzRelationResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="type">Optional. The type of the relationship.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="typeId">Optional. The MusicBrainz identifier of the type of the relationship.</param>
    /// <param name="includeTypeId">Whether the type identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="direction">Optional. The direction of the relationship.</param>
    /// <param name="includeDirection">Whether the direction should be included, or forced to <see langword="null"/>.</param>
    /// <param name="targetType">Optional. The type of the entity the relationship targets.</param>
    /// <param name="includeTargetType">Whether the target type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="artist">Optional. The artist the relationship targets.</param>
    /// <param name="includeArtist">Whether the artist should be included, or forced to <see langword="null"/>.</param>
    /// <param name="work">Optional. The work the relationship targets.</param>
    /// <param name="includeWork">Whether the work should be included, or forced to <see langword="null"/>.</param>
    /// <param name="url">Optional. The URL the relationship targets.</param>
    /// <param name="includeUrl">Whether the URL should be included, or forced to <see langword="null"/>.</param>
    /// <param name="attributes">Optional. The attributes of the relationship.</param>
    /// <returns>A configured <see cref="MusicBrainzRelationResponse"/> instance.</returns>
    public MusicBrainzRelationResponse Create(
        string? type = null,
        bool includeType = true,
        string? typeId = null,
        bool includeTypeId = true,
        string? direction = null,
        bool includeDirection = true,
        string? targetType = null,
        bool includeTargetType = true,
        MusicBrainzArtistResponse? artist = null,
        bool includeArtist = false,
        MusicBrainzWorkResponse? work = null,
        bool includeWork = false,
        MusicBrainzUrlResponse? url = null,
        bool includeUrl = false,
        List<string>? attributes = null)
    {
        return new MusicBrainzRelationResponse
        {
            Type = includeType ? type ?? _faker.PickRandom("producer", "composer", "performer") : null,
            TypeId = includeTypeId ? typeId ?? Guid.NewGuid().ToString() : null,
            Direction = includeDirection ? direction ?? "backward" : null,
            TargetType = includeTargetType ? targetType ?? "artist" : null,
            Artist = includeArtist ? artist ?? _musicBrainzArtistResponseFixture.Value.Create() : null,
            Work = includeWork ? work ?? _musicBrainzWorkResponseFixture.Value.Create() : null,
            Url = includeUrl ? url ?? _musicBrainzUrlResponseFixture.Create() : null,
            Attributes = attributes ?? []
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzRelationResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzRelationResponse"/> instances.</returns>
    public List<MusicBrainzRelationResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
