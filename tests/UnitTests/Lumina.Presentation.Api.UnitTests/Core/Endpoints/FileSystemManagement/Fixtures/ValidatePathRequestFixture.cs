#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;

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
    /// <param name="path">The path to validate.</param>
    /// <returns>The created <see cref="ValidatePathRequest"/>.</returns>
    public ValidatePathRequest Create(string? path = null)
    {
        return new ValidatePathRequest(
            Path: path ?? _faker.System.FilePath()
        );
    }
}
