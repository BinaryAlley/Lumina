#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;

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
    /// <param name="path">The path to split.</param>
    /// <returns>The created <see cref="SplitPathRequest"/>.</returns>
    public SplitPathRequest Create(string? path = null)
    {
        return new SplitPathRequest(
            Path: path ?? _faker.System.FilePath()
        );
    }
}
