#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.FileManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Fixtures;

/// <summary>
/// Fixture class for the <see cref="PathSeparatorResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSeparatorResponseFixture
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly Faker _faker = new();
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a random valid <see cref="PathSeparatorResponse"/>.
    /// </summary>
    /// <returns>The created <see cref="PathSeparatorResponse"/>.</returns>
    public PathSeparatorResponse Create()
    {
        return new PathSeparatorResponse(_faker.System.DirectoryPath());
    }

    /// <summary>
    /// Creates a list of <see cref="PathSeparatorResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PathSeparatorResponse> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count).Select(_ => Create()).ToList();
    }
    #endregion
}
