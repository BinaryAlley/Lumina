#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.FileSystem;

/// <summary>
/// Fixture class for the <see cref="FileSystemTypeResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemTypeResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="FileSystemTypeResponse"/>.
    /// </summary>
    /// <param name="platformType">Optional. The type of the file system platform.</param>
    /// <returns>The created <see cref="FileSystemTypeResponse"/>.</returns>
    public FileSystemTypeResponse Create(
        PlatformType? platformType = null)
    {
        return new FileSystemTypeResponse(platformType ?? _faker.PickRandom<PlatformType>());
    }

    /// <summary>
    /// Creates a list of <see cref="FileSystemTypeResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<FileSystemTypeResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
