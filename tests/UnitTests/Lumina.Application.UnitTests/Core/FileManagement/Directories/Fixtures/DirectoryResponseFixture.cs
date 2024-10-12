#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.FileManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Directories.Fixtures;

/// <summary>
/// Fixture class for the <see cref="DirectoryResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryResponseFixture
{
    private readonly Faker _faker = new();
    private readonly FileSystemItemModelFixture _fileSystemItemModelFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="DirectoryResponse"/>.
    /// </summary>
    /// <param name="itemCount">The number of children elements to create.</param>
    /// <returns>The created <see cref="DirectoryResponse"/>.</returns>
    public DirectoryResponse Create(int itemCount = 3)
    {
        return new DirectoryResponse(
            _faker.System.FilePath(),
            _faker.System.FileName(),
            _faker.Date.Past(),
            _faker.Date.Recent(),
            _fileSystemItemModelFixture.CreateMany(itemCount)
        );
    }

    /// <summary>
    /// Creates a list of <see cref="DirectoryResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <param name="itemCount">The number of children elemens to create for each element.</param>
    /// <returns>The created list.</returns>
    public List<DirectoryResponse> CreateMany(int count = 3, int itemCount = 3)
    {
        return Enumerable.Range(0, count).Select(_ => Create(itemCount)).ToList();
    }
}
