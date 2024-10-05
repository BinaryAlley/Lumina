#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileManagement.Files.Queries.GetFiles;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Files.Queries.GetFiles.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetFilesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesQueryFixture
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a random valid query to get files.
    /// </summary>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns>The created query to get files.</returns>
    public static GetFilesQuery CreateGetFilesQuery(bool includeHiddenElements = false)
    {
        return new Faker<GetFilesQuery>()
            .CustomInstantiator(f => new GetFilesQuery(
                default!,
                default
            ))
            .RuleFor(x => x.Path, f => f.System.FilePath())
            .RuleFor(x => x.IncludeHiddenElements, f => includeHiddenElements);
    }
    #endregion
}
