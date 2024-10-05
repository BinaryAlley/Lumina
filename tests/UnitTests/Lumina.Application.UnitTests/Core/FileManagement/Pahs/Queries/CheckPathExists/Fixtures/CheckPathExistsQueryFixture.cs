#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileManagement.Paths.Queries.CheckPathExists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.CheckPathExists.Fixtures;

/// <summary>
/// Fixture class for the <see cref="CheckPathExistsQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsQueryFixture
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a random valid query to check if paths exist.
    /// </summary>
    /// <returns>The created query.</returns>
    public static CheckPathExistsQuery CreateCheckPathExistsQuery()
    {
        return new Faker<CheckPathExistsQuery>()
            .CustomInstantiator(f => new CheckPathExistsQuery(
                default!
            ))
            .RuleFor(x => x.Path, f => f.System.FilePath());
    }
    #endregion
}
