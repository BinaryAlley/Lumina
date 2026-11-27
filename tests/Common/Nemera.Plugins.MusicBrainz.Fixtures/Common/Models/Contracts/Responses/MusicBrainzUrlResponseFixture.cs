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
/// Fixture class for the <see cref="MusicBrainzUrlResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzUrlResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzUrlResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The MusicBrainz identifier of the URL.</param>
    /// <param name="includeId">Whether the identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="resource">Optional. The resource the URL points to.</param>
    /// <param name="includeResource">Whether the resource should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzUrlResponse"/> instance.</returns>
    public MusicBrainzUrlResponse Create(
        string? id = null,
        bool includeId = true,
        string? resource = null,
        bool includeResource = true)
    {
        return new MusicBrainzUrlResponse
        {
            Id = includeId ? id ?? Guid.NewGuid().ToString() : null,
            Resource = includeResource ? resource ?? _faker.Internet.Url() : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzUrlResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzUrlResponse"/> instances.</returns>
    public List<MusicBrainzUrlResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
