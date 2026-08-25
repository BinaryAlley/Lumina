#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;

/// <summary>
/// Fixture class for generating <see cref="PathExistsDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathExistsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PathExistsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="exists">Optional value indicating whether a file system path exists or not.</param>
    /// <returns>A configured <see cref="PathExistsDto"/> instance.</returns>
    public PathExistsDto Create(
        bool? exists = null)
    {
        return new PathExistsDto(
            Exists: exists ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PathExistsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PathExistsDto"/> instances.</returns>
    public List<PathExistsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
