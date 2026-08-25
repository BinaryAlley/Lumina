#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;

/// <summary>
/// Fixture class for generating <see cref="LibraryScanJobProgressDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanJobProgressDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="LibraryScanJobProgressDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="completedItems">Optional number of completed items of the media library scan job.</param>
    /// <param name="totalItems">Optional total number of items of the media library scan job.</param>
    /// <param name="currentOperation">Optional current operation being performed by the media library scan job.</param>
    /// <param name="progressPercentage">Optional completion ratio of the media library scan job, as a percentage.</param>
    /// <returns>A configured <see cref="LibraryScanJobProgressDto"/> instance.</returns>
    public LibraryScanJobProgressDto Create(
        int? completedItems = null, 
        int? totalItems = null, 
        string? currentOperation = null, 
        decimal? progressPercentage = null)
    {
        int completed = completedItems ?? _faker.Random.Int(0, 100);
        int total = totalItems ?? _faker.Random.Int(completed, 100);
        return new LibraryScanJobProgressDto
        {
            CompletedItems = completed,
            TotalItems = total,
            CurrentOperation = currentOperation ?? _faker.Hacker.Verb(),
            ProgressPercentage = progressPercentage ?? (total > 0 ? (decimal)completed / total * 100 : 0)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryScanJobProgressDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryScanJobProgressDto"/> instances.</returns>
    public List<LibraryScanJobProgressDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
