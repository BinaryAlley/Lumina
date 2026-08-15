#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Thumbnails;

/// <summary>
/// Fixture class for the <see cref="GetThumbnailRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="GetThumbnailRequest"/> with default or random values.
    /// </summary>
    /// <param name="path">Optional. The path of the file to get the thumbnail for.</param>
    /// <param name="quality">Optional. The quality of the thumbnail.</param>
    /// <returns>The created <see cref="GetThumbnailRequest"/>.</returns>
    public GetThumbnailRequest Create(string? path = null, int? quality = null)
    {
        return new GetThumbnailRequest(
            Path: path ?? _faker.System.FilePath(),
            Quality: quality ?? _faker.Random.Int(1, 100)
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetThumbnailRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetThumbnailRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
