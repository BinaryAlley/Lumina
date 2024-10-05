#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.FileManagement.Paths.Queries.ValidatePath;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Queries.ValidatePath.Fixtures;

/// <summary>
/// Fixture class for the <see cref="ValidatePathQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathQueryFixure
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a random valid query to check if paths exist.
    /// </summary>
    /// <returns>The created query.</returns>
    public static ValidatePathQuery CreateValidatePathQuery()
    {
        return new Faker<ValidatePathQuery>()
            .CustomInstantiator(f => new ValidatePathQuery(
                default!
            ))
            .RuleFor(x => x.Path, f => f.System.FilePath());
    }
    #endregion
}
