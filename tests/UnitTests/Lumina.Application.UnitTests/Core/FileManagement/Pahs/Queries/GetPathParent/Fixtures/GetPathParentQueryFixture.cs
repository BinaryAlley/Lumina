#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileManagement.Paths.Queries.GetPathParent;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.GetPathParent.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetPathParentQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentQueryFixture
{
    /// <summary>
    /// Creates a random valid query to check if paths exist.
    /// </summary>
    /// <returns>The created query.</returns>
    public static GetPathParentQuery CreateGetPathParentQuery()
    {
        return new Faker<GetPathParentQuery>()
            .CustomInstantiator(f => new GetPathParentQuery(
                default!
            ))
            .RuleFor(x => x.Path, f => f.System.FilePath());
    }
}
