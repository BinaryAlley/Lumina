#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="LibraryConsumptionSettings"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryConsumptionSettingsFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a valid <see cref="LibraryConsumptionSettings"/>.
    /// </summary>
    /// <param name="shouldTrackConsumptionProgress">Optional. Whether the consumption progress is tracked. A random value is used when absent.</param>
    /// <param name="completionThresholdPercentage">Optional. The completion threshold percentage. A random value is used when absent and the threshold is included.</param>
    /// <param name="includeCompletionThreshold">Whether the completion threshold should be included. When <see langword="false"/>, no threshold is set.</param>
    /// <returns>The created consumption settings.</returns>
    public LibraryConsumptionSettings Create(
        bool? shouldTrackConsumptionProgress = null,
        int? completionThresholdPercentage = null,
        bool includeCompletionThreshold = true)
    {
        bool resolvedShouldTrackConsumptionProgress = shouldTrackConsumptionProgress ?? _faker.Random.Bool();
        Optional<int> resolvedCompletionThresholdPercentage = includeCompletionThreshold
            ? Optional<int>.FromNullable(completionThresholdPercentage ?? _faker.Random.Int(0, 100))
            : Optional<int>.None();
        return LibraryConsumptionSettings.Create(resolvedShouldTrackConsumptionProgress, resolvedCompletionThresholdPercentage);
    }

    /// <summary>
    /// Creates the default <see cref="LibraryConsumptionSettings"/> for <paramref name="libraryType"/>.
    /// </summary>
    /// <param name="libraryType">The type of the media library for which to create the default consumption settings.</param>
    /// <returns>The default consumption settings of the media library type.</returns>
    public LibraryConsumptionSettings Create(LibraryType libraryType)
    {
        return LibraryConsumptionSettings.CreateDefault(libraryType);
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryConsumptionSettings"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryConsumptionSettings"/> instances.</returns>
    public List<LibraryConsumptionSettings> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
