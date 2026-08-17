#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Tools;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Tools;

/// <summary>
/// Fixture class for generating <see cref="SetLanguageRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLanguageRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="SetLanguageRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="newCulture">Optional new culture to set.</param>
    /// <param name="returnUrl">Optional URL to return to, after setting the new culture.</param>
    /// <returns>A configured <see cref="SetLanguageRequest"/> instance.</returns>
    public SetLanguageRequest Create(string? newCulture = "en-US", string? returnUrl = null)
    {
        return new SetLanguageRequest(
            NewCulture: newCulture,
            ReturnUrl: returnUrl
        );
    }

    /// <summary>
    /// Creates multiple <see cref="SetLanguageRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="SetLanguageRequest"/> instances.</returns>
    public List<SetLanguageRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
