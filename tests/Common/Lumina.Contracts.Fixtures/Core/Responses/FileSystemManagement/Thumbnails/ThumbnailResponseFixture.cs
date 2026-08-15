#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Thumbnails;

/// <summary>
/// Fixture class for the <see cref="ThumbnailResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ThumbnailResponse"/>.
    /// </summary>
    /// <param name="type">Optional. The thumbnail image type.</param>
    /// <param name="bytes">Optional. The thumbnail image bytes.</param>
    /// <returns>The created <see cref="ThumbnailResponse"/>.</returns>
    public ThumbnailResponse Create(ImageType? type = null, byte[]? bytes = null)
    {
        Faker faker = new();
        return new ThumbnailResponse(
            type ?? faker.PickRandom<ImageType>(),
            bytes ?? faker.Random.Bytes(20)
        );
    }

    /// <summary>
    /// Creates a list of <see cref="ThumbnailResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ThumbnailResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
