#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="OpenLibraryWorkAuthorResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenLibraryWorkAuthorResponseFixture
{
    private readonly Faker _faker = new();
    private readonly OpenLibraryKeyReferenceResponseFixture _keyReferenceResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="OpenLibraryWorkAuthorResponse"/>.
    /// </summary>
    /// <param name="author">Optional. The author reference of the work.</param>
    /// <param name="key">Optional. The direct key of the author reference.</param>
    /// <returns>The created work author response.</returns>
    public OpenLibraryWorkAuthorResponse Create(
        OpenLibraryKeyReferenceResponse? author = null,
        string? key = null)
    {
        return new OpenLibraryWorkAuthorResponse
        {
            Author = author ?? _keyReferenceResponseFixture.Create(key: $"/authors/OL{_faker.Random.Number(1000, 9999)}A"),
            Key = key
        };
    }
}
