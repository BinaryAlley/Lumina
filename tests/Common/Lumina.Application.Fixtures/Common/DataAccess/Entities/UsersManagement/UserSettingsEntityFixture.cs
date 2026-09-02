#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;

/// <summary>
/// Fixture class for the <see cref="UserSettingsEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UserSettingsEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the user settings.</param>
    /// <param name="userId">Optional. The Id of the user that owns these settings.</param>
    /// <param name="isPaginationEnabled">Optional. Whether pagination is enabled for the user, or not.</param>
    /// <param name="itemsPerPage">Optional. The number of library items displayed per page when pagination is enabled.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Optional. Whether the "The" prefix is ignored by the alpha picker, or not.</param>
    /// <param name="isThemeCachingEnabled">Optional. Whether the theme data served to this user is cached, or not.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Optional. Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.</param>
    /// <param name="shouldRenderPdfAsImages">Optional. Whether PDF books are rendered as page images for the user, or not.</param>
    /// <param name="shouldPreserveBookStyles">Optional. Whether the styles of the book content are preserved when it is rendered for the user, or not.</param>
    /// <returns>The created user settings entity.</returns>
    public UserSettingsEntity Create(
        Guid? id = null,
        Guid? userId = null,
        bool? isPaginationEnabled = null,
        int? itemsPerPage = null,
        bool? shouldIgnoreThePrefixForAlphaPicker = null,
        bool? isThemeCachingEnabled = null,
        bool? shouldAggregateMetadataWhenMissing = null,
        bool? shouldRenderPdfAsImages = null,
        bool? shouldPreserveBookStyles = null)
    {
        return new Faker<UserSettingsEntity>()
            .CustomInstantiator(f => new UserSettingsEntity
            {
                Id = id ?? Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                IsPaginationEnabled = isPaginationEnabled ?? f.Random.Bool(),
                ItemsPerPage = itemsPerPage ?? f.Random.Int(1, 100),
                ShouldIgnoreThePrefixForAlphaPicker = shouldIgnoreThePrefixForAlphaPicker ?? f.Random.Bool(),
                IsThemeCachingEnabled = isThemeCachingEnabled ?? f.Random.Bool(),
                ShouldAggregateMetadataWhenMissing = shouldAggregateMetadataWhenMissing ?? f.Random.Bool(),
                ShouldRenderPdfAsImages = shouldRenderPdfAsImages ?? f.Random.Bool(),
                ShouldPreserveBookStyles = shouldPreserveBookStyles ?? f.Random.Bool()
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UserSettingsEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UserSettingsEntity"/> instances.</returns>
    public List<UserSettingsEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
