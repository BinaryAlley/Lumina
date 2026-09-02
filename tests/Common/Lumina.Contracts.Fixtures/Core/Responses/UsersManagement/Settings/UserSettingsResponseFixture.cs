#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.UsersManagement.Settings;

/// <summary>
/// Fixture class for the <see cref="UserSettingsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UserSettingsResponse"/>.
    /// </summary>
    /// <param name="userId">Optional. The Id of the user that owns these settings.</param>
    /// <param name="isPaginationEnabled">Optional. Whether pagination is enabled for the user.</param>
    /// <param name="itemsPerPage">Optional. The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Optional. Whether the "The" prefix is ignored by the alpha picker.</param>
    /// <param name="isThemeCachingEnabled">Optional. Whether the theme data served to this user is cached.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Optional. Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing.</param>
    /// <param name="shouldRenderPdfAsImages">Optional. Whether PDF books are rendered as page images for the user.</param>
    /// <param name="shouldPreserveBookStyles">Optional. Whether the styles of the book content are preserved when it is rendered for the user.</param>
    /// <returns>The created <see cref="UserSettingsResponse"/>.</returns>
    public UserSettingsResponse Create(
        Guid? userId = null,
        bool? isPaginationEnabled = null,
        int? itemsPerPage = null,
        bool? shouldIgnoreThePrefixForAlphaPicker = null,
        bool? isThemeCachingEnabled = null,
        bool? shouldAggregateMetadataWhenMissing = null,
        bool? shouldRenderPdfAsImages = null,
        bool? shouldPreserveBookStyles = null)
    {
        return new UserSettingsResponse(
            userId ?? Guid.NewGuid(),
            isPaginationEnabled ?? _faker.Random.Bool(),
            itemsPerPage ?? _faker.Random.Int(1, 100),
            shouldIgnoreThePrefixForAlphaPicker ?? _faker.Random.Bool(),
            isThemeCachingEnabled ?? _faker.Random.Bool(),
            shouldAggregateMetadataWhenMissing ?? _faker.Random.Bool(),
            shouldRenderPdfAsImages ?? _faker.Random.Bool(),
            shouldPreserveBookStyles ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="UserSettingsResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UserSettingsResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
