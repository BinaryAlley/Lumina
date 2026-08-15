#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Paths.Queries.GetPathRoot;

/// <summary>
/// Fixture class for the <see cref="GetPathRootQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the root of a path.
    /// </summary>
    /// <param name="path">Optional. The file system path.</param>
    /// <returns>The created query.</returns>
    public GetPathRootQuery Create(string? path = null)
    {
        return new Faker<GetPathRootQuery>()
            .CustomInstantiator(f => new GetPathRootQuery(
                default!))
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath());
    }

    /// <summary>
    /// Creates a list of <see cref="GetPathRootQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetPathRootQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
