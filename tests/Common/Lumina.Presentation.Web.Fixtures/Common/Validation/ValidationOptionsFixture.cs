#region ========================================================================= USING =====================================================================================
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Validation;

/// <summary>
/// Test-support placeholder options type used as the validated options in validation options tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidationOptionsFixture
{
    /// <summary>
    /// Creates a new <see cref="ValidationOptionsFixture"/> instance.
    /// </summary>
    /// <returns>A configured <see cref="ValidationOptionsFixture"/> instance.</returns>
    public ValidationOptionsFixture Create()
    {
        return new ValidationOptionsFixture();
    }
}
