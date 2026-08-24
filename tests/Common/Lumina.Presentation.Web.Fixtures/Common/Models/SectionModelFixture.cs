#region ========================================================================= USING =====================================================================================
using Bogus;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Models;

/// <summary>
/// Fixture class for generating <see cref="SectionModel"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class SectionModelFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="SectionModel"/> instance with randomized test data.
    /// </summary>
    /// <param name="text">Optional text rendered by the section.</param>
    /// <returns>A configured <see cref="SectionModel"/> instance.</returns>
    public SectionModel Create(
        string? text = null)
    {
        return new SectionModel { Text = text ?? _faker.Lorem.Word() };
    }

    /// <summary>
    /// Creates multiple <see cref="SectionModel"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="SectionModel"/> instances.</returns>
    public List<SectionModel> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
