#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Common;

/// <summary>
/// Fixture class for the <see cref="MediaArtworkDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaArtworkDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MediaArtworkDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="type">Optional. The type of the artwork.</param>
    /// <param name="ordinal">Optional. The ordinal of the artwork within its type.</param>
    /// <param name="path">Optional. The relative path of the stored artwork file.</param>
    /// <returns>A configured <see cref="MediaArtworkDto"/> instance.</returns>
    public MediaArtworkDto Create(
        ArtworkType? type = null,
        int? ordinal = null,
        string? path = null)
    {
        return new MediaArtworkDto
        {
            Type = type ?? _faker.PickRandom<ArtworkType>(),
            Ordinal = ordinal ?? _faker.Random.Int(1, 10),
            Path = path ?? _faker.System.FilePath()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MediaArtworkDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaArtworkDto"/> instances.</returns>
    public List<MediaArtworkDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
