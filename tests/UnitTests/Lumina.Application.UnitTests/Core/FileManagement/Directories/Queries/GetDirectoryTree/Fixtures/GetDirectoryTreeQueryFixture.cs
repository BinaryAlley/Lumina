#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectoryTree;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetDirectoryTree.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetDirectoryTreeQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoryTreeQueryFixture
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a random valid query to get directory tree.
    /// </summary>
    /// <param name="includeFiles">Whether to include file system files or not.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns>The created query to get directory tree.</returns>
    public static GetDirectoryTreeQuery CreateGetDirectoryTreeQuery(bool includeFiles = false, bool includeHiddenElements = false)
    {
        return new Faker<GetDirectoryTreeQuery>()
            .CustomInstantiator(f => new GetDirectoryTreeQuery(
                default!,
                default,
                default
            ))
            .RuleFor(x => x.Path, f => f.System.DirectoryPath())
            .RuleFor(x => x.IncludeFiles, f => includeFiles)
            .RuleFor(x => x.IncludeHiddenElements, f => includeHiddenElements);
    }
    #endregion
}
