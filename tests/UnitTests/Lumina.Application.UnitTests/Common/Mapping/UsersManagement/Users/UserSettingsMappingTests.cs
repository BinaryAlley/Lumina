#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.UsersManagement.Users;

/// <summary>
/// Contains unit tests for the <see cref="UserSettingsMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsMappingTests
{
    private readonly UserSettingsFixture _userSettingsFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingValidDomainEntity_ShouldMapCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserSettings domainEntity = _userSettingsFixture.Create(userId: userId);

        // Act
        UserSettingsEntity result = domainEntity.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainEntity.Id.Value, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(domainEntity.IsPaginationEnabled, result.IsPaginationEnabled);
        Assert.Equal(domainEntity.ItemsPerPage, result.ItemsPerPage);
        Assert.Equal(domainEntity.IgnoreThePrefixForAlphaPicker, result.IgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public void ToResponse_WhenMappingValidDomainEntity_ShouldMapCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserSettings domainEntity = _userSettingsFixture.Create(userId: userId);

        // Act
        UserSettingsResponse result = domainEntity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(domainEntity.IsPaginationEnabled, result.IsPaginationEnabled);
        Assert.Equal(domainEntity.ItemsPerPage, result.ItemsPerPage);
        Assert.Equal(domainEntity.IgnoreThePrefixForAlphaPicker, result.IgnoreThePrefixForAlphaPicker);
    }
}
