#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;

/// <summary>
/// Fixture class for the <see cref="UpdateUserSettingsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdateUserSettingsRequest"/>.
    /// </summary>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user.</param>
    /// <param name="itemsPerPage">The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix is ignored by the alpha picker.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing.</param>
    /// <returns>The created <see cref="UpdateUserSettingsRequest"/>.</returns>
    public UpdateUserSettingsRequest Create(
        bool? isPaginationEnabled = null,
        int? itemsPerPage = null,
        bool? shouldIgnoreThePrefixForAlphaPicker = null,
        bool? isThemeCachingEnabled = null,
        bool? shouldAggregateMetadataWhenMissing = null)
    {
        return new UpdateUserSettingsRequest(
            isPaginationEnabled ?? _faker.Random.Bool(),
            itemsPerPage ?? _faker.Random.Int(1, 100),
            shouldIgnoreThePrefixForAlphaPicker ?? _faker.Random.Bool(),
            isThemeCachingEnabled ?? _faker.Random.Bool(),
            shouldAggregateMetadataWhenMissing ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateUserSettingsRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateUserSettingsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
