#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;

/// <summary>
/// Fixture class for the <see cref="CombinePathRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="CombinePathRequest"/> with default or random values.
    /// </summary>
    /// <param name="originalPath">Optional. The original path.</param>
    /// <param name="newPath">Optional. The new path to combine.</param>
    /// <returns>The created <see cref="CombinePathRequest"/>.</returns>
    public CombinePathRequest Create(string? originalPath = null, string? newPath = null)
    {
        return new CombinePathRequest(
            OriginalPath: originalPath ?? _faker.System.DirectoryPath(),
            NewPath: newPath ?? _faker.System.FileName()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="CombinePathRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<CombinePathRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
