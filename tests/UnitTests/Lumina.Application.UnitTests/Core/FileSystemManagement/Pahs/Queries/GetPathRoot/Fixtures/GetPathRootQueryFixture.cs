#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Pahs.Queries.GetPathRoot.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetPathRootQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootQueryFixture
{
    /// <summary>
    /// Creates a random valid query to check if paths exist.
    /// </summary>
    /// <returns>The created query.</returns>
    public static GetPathRootQuery CreateGetPathRootQuery()
    {
        return new Faker<GetPathRootQuery>()
            .CustomInstantiator(f => new GetPathRootQuery(
                default!
            ))
            .RuleFor(x => x.Path, f => f.System.FilePath());
    }
}
