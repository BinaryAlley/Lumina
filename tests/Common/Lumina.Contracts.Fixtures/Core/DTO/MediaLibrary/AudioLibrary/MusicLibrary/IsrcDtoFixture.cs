#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="IsrcDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsrcDtoFixture
{
    private const string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string ALPHANUMERICS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string DIGITS = "0123456789";

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="IsrcDto"/>.
    /// </summary>
    /// <param name="value">Optional. The value of the ISRC.</param>
    /// <param name="includeValue">Whether the value should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="IsrcDto"/>.</returns>
    public IsrcDto Create(
        string? value = null,
        bool includeValue = true)
    {
        return new IsrcDto(includeValue ? value ?? GenerateValidIsrc() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="IsrcDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<IsrcDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Generates a random ISRC that satisfies the domain format: two letters, three alphanumeric characters and seven digits.
    /// </summary>
    /// <returns>The generated ISRC value.</returns>
    private string GenerateValidIsrc()
    {
        return _faker.Random.String2(2, LETTERS)
             + _faker.Random.String2(3, ALPHANUMERICS)
             + _faker.Random.String2(7, DIGITS);
    }
}
