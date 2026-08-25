#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;

/// <summary>
/// Fixture class for the <see cref="CheckInitializationQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckInitializationQueryFixture
{
    /// <summary>
    /// Creates a <see cref="CheckInitializationQuery"/>.
    /// </summary>
    /// <returns>The created <see cref="CheckInitializationQuery"/>.</returns>
    public CheckInitializationQuery Create()
    {
        return new CheckInitializationQuery();
    }

    /// <summary>
    /// Creates a list of <see cref="CheckInitializationQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<CheckInitializationQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
