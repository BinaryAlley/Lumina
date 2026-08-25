#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;

/// <summary>
/// Fixture class for generating <see cref="PathValidDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathValidDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PathValidDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="isValid">Optional value indicating whether a file system path is valid or not.</param>
    /// <returns>A configured <see cref="PathValidDto"/> instance.</returns>
    public PathValidDto Create(
        bool? isValid = null)
    {
        return new PathValidDto(
            IsValid: isValid ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PathValidDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PathValidDto"/> instances.</returns>
    public List<PathValidDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
