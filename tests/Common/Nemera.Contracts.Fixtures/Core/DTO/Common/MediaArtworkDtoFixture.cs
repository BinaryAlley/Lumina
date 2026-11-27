#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.Common;

/// <summary>
/// Fixture class for the <see cref="MediaArtworkDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaArtworkDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaArtworkDto"/>.
    /// </summary>
    /// <param name="type">Optional. The type of the artwork.</param>
    /// <param name="ordinal">Optional. The ordinal of the artwork within its type.</param>
    /// <param name="path">Optional. The relative path of the stored artwork file.</param>
    /// <returns>The created <see cref="MediaArtworkDto"/>.</returns>
    public MediaArtworkDto Create(
        ArtworkType? type = null,
        int? ordinal = null,
        string? path = null)
    {
        return new MediaArtworkDto(
            type ?? _faker.PickRandom<ArtworkType>(),
            ordinal ?? _faker.Random.Int(0, 5),
            path ?? _faker.System.FilePath());
    }

    /// <summary>
    /// Creates a list of <see cref="MediaArtworkDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MediaArtworkDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
