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
/// Fixture class for the <see cref="MusicBrainzLabelInfoResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzLabelInfoResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzLabelResponseFixture _musicBrainzLabelResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzLabelInfoResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="catalogNumber">Optional. The catalog number assigned by the label.</param>
    /// <param name="includeCatalogNumber">Whether the catalog number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="label">Optional. The label that issued the release.</param>
    /// <param name="includeLabel">Whether the label should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzLabelInfoResponse"/> instance.</returns>
    public MusicBrainzLabelInfoResponse Create(
        string? catalogNumber = null,
        bool includeCatalogNumber = true,
        MusicBrainzLabelResponse? label = null,
        bool includeLabel = true)
    {
        return new MusicBrainzLabelInfoResponse
        {
            CatalogNumber = includeCatalogNumber ? catalogNumber ?? _faker.Random.AlphaNumeric(8).ToUpperInvariant() : null,
            Label = includeLabel ? label ?? _musicBrainzLabelResponseFixture.Create() : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzLabelInfoResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzLabelInfoResponse"/> instances.</returns>
    public List<MusicBrainzLabelInfoResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
