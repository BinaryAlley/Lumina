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
    /// <summary>
    /// Creates a new <see cref="UserSettingsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional identifier of the user that owns the settings.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled for the user.</param>
    /// <param name="itemsPerPage">Number of library items displayed per page.</param>
    /// <param name="ignoreThePrefixForAlphaPicker">Whether the "The" prefix is ignored by the alpha picker.</param>
    /// <returns>A configured <see cref="UserSettingsDto"/> instance.</returns>
    public UserSettingsDto Create(Guid? userId = null, bool? isPaginationEnabled = null, int? itemsPerPage = null, bool? ignoreThePrefixForAlphaPicker = null)
    {
        Faker faker = new();
        return new UserSettingsDto
        {
            UserId = userId ?? Guid.NewGuid(),
            IsPaginationEnabled = isPaginationEnabled ?? faker.Random.Bool(),
            ItemsPerPage = itemsPerPage ?? faker.Random.Int(1, 200),
            IgnoreThePrefixForAlphaPicker = ignoreThePrefixForAlphaPicker ?? faker.Random.Bool()
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
