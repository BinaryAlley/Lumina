#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Fixture class for the <see cref="GetTreeFilesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get tree files.
    /// </summary>
    /// <param name="path">Optional. The file system path.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns>The created query to get tree files.</returns>
    public GetTreeFilesQuery Create(string? path = null, bool includeHiddenElements = false)
    {
        return new Faker<GetTreeFilesQuery>()
            .CustomInstantiator(f => new GetTreeFilesQuery(
                default!,
                default
            ))
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath())
            .RuleFor(x => x.IncludeHiddenElements, f => includeHiddenElements);
    }

    /// <summary>
    /// Creates a list of <see cref="GetTreeFilesQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetTreeFilesQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
