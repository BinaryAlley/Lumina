#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Paths.Queries.CheckPathExists;

/// <summary>
/// Fixture class for the <see cref="CheckPathExistsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsQueryFixture
{
    /// <summary>
    /// Creates a random valid query to check if paths exist.
    /// </summary>
    /// <param name="path">Optional. The file system path.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns>The created query.</returns>
    public CheckPathExistsQuery Create(string? path = null, bool includeHiddenElements = true)
    {
        return new Faker<CheckPathExistsQuery>()
            .CustomInstantiator(f => new CheckPathExistsQuery(
                default!,
                default
            ))
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath())
            .RuleFor(x => x.IncludeHiddenElements, includeHiddenElements);
    }

    /// <summary>
    /// Creates a list of <see cref="CheckPathExistsQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<CheckPathExistsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
