#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.DataAccess.Core.Repositories.Themes;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly ThemeRepository _sut;
    private readonly ThemeEntityFixture _themeEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeRepositoryTests"/> class.
    /// </summary>
    public ThemeRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new ThemeRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenThemeDoesNotExist_ShouldAddThemeToContextAndReturnCreated()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(theme, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<ThemeEntity>? addedTheme = _mockContext.ChangeTracker.Entries<ThemeEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == theme.Id);
        Assert.NotNull(addedTheme);
        Assert.Equal(theme.ThemeId, addedTheme.Entity.ThemeId);
    }

    [Fact]
    public async Task InsertAsync_WhenThemeAlreadyExists_ShouldReturnError()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create();
        _mockContext.Themes.Add(theme);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Created> result = await _sut.InsertAsync(theme, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<ThemeEntity>());
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnAllThemes()
    {
        // Arrange
        List<ThemeEntity> themes = _themeEntityFixture.CreateMany();
        _mockContext.Themes.AddRange(themes);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<ThemeEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());
        Assert.Equal(themes, result.Value);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoThemesExist_ShouldReturnEmptyList()
    {
        // Act
        Result<IEnumerable<ThemeEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByThemeIdAsync_WhenThemeExists_ShouldReturnTheme()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create(themeId: "my-theme");
        _mockContext.Themes.Add(theme);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<ThemeEntity?> result = await _sut.GetByThemeIdAsync(theme.ThemeId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(theme, result.Value);
    }

    [Fact]
    public async Task GetByThemeIdAsync_WhenThemeDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<ThemeEntity?> result = await _sut.GetByThemeIdAsync("nonexistent-theme", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenThemeExists_ShouldUpdateThemeAndReturnUpdated()
    {
        // Arrange
        ThemeEntity existingTheme = _themeEntityFixture.Create(name: "Existing Theme");
        _mockContext.Themes.Add(existingTheme);
        await _mockContext.SaveChangesAsync();

        ThemeEntity updatedTheme = _themeEntityFixture.Create(id: existingTheme.Id, themeId: existingTheme.ThemeId, name: "Updated Theme", includeIsCurrent: true, isCurrent: true);
        updatedTheme.CreatedOnUtc = existingTheme.CreatedOnUtc;
        updatedTheme.CreatedBy = existingTheme.CreatedBy;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(updatedTheme, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        ThemeEntity? modifiedTheme = await _mockContext.Themes.FirstOrDefaultAsync(theme => theme.Id == existingTheme.Id);
        Assert.NotNull(modifiedTheme);
        Assert.Equal("Updated Theme", modifiedTheme.Name);
        Assert.True(modifiedTheme.IsCurrent);
    }

    [Fact]
    public async Task UpdateAsync_WhenThemeDoesNotExist_ShouldReturnError()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(theme, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenThemeExists_ShouldRemoveThemeFromContextAndReturnDeleted()
    {
        // Arrange
        ThemeEntity theme = _themeEntityFixture.Create();
        _mockContext.Themes.Add(theme);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(theme.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        EntityEntry<ThemeEntity>? deletedTheme = _mockContext.ChangeTracker.Entries<ThemeEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Deleted && entry.Entity.Id == theme.Id);
        Assert.NotNull(deletedTheme);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenThemeDoesNotExist_ShouldReturnError()
    {
        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Themes.ThemeNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenCurrentThemeExists_ShouldReturnCurrentTheme()
    {
        // Arrange
        ThemeEntity currentTheme = _themeEntityFixture.Create(includeIsCurrent: true, isCurrent: true);
        ThemeEntity nonCurrentTheme = _themeEntityFixture.Create(includeIsCurrent: true, isCurrent: false);
        _mockContext.Themes.AddRange(currentTheme, nonCurrentTheme);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<ThemeEntity?> result = await _sut.GetCurrentAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(currentTheme, result.Value);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenNoCurrentThemeExists_ShouldReturnNull()
    {
        // Arrange
        ThemeEntity nonCurrentTheme = _themeEntityFixture.Create(includeIsCurrent: true, isCurrent: false);
        _mockContext.Themes.Add(nonCurrentTheme);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<ThemeEntity?> result = await _sut.GetCurrentAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }
}
