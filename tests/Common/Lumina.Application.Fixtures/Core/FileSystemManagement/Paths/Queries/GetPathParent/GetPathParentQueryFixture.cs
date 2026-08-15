#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Paths.Queries.GetPathParent;

/// <summary>
/// Fixture class for the <see cref="GetPathParentQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the parent of a path.
    /// </summary>
    /// <param name="path">Optional. The file system path.</param>
    /// <returns>The created query.</returns>
    public GetPathParentQuery Create(string? path = null)
    {
        return new Faker<GetPathParentQuery>()
            .CustomInstantiator(f => new GetPathParentQuery(
                default!))
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath());
    }

    /// <summary>
    /// Creates a list of <see cref="GetPathParentQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetPathParentQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
