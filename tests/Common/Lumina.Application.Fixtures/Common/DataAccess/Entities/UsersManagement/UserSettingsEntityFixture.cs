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
    /// <param name="ignoreThePrefixForAlphaPicker">Optional. Whether the "The" prefix is ignored by the alpha picker, or not.</param>
    /// <returns>The created user settings entity.</returns>
    public UserSettingsEntity Create(
        Guid? id = null,
        Guid? userId = null,
        bool? isPaginationEnabled = null,
        int? itemsPerPage = null,
        bool? ignoreThePrefixForAlphaPicker = null)
    {
        return new Faker<UserSettingsEntity>()
            .CustomInstantiator(f => new UserSettingsEntity
            {
                Id = id ?? Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                IsPaginationEnabled = isPaginationEnabled ?? f.Random.Bool(),
                ItemsPerPage = itemsPerPage ?? f.Random.Int(1, 100),
                IgnoreThePrefixForAlphaPicker = ignoreThePrefixForAlphaPicker ?? f.Random.Bool()
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
