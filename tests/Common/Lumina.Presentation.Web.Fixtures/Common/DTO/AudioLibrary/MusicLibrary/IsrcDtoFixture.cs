#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="IsrcDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsrcDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="IsrcDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="value">Optional. The value of the ISRC.</param>
    /// <param name="includeValue">Whether the value should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="IsrcDto"/> instance.</returns>
    public IsrcDto Create(
        string? value = null,
        bool includeValue = true)
    {
        return new IsrcDto
        {
            Value = includeValue ? (value ?? $"US{_faker.Random.String2(3, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")}{_faker.Random.Number(10000, 99999)}") : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="IsrcDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="IsrcDto"/> instances.</returns>
    public List<IsrcDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
