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
/// Fixture class for the <see cref="MusicBrainzTextRepresentationResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzTextRepresentationResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzTextRepresentationResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="language">Optional. The ISO 639-3 language code of the release.</param>
    /// <param name="includeLanguage">Whether the language should be included, or forced to <see langword="null"/>.</param>
    /// <param name="script">Optional. The ISO 15924 script code of the release.</param>
    /// <param name="includeScript">Whether the script should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzTextRepresentationResponse"/> instance.</returns>
    public MusicBrainzTextRepresentationResponse Create(
        string? language = null,
        bool includeLanguage = true,
        string? script = null,
        bool includeScript = true)
    {
        return new MusicBrainzTextRepresentationResponse
        {
            Language = includeLanguage ? language ?? _faker.PickRandom("eng", "deu", "fra") : null,
            Script = includeScript ? script ?? _faker.PickRandom("Latn", "Cyrl", "Arab") : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzTextRepresentationResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzTextRepresentationResponse"/> instances.</returns>
    public List<MusicBrainzTextRepresentationResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
