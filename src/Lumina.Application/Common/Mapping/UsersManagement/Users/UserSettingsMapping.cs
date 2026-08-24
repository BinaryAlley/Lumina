#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
#endregion

namespace Lumina.Application.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Extension methods for converting <see cref="UserSettings"/>.
/// </summary>
public static class UserSettingsMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="UserSettingsEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static UserSettingsEntity ToRepositoryEntity(this UserSettings domainEntity)
    {
        return new UserSettingsEntity
        {
            Id = domainEntity.Id.Value,
            UserId = domainEntity.UserId.Value,
            IsPaginationEnabled = domainEntity.IsPaginationEnabled,
            ItemsPerPage = domainEntity.ItemsPerPage,
            ShouldIgnoreThePrefixForAlphaPicker = domainEntity.ShouldIgnoreThePrefixForAlphaPicker,
            IsThemeCachingEnabled = domainEntity.IsThemeCachingEnabled,
            ShouldAggregateMetadataWhenMissing = domainEntity.ShouldAggregateMetadataWhenMissing
        };
    }

    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="UserSettingsResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static UserSettingsResponse ToResponse(this UserSettings domainEntity)
    {
        return new UserSettingsResponse(
            domainEntity.UserId.Value,
            domainEntity.IsPaginationEnabled,
            domainEntity.ItemsPerPage,
            domainEntity.ShouldIgnoreThePrefixForAlphaPicker,
            domainEntity.IsThemeCachingEnabled,
            domainEntity.ShouldAggregateMetadataWhenMissing
        );
    }
}
