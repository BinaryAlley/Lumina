#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.DataAccess.Core.Repositories.MediaContributors;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.MediaContributors;

/// <summary>
/// Contains integration tests for the <see cref="MediaContributorRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorRepositoryTests
{
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();

    [Fact]
    public async Task InsertAsync_WhenContributorWithSameDisplayNameInDifferentCaseExists_ShouldReturnError()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediacontributorrepo-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        MediaContributorRepository sut = new(context);

        MediaContributorEntity existingContributor = _mediaContributorEntityFixture.Create(displayName: "Stephen King");
        context.MediaContributors.Add(existingContributor);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        // The comparison is case-insensitive, matching the unique index on the display name.
        Result<Created> result = await sut.InsertAsync(_mediaContributorEntityFixture.Create(displayName: "stephen king"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaContributor.MediaContributorAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenCalled_ShouldReturnOnlyTheRequestedContributorsAndIgnoreMissingIds()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediacontributorrepo-getbyids-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        MediaContributorRepository sut = new(context);
        MediaContributorEntity firstContributor = _mediaContributorEntityFixture.Create(displayName: "Frank Herbert");
        MediaContributorEntity secondContributor = _mediaContributorEntityFixture.Create(displayName: "Stephen King");
        MediaContributorEntity thirdContributor = _mediaContributorEntityFixture.Create(displayName: "Ursula K. Le Guin");
        context.MediaContributors.AddRange(firstContributor, secondContributor, thirdContributor);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<MediaContributorEntity>> result = await sut.GetByIdsAsync(
            [firstContributor.Id, thirdContributor.Id, Guid.NewGuid()], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, contributor => contributor.Id == firstContributor.Id);
        Assert.Contains(result.Value, contributor => contributor.Id == thirdContributor.Id);
        Assert.DoesNotContain(result.Value, contributor => contributor.Id == secondContributor.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenIdsIsEmpty_ShouldReturnAnEmptyList()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediacontributorrepo-getbyids-empty-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        MediaContributorRepository sut = new(context);
        context.MediaContributors.Add(_mediaContributorEntityFixture.Create());
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaContributorEntity>> result = await sut.GetByIdsAsync([], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenMoreThanFiveHundredIdsAreRequested_ShouldReturnEveryContributor()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediacontributorrepo-getbyids-chunks-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        MediaContributorRepository sut = new(context);
        const int CONTRIBUTOR_COUNT = 501;
        List<MediaContributorEntity> contributors = [.. Enumerable.Range(0, CONTRIBUTOR_COUNT).Select(index =>
            _mediaContributorEntityFixture.Create(displayName: $"Contributor {index}-{Guid.NewGuid():N}"))];
        context.MediaContributors.AddRange(contributors);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        // The lookup is chunked in batches of 500, so the 501st Id proves the second chunk is queried as well.
        Result<IReadOnlyList<MediaContributorEntity>> result = await sut.GetByIdsAsync(
            [.. contributors.Select(contributor => contributor.Id)], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(CONTRIBUTOR_COUNT, result.Value.Count);
        Assert.Equal(
            contributors.Select(contributor => contributor.Id).OrderBy(id => id),
            result.Value.Select(contributor => contributor.Id).OrderBy(id => id));
    }

    /// <summary>
    /// Creates a real SQLite backed context, so that the stored values are read exactly as they were written.
    /// </summary>
    /// <param name="anchorConnection">The open in-memory SQLite connection that keeps the database alive for the duration of the test.</param>
    /// <returns>The created context.</returns>
    private static LuminaDbContext CreateContext(SqliteConnection anchorConnection)
    {
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .Options);
        context.Database.EnsureCreated();
        return context;
    }
}
