#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetTreeDirectories;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetTreeDirectories.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetTreeDirectoriesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeDirectoriesQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get tree directories.
    /// </summary>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns>The created query to get tree directory.</returns>
    public static GetTreeDirectoriesQuery CreateGetTreeDirectoryQuery(bool includeHiddenElements = false)
    {
        return new Faker<GetTreeDirectoriesQuery>()
            .CustomInstantiator(f => new GetTreeDirectoriesQuery(
                default!,
                default
            ))
            .RuleFor(x => x.Path, f => f.System.DirectoryPath())
            .RuleFor(x => x.IncludeHiddenElements, f => includeHiddenElements);
    }
}
