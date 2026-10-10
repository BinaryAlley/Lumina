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
/// Fixture class for the <see cref="MusicBrainzLabelResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzLabelResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzLabelResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The MusicBrainz identifier of the label.</param>
    /// <param name="includeId">Whether the identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="name">Optional. The name of the label.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the label.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="labelCode">Optional. The label code of the label.</param>
    /// <param name="includeLabelCode">Whether the label code should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzLabelResponse"/> instance.</returns>
    public MusicBrainzLabelResponse Create(
        string? id = null,
        bool includeId = true,
        string? name = null,
        bool includeName = true,
        string? disambiguation = null,
        bool includeDisambiguation = true,
        int? labelCode = null,
        bool includeLabelCode = true)
    {
        return new MusicBrainzLabelResponse
        {
            Id = includeId ? id ?? Guid.NewGuid().ToString() : null,
            Name = includeName ? name ?? _faker.Company.CompanyName() : null,
            Disambiguation = includeDisambiguation ? disambiguation ?? _faker.Lorem.Sentence(2) : null,
            LabelCode = includeLabelCode ? labelCode ?? _faker.Random.Int(1, 9999) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzLabelResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzLabelResponse"/> instances.</returns>
    public List<MusicBrainzLabelResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
