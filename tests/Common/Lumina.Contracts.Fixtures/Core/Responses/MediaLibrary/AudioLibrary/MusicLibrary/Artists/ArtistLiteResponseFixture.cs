#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="ArtistLiteResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistLiteResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ArtistLiteResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the artist.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <returns>The created <see cref="ArtistLiteResponse"/>.</returns>
    public ArtistLiteResponse Create(
        Guid? id = null,
        string? name = null)
    {
        return new ArtistLiteResponse(
            id ?? Guid.NewGuid(),
            name ?? _faker.Name.FullName());
    }

    /// <summary>
    /// Creates a list of <see cref="ArtistLiteResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ArtistLiteResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
