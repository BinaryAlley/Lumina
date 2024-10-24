#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;

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
    /// <param name="originalPath">The original path.</param>
    /// <param name="newPath">The new path to combine.</param>
    /// <returns>The created <see cref="CombinePathRequest"/>.</returns>
    public CombinePathRequest Create(string? originalPath = null, string? newPath = null)
    {
        return new CombinePathRequest(
            OriginalPath: originalPath ?? _faker.System.DirectoryPath(),
            NewPath: newPath ?? _faker.System.FileName()
        );
    }
}
