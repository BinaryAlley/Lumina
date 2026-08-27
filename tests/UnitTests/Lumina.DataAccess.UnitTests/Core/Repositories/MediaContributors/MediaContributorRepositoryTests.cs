#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.DataAccess.Core.Repositories.MediaContributors;
using Lumina.DataAccess.Core.UoW;
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

namespace Lumina.DataAccess.UnitTests.Core.Repositories.MediaContributors;

/// <summary>
/// Contains unit tests for the <see cref="MediaContributorRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly MediaContributorRepository _sut;
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContributorRepositoryTests"/> class.
    /// </summary>
    public MediaContributorRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new MediaContributorRepository(_mockContext);
    }

    [Fact]
    public async Task FindOrCreateByDisplayNameAsync_WhenContributorExists_ShouldReturnTheExistingContributor()
    {
        // Arrange
        MediaContributorEntity existingContributor = _mediaContributorEntityFixture.Create(displayName: "Stephen King");
        _mockContext.MediaContributors.Add(existingContributor);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaContributorEntity> result = await _sut.FindOrCreateByDisplayNameAsync("stephen king", null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        // the comparison is case-insensitive, so the existing contributor is returned and no new one is created
        Assert.Equal(existingContributor.Id, result.Value.Id);
        Assert.Equal("Stephen King", result.Value.DisplayName);
        Assert.Single(_mockContext.ChangeTracker.Entries<MediaContributorEntity>());
    }

    [Fact]
    public async Task FindOrCreateByDisplayNameAsync_WhenContributorDoesNotExist_ShouldCreateAndAddANewContributor()
    {
        // Arrange
        string displayName = "Frank Herbert";

        // Act
        Result<MediaContributorEntity> result = await _sut.FindOrCreateByDisplayNameAsync(displayName, "Franklin Patrick Herbert", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(displayName, result.Value.DisplayName);
        Assert.Equal("Franklin Patrick Herbert", result.Value.LegalName);

        EntityEntry<MediaContributorEntity>? addedContributor = _mockContext.ChangeTracker.Entries<MediaContributorEntity>()
            .FirstOrDefault(entityEntry => entityEntry.State == EntityState.Added && entityEntry.Entity.Id == result.Value.Id);
        Assert.NotNull(addedContributor);
    }

    [Fact]
    public async Task FindOrCreateByDisplayNameAsync_WhenContributorWithSameNameExistsInDifferentCase_ShouldNotCreateADuplicate()
    {
        // Arrange
        MediaContributorEntity existingContributor = _mediaContributorEntityFixture.Create(displayName: "Frank Herbert");
        _mockContext.MediaContributors.Add(existingContributor);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaContributorEntity> result = await _sut.FindOrCreateByDisplayNameAsync("FRANK HERBERT", "Franklin Patrick Herbert", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(existingContributor.Id, result.Value.Id);
        Assert.Equal(existingContributor.DisplayName, result.Value.DisplayName);
        Assert.Single(_mockContext.ChangeTracker.Entries<MediaContributorEntity>());
    }

    [Fact]
    public async Task GetByIdsAsync_WhenCalled_ShouldReturnOnlyTheContributorsWithTheProvidedIds()
    {
        // Arrange
        MediaContributorEntity firstContributor = _mediaContributorEntityFixture.Create();
        MediaContributorEntity secondContributor = _mediaContributorEntityFixture.Create();
        MediaContributorEntity thirdContributor = _mediaContributorEntityFixture.Create();
        _mockContext.MediaContributors.AddRange(firstContributor, secondContributor, thirdContributor);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaContributorEntity>> result = await _sut.GetByIdsAsync([firstContributor.Id, thirdContributor.Id], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, contributor => contributor.Id == firstContributor.Id);
        Assert.Contains(result.Value, contributor => contributor.Id == thirdContributor.Id);
    }
}
