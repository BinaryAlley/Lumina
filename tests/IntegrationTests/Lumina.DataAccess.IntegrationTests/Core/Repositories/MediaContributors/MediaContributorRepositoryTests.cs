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
using System.Diagnostics.CodeAnalysis;
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
}
