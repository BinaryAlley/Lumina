#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="OpenLibraryKeyReferenceResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenLibraryKeyReferenceResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="OpenLibraryKeyReferenceResponse"/>.
    /// </summary>
    /// <param name="key">Optional. The key of the referenced resource.</param>
    /// <returns>The created reference response.</returns>
    public OpenLibraryKeyReferenceResponse Create(string? key = null)
    {
        return new OpenLibraryKeyReferenceResponse
        {
            Key = key ?? $"/works/OL{_faker.Random.Number(1000, 9999)}W"
        };
    }
}
