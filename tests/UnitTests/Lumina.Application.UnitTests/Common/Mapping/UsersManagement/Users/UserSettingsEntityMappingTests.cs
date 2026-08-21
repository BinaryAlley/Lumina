#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Contains unit tests for the <see cref="UserSettingsEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsEntityMappingTests
{
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingValidRepositoryEntity_ShouldMapCorrectly()
    {
        // Arrange
        UserSettingsEntity entity = _userSettingsEntityFixture.Create();

        // Act
        Result<UserSettings> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(entity.Id, result.Value.Id.Value);
        Assert.Equal(entity.UserId, result.Value.UserId.Value);
        Assert.Equal(entity.IsPaginationEnabled, result.Value.IsPaginationEnabled);
        Assert.Equal(entity.ItemsPerPage, result.Value.ItemsPerPage);
        Assert.Equal(entity.IgnoreThePrefixForAlphaPicker, result.Value.IgnoreThePrefixForAlphaPicker);
        Assert.Equal(entity.IsThemeCachingEnabled, result.Value.IsThemeCachingEnabled);
    }

    [Fact]
    public void ToResponse_WhenMappingValidRepositoryEntity_ShouldMapCorrectly()
    {
        // Arrange
        UserSettingsEntity entity = _userSettingsEntityFixture.Create();

        // Act
        UserSettingsResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.UserId, result.UserId);
        Assert.Equal(entity.IsPaginationEnabled, result.IsPaginationEnabled);
        Assert.Equal(entity.ItemsPerPage, result.ItemsPerPage);
        Assert.Equal(entity.IgnoreThePrefixForAlphaPicker, result.IgnoreThePrefixForAlphaPicker);
        Assert.Equal(entity.IsThemeCachingEnabled, result.IsThemeCachingEnabled);
    }
}
