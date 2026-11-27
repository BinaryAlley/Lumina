#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="OpenLibraryAuthorResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenLibraryAuthorResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="OpenLibraryAuthorResponse"/>.
    /// </summary>
    /// <param name="key">Optional. The key of the author.</param>
    /// <param name="name">Optional. The name of the author.</param>
    /// <param name="personalName">Optional. The personal name of the author.</param>
    /// <returns>The created author response.</returns>
    public OpenLibraryAuthorResponse Create(
        string? key = null,
        string? name = null,
        string? personalName = null)
    {
        return new OpenLibraryAuthorResponse
        {
            Key = key ?? $"/authors/OL{_faker.Random.Number(1000, 9999)}A",
            Name = name ?? _faker.Name.FullName(),
            PersonalName = personalName
        };
    }
}
