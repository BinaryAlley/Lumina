#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.Repositories.Users;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Users;

/// <summary>
/// Contains unit tests for the <see cref="UserSettingsRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly UserSettingsRepository _sut;
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettingsRepositoryTests"/> class.
    /// </summary>
    public UserSettingsRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new UserSettingsRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenSettingsDoNotExist_ShouldAddSettingsToContextAndReturnCreated()
    {
        // Arrange
        UserSettingsEntity settings = _userSettingsEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(settings, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<UserSettingsEntity>? addedSettings = _mockContext.ChangeTracker.Entries<UserSettingsEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == settings.Id);
        Assert.NotNull(addedSettings);
        Assert.Equal(settings.UserId, addedSettings.Entity.UserId);
    }

    [Fact]
    public async Task InsertAsync_WhenSettingsForUserAlreadyExist_ShouldReturnError()
    {
        // Arrange
        UserSettingsEntity settings = _userSettingsEntityFixture.Create();
        _mockContext.UserSettings.Add(settings);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Created> result = await _sut.InsertAsync(settings, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.UserSettings.UserSettingsAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<UserSettingsEntity>());
    }

    [Fact]
    public async Task GetByIdAsync_WhenSettingsExist_ShouldReturnSettings()
    {
        // Arrange
        UserSettingsEntity settings = _userSettingsEntityFixture.Create();
        _mockContext.UserSettings.Add(settings);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<UserSettingsEntity?> result = await _sut.GetByIdAsync(settings.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(settings.Id, result.Value.Id);
        Assert.Equal(settings.UserId, result.Value.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSettingsDoNotExist_ShouldReturnNull()
    {
        // Act
        Result<UserSettingsEntity?> result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenSettingsExist_ShouldReturnSettings()
    {
        // Arrange
        UserSettingsEntity settings = _userSettingsEntityFixture.Create();
        _mockContext.UserSettings.Add(settings);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<UserSettingsEntity?> result = await _sut.GetByUserIdAsync(settings.UserId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(settings.Id, result.Value.Id);
        Assert.Equal(settings.UserId, result.Value.UserId);
        Assert.Equal(settings.IsPaginationEnabled, result.Value.IsPaginationEnabled);
        Assert.Equal(settings.ItemsPerPage, result.Value.ItemsPerPage);
        Assert.Equal(settings.ShouldIgnoreThePrefixForAlphaPicker, result.Value.ShouldIgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenSettingsDoNotExist_ShouldReturnNull()
    {
        // Act
        Result<UserSettingsEntity?> result = await _sut.GetByUserIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenSettingsExist_ShouldUpdateSettingsAndReturnUpdated()
    {
        // Arrange
        UserSettingsEntity existingSettings = _userSettingsEntityFixture.Create();
        _mockContext.UserSettings.Add(existingSettings);
        await _mockContext.SaveChangesAsync();

        UserSettingsEntity updatedSettings = _userSettingsEntityFixture.Create(
            id: existingSettings.Id,
            userId: existingSettings.UserId,
            isPaginationEnabled: false,
            itemsPerPage: 12,
            shouldIgnoreThePrefixForAlphaPicker: true);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedSettings, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        UserSettingsEntity? modifiedSettings = await _mockContext.UserSettings.FirstOrDefaultAsync(settings => settings.Id == existingSettings.Id);
        Assert.NotNull(modifiedSettings);
        Assert.Equal(existingSettings.UserId, modifiedSettings.UserId);
        Assert.False(modifiedSettings.IsPaginationEnabled);
        Assert.Equal(12, modifiedSettings.ItemsPerPage);
        Assert.True(modifiedSettings.ShouldIgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public async Task UpdateAsync_WhenSettingsDoNotExist_ShouldReturnError()
    {
        // Arrange
        UserSettingsEntity settings = _userSettingsEntityFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(settings, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.UserSettings.UserSettingsNotFound, result.FirstError);
    }
}
