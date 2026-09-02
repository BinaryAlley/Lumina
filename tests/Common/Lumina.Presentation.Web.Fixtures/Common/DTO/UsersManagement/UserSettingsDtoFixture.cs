#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;

/// <summary>
/// Fixture class for generating <see cref="UserSettingsDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="UserSettingsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional identifier of the user that owns the settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user.</param>
    /// <param name="itemsPerPage">Number of library items displayed per page.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the "The" prefix is ignored by the alpha picker.</param>
    /// <param name="isThemeCachingEnabled">Whether the theme data served to this user is cached.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing.</param>
    /// <param name="shouldRenderPdfAsImages">Whether PDF books are rendered as page images for the user.</param>
    /// <param name="shouldPreserveBookStyles">Whether the styles of the book content are preserved when it is rendered for the user.</param>
    /// <returns>A configured <see cref="UserSettingsDto"/> instance.</returns>
    public UserSettingsDto Create(
        Guid? userId = null, 
        bool? isPaginationEnabled = null, 
        int? itemsPerPage = null, 
        bool? shouldIgnoreThePrefixForAlphaPicker = null, 
        bool? isThemeCachingEnabled = null, 
        bool? shouldAggregateMetadataWhenMissing = null,
        bool? shouldRenderPdfAsImages = null,
        bool? shouldPreserveBookStyles = null)
    {
        return new UserSettingsDto
        {
            UserId = userId ?? Guid.NewGuid(),
            IsPaginationEnabled = isPaginationEnabled ?? _faker.Random.Bool(),
            ItemsPerPage = itemsPerPage ?? _faker.Random.Int(1, 200),
            ShouldIgnoreThePrefixForAlphaPicker = shouldIgnoreThePrefixForAlphaPicker ?? _faker.Random.Bool(),
            IsThemeCachingEnabled = isThemeCachingEnabled ?? _faker.Random.Bool(),
            ShouldAggregateMetadataWhenMissing = shouldAggregateMetadataWhenMissing ?? _faker.Random.Bool(),
            ShouldRenderPdfAsImages = shouldRenderPdfAsImages ?? _faker.Random.Bool(),
            ShouldPreserveBookStyles = shouldPreserveBookStyles ?? _faker.Random.Bool()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="UserSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UserSettingsDto"/> instances.</returns>
    public List<UserSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
