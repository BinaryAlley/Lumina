#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Common;

/// <summary>
/// Fixture class for the <see cref="MoodDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MoodDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MoodDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional. The name of the mood.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MoodDto"/> instance.</returns>
    public MoodDto Create(
        string? name = null,
        bool includeName = true)
    {
        return new MoodDto
        {
            Name = includeName ? (name ?? _faker.Lorem.Word()) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MoodDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MoodDto"/> instances.</returns>
    public List<MoodDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
