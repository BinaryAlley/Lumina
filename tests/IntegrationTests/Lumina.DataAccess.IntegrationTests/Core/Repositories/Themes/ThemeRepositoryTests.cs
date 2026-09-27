#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.DataAccess.Core.Repositories.Themes;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Themes;

/// <summary>
/// Contains integration tests for the <see cref="ThemeRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeRepositoryTests
{
    private readonly ThemeEntityFixture _themeEntityFixture = new();

    [Fact]
    public async Task UpdateAsync_WhenAnEditableValueChanges_ShouldPreserveTheStoredAuditColumns()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-themerepo-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ThemeRepository sut = new(context);

        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        ThemeEntity storedTheme = _themeEntityFixture.Create(createdOnUtc: storedCreatedOnUtc, createdBy: storedCreatedBy);
        context.Themes.Add(storedTheme);
        await context.SaveChangesAsync();

        ThemeEntity updatedTheme = _themeEntityFixture.Create(
            id: storedTheme.Id,
            themeId: storedTheme.ThemeId,
            name: "Updated Theme",
            description: storedTheme.Description,
            author: storedTheme.Author,
            version: storedTheme.Version,
            installSource: storedTheme.InstallSource,
            createdOnUtc: DateTime.UtcNow.AddYears(-10),
            createdBy: Guid.NewGuid(),
            includeUpdatedOnUtc: true,
            updatedOnUtc: DateTime.UtcNow.AddYears(-5),
            includeUpdatedBy: true,
            updatedBy: Guid.NewGuid());

        // Act
        Result<Updated> result = await sut.UpdateAsync(updatedTheme, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        ThemeEntity reloadedTheme = await context.Themes.AsNoTracking().FirstAsync(theme => theme.Id == storedTheme.Id);
        Assert.Equal("Updated Theme", reloadedTheme.Name);
        Assert.Equal(storedCreatedOnUtc, reloadedTheme.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, reloadedTheme.CreatedBy);
        Assert.Null(reloadedTheme.UpdatedOnUtc);
        Assert.Null(reloadedTheme.UpdatedBy);
    }
}
