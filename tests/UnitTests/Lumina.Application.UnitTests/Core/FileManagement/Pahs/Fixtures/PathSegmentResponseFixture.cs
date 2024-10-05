#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.FileManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Pahs.Fixtures;

/// <summary>
/// Fixture class for the <see cref="PathSegmentResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSegmentResponseFixture
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly Faker _faker = new();
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a random valid <see cref="PathSegmentResponse"/>.
    /// </summary>
    /// <returns>The created <see cref="PathSegmentResponse"/>.</returns>
    public PathSegmentResponse Create()
    {
        return new PathSegmentResponse(_faker.System.DirectoryPath());
    }

    /// <summary>
    /// Creates a list of <see cref="PathSegmentResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PathSegmentResponse> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count).Select(_ => Create()).ToList();
    }
    #endregion
}
