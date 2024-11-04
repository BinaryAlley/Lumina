#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.FileSystemManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;

/// <summary>
/// Fixture class for the <see cref="FileSystemItemDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemModelFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="FileSystemItemDto"/>.
    /// </summary>
    /// <returns>The created <see cref="FileSystemItemDto"/>.</returns>
    public FileSystemItemDto Create()
    {
        return new FileSystemItemDto(
            _faker.System.FilePath(),
            _faker.System.FileName(),
            _faker.Date.Past(),
            _faker.Date.Recent()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="FileSystemItemDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<FileSystemItemDto> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count).Select(_ => Create()).ToList();
    }
}
