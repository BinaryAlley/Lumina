#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetFiles;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.FileSystemManagement.Files.Queries.GetFiles;

/// <summary>
/// Fixture class for the <see cref="GetFilesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get files.
    /// </summary>
    /// <param name="path">Optional. The file system path.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns>The created query to get files.</returns>
    public GetFilesQuery Create(string? path = null, bool includeHiddenElements = false)
    {
        return new Faker<GetFilesQuery>()
            .CustomInstantiator(f => new GetFilesQuery(
                default!,
                default
            ))
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath())
            .RuleFor(x => x.IncludeHiddenElements, f => includeHiddenElements);
    }

    /// <summary>
    /// Creates a list of <see cref="GetFilesQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetFilesQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
