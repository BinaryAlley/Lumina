#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;
#endregion

namespace Lumina.Application.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Extension methods for converting <see cref="UserSettingsEntity"/>.
/// </summary>
public static class UserSettingsEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="UserSettings"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="UserSettings"/>, or an error message.
    /// </returns>
    public static Result<UserSettings> ToDomainEntity(this UserSettingsEntity repositoryEntity)
    {
        return UserSettings.Create(
            UserSettingsId.Create(repositoryEntity.Id),
            UserId.Create(repositoryEntity.UserId),
            repositoryEntity.IsPaginationEnabled,
            repositoryEntity.ItemsPerPage,
            repositoryEntity.IgnoreThePrefixForAlphaPicker,
            repositoryEntity.IsThemeCachingEnabled);
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="UserSettingsResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static UserSettingsResponse ToResponse(this UserSettingsEntity repositoryEntity)
    {
        return new UserSettingsResponse(
            repositoryEntity.UserId,
            repositoryEntity.IsPaginationEnabled,
            repositoryEntity.ItemsPerPage,
            repositoryEntity.IgnoreThePrefixForAlphaPicker,
            repositoryEntity.IsThemeCachingEnabled);
    }
}
