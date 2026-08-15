#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Fixture class for the <see cref="GetDirectoriesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get directories.
    /// </summary>
    /// <param name="path">Optional. The file system path.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns>The created query to get directories.</returns>
    public GetDirectoriesQuery Create(string? path = null, bool includeHiddenElements = false)
    {
        return new Faker<GetDirectoriesQuery>()
            .CustomInstantiator(f => new GetDirectoriesQuery(
                default!,
                default
            ))
            .RuleFor(x => x.Path, f => path ?? f.System.DirectoryPath())
            .RuleFor(x => x.IncludeHiddenElements, f => includeHiddenElements);
    }

    /// <summary>
    /// Creates a list of <see cref="GetDirectoriesQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetDirectoriesQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
