#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Fixture class for the <see cref="GetThumbnailQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get a thumbnail.
    /// </summary>
    /// <param name="path">Optional. The file system path.</param>
    /// <param name="quality">Optional. The thumbnail quality.</param>
    /// <returns>The created query.</returns>
    public GetThumbnailQuery Create(string? path = null, int? quality = null)
    {
        return new Faker<GetThumbnailQuery>()
            .CustomInstantiator(f => new GetThumbnailQuery(
                default!,
                default
            ))
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath())
            .RuleFor(x => x.Quality, f => quality ?? f.Random.Int());
    }

    /// <summary>
    /// Creates a list of <see cref="GetThumbnailQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetThumbnailQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
