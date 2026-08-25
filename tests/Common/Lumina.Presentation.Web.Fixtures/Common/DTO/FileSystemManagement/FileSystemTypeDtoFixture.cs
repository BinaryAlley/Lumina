#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Enums.FileSystem;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;

/// <summary>
/// Fixture class for generating <see cref="FileSystemTypeDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemTypeDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="FileSystemTypeDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="platformType">Optional file system platform type.</param>
    /// <returns>A configured <see cref="FileSystemTypeDto"/> instance.</returns>
    public FileSystemTypeDto Create(
        PlatformType? platformType = null)
    {
        return new FileSystemTypeDto
        {
            PlatformType = platformType ?? _faker.PickRandom<PlatformType>()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="FileSystemTypeDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FileSystemTypeDto"/> instances.</returns>
    public List<FileSystemTypeDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
