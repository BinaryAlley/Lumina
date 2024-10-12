#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileManagement.Thumbnails.Queries.GetThumbnail;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Thumbnails.Queries.GetThumbnail.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetThumbnailQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get a thumbnail.
    /// </summary>
    /// <returns>The created query.</returns>
    public static GetThumbnailQuery CreateGetThumbnailQuery()
    {
        return new Faker<GetThumbnailQuery>()
            .CustomInstantiator(f => new GetThumbnailQuery(
                default!,
                default
            ))
            .RuleFor(x => x.Path, f => f.System.FilePath())
            .RuleFor(x => x.Quality, f => f.Random.Int());
    }
}
