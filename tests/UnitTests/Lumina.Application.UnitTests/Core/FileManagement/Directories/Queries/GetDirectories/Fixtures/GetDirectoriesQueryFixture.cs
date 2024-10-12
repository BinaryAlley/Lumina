#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectories;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Directories.Queries.GetDirectories.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetDirectoriesQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get directories.
    /// </summary>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns>The created query to get directories.</returns>
    public static GetDirectoriesQuery CreateGetDirectoriesQuery(bool includeHiddenElements = false)
    {
        return new Faker<GetDirectoriesQuery>()
            .CustomInstantiator(f => new GetDirectoriesQuery(
                default!,
                default
            ))
            .RuleFor(x => x.Path, f => f.System.DirectoryPath())
            .RuleFor(x => x.IncludeHiddenElements, f => includeHiddenElements);
    }
}
