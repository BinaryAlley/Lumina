#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="Thumbnail"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="Thumbnail"/>.
    /// </summary>
    /// <param name="type">Optional. The <see cref="ImageType"/> of the thumbnail. If not provided, a random image type is selected.</param>
    /// <param name="bytes">Optional. The byte array representing the image data. If not provided, a random 1KB byte array is generated.</param>
    /// <returns>A newly created <see cref="Thumbnail"/> instance.</returns>
    public Thumbnail Create(ImageType? type = null, byte[]? bytes = null)
    {
        type ??= _faker.PickRandom<ImageType>();
        bytes ??= _faker.Random.Bytes(1024); // Generate 1KB of random bytes

        return new Thumbnail(type.Value, bytes);
    }

    /// <summary>
    /// Creates a list of <see cref="Thumbnail"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<Thumbnail> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
