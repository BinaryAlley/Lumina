#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.Repositories.Users;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Users;

/// <summary>
/// Contains integration tests for the <see cref="UserSettingsRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsRepositoryTests
{
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();

    [Fact]
    public async Task UpdateAsync_WhenAnEditableValueChanges_ShouldPreserveTheStoredAuditColumns()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-usersettingsrepo-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        UserSettingsRepository sut = new(context);

        UserEntity storedUser = _userEntityFixture.Create(username: "settings-user", password: "Password123");
        context.Users.Add(storedUser);
        await context.SaveChangesAsync();

        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        UserSettingsEntity storedSettings = _userSettingsEntityFixture.Create(userId: storedUser.Id, itemsPerPage: 10);
        storedSettings.CreatedOnUtc = storedCreatedOnUtc;
        storedSettings.CreatedBy = storedCreatedBy;
        context.UserSettings.Add(storedSettings);
        await context.SaveChangesAsync();

        UserSettingsEntity updatedSettings = _userSettingsEntityFixture.Create(id: storedSettings.Id, userId: storedUser.Id, itemsPerPage: 25);
        updatedSettings.CreatedOnUtc = DateTime.UtcNow.AddYears(-10);
        updatedSettings.CreatedBy = Guid.NewGuid();
        updatedSettings.UpdatedOnUtc = DateTime.UtcNow.AddYears(-5);
        updatedSettings.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await sut.UpdateAsync(updatedSettings, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        UserSettingsEntity reloadedSettings = await context.UserSettings.AsNoTracking().FirstAsync(settings => settings.Id == storedSettings.Id);
        Assert.Equal(25, reloadedSettings.ItemsPerPage);
        Assert.Equal(storedCreatedOnUtc, reloadedSettings.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, reloadedSettings.CreatedBy);
        Assert.Null(reloadedSettings.UpdatedOnUtc);
        Assert.Null(reloadedSettings.UpdatedBy);
    }

    [Fact]
    public async Task InsertAsync_WhenVuMeterSettingsAreProvided_ShouldPersistEveryValue()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-usersettingsvu-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        UserSettingsRepository sut = new(context);

        UserEntity storedUser = _userEntityFixture.Create(username: "vu-meter-settings-user", password: "Password123");
        context.Users.Add(storedUser);
        await context.SaveChangesAsync();

        UserSettingsEntity settingsToStore = _userSettingsEntityFixture.Create(
            userId: storedUser.Id,
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 20,
            vuMeterSegmentGap: 4,
            vuMeterMediumThreshold: 0.35,
            vuMeterHighThreshold: 0.9,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 1200,
            vuMeterReleasePerSecond: 7.5,
            vuMeterPeakReleasePerSecond: 0.25);
        settingsToStore.CreatedOnUtc = DateTime.UtcNow;
        settingsToStore.CreatedBy = storedUser.Id;

        // Act
        Result<Created> insertResult = await sut.InsertAsync(settingsToStore, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(insertResult.IsFailure);
        Assert.Equal(Result.Created, insertResult.Value);

        context.ChangeTracker.Clear();
        UserSettingsEntity reloadedSettings = await context.UserSettings.AsNoTracking().FirstAsync(settings => settings.Id == settingsToStore.Id);
        Assert.Equal(VuMeterOrientation.Horizontal, reloadedSettings.VuMeterOrientation);
        Assert.Equal(20, reloadedSettings.VuMeterSegmentCount);
        Assert.Equal(4, reloadedSettings.VuMeterSegmentGap);
        Assert.Equal(0.35, reloadedSettings.VuMeterMediumThreshold);
        Assert.Equal(0.9, reloadedSettings.VuMeterHighThreshold);
        Assert.False(reloadedSettings.VuMeterShowPeak);
        Assert.Equal(1200, reloadedSettings.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(7.5, reloadedSettings.VuMeterReleasePerSecond);
        Assert.Equal(0.25, reloadedSettings.VuMeterPeakReleasePerSecond);
    }
}
