#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;

/// <summary>
/// Fixture class for the <see cref="SplitPathRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="SplitPathRequest"/> with a default or random value.
    /// </summary>
    /// <param name="path">Optional. The path to split.</param>
    /// <returns>The created <see cref="SplitPathRequest"/>.</returns>
    public SplitPathRequest Create(string? path = null)
    {
        return new SplitPathRequest(
            Path: path ?? _faker.System.FilePath()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="SplitPathRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<SplitPathRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
