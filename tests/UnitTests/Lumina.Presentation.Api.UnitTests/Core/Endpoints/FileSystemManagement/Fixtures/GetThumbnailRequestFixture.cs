#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;

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
    /// <param name="path">The path of the file to get the thumbnail for.</param>
    /// <param name="quality">The quality of the thumbnail.</param>
    /// <returns>The created <see cref="GetThumbnailRequest"/>.</returns>
    public GetThumbnailRequest Create(string? path = null, int? quality = null)
    {
        return new GetThumbnailRequest(
            Path: path ?? _faker.System.FilePath(),
            Quality: quality ?? _faker.Random.Int(1, 100)
        );
    }
}
