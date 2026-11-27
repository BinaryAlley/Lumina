#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;

/// <summary>
/// Fixture class for the <see cref="ValidatePathRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="ValidatePathRequest"/> with a default or random value.
    /// </summary>
    /// <param name="path">Optional. The path to validate.</param>
    /// <returns>The created <see cref="ValidatePathRequest"/>.</returns>
    public ValidatePathRequest Create(string? path = null)
    {
        return new ValidatePathRequest(
            Path: path ?? _faker.System.FilePath()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="ValidatePathRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ValidatePathRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
