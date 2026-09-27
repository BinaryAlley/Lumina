#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.Repositories.Libraries;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Libraries;

/// <summary>
/// Contains integration tests for the <see cref="LibraryRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryRepositoryTests
{
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    [Fact]
    public async Task UpdateAsync_WhenNothingChanged_ShouldNotChangeTheLibraryAudit()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-libraryrepo-update-noop-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        LibraryRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        LibraryEntity library = _libraryEntityFixture.Create(userId: userId, contentLocations: ["/media/one", "/media/two"]);
        context.Libraries.Add(library);
        await context.SaveChangesAsync();
        LibraryEntity incoming = await LoadDetachedLibraryAsync(context, library.Id);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        LibraryEntity? storedLibrary = await context.Libraries.AsNoTracking().Include(candidate => candidate.ContentLocations).FirstOrDefaultAsync(candidate => candidate.Id == library.Id);
        Assert.NotNull(storedLibrary);
        Assert.Null(storedLibrary!.UpdatedOnUtc);
        Assert.Equal(2, storedLibrary.ContentLocations.Count);
    }

    [Fact]
    public async Task UpdateAsync_WhenContentLocationsChange_ShouldApplyOnlyTheDeltasAndKeepTheUnchangedOnes()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-libraryrepo-update-locations-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        LibraryRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        LibraryEntity library = _libraryEntityFixture.Create(userId: userId, contentLocations: ["/media/one", "/media/two", "/media/keep"]);
        context.Libraries.Add(library);
        await context.SaveChangesAsync();
        Guid keptLocationId = context.Entry(library.ContentLocations.Single(location => location.Path == "/media/keep")).Property<Guid>("Id").CurrentValue;
        Guid retainedLocationId = context.Entry(library.ContentLocations.Single(location => location.Path == "/media/one")).Property<Guid>("Id").CurrentValue;
        LibraryEntity incoming = await LoadDetachedLibraryAsync(context, library.Id);
        incoming.ContentLocations.Clear();
        foreach (string path in new[] { "/media/one", "/media/keep", "/media/three" })
            incoming.ContentLocations.Add(new LibraryContentLocationEntity { Path = path });

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        LibraryEntity? storedLibrary = await context.Libraries.AsNoTracking().Include(candidate => candidate.ContentLocations).FirstOrDefaultAsync(candidate => candidate.Id == library.Id);
        Assert.NotNull(storedLibrary);
        Assert.Equal(3, storedLibrary!.ContentLocations.Count);
        Assert.Equal(["/media/keep", "/media/one", "/media/three"], [.. storedLibrary.ContentLocations.Select(location => location.Path).OrderBy(path => path)]);
        // The two locations that did not change must keep their stored row identity, instead of being deleted and re-inserted.
        Assert.Equal(retainedLocationId, context.Entry(library.ContentLocations.Single(location => location.Path == "/media/one")).Property<Guid>("Id").CurrentValue);
        Assert.Equal(keptLocationId, context.Entry(library.ContentLocations.Single(location => location.Path == "/media/keep")).Property<Guid>("Id").CurrentValue);
    }

    /// <summary>
    /// Creates a real SQLite backed context with the auditing interceptor attached, so that audit columns behave exactly like in production.
    /// </summary>
    /// <param name="anchorConnection">The open in-memory SQLite connection that keeps the database alive for the duration of the test.</param>
    /// <param name="userId">The Id of the user reported as the current user.</param>
    /// <param name="utcNow">The current UTC time reported by the time provider.</param>
    /// <returns>The created context.</returns>
    private static LuminaDbContext CreateAuditedContext(SqliteConnection anchorConnection, Guid userId, DateTime utcNow)
    {
        ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(utcNow);
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .AddInterceptors(new UpdateAuditableEntitiesInterceptor(currentUserService, dateTimeProvider))
            .Options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Loads a detached copy of a stored library, with its content locations, so that it can be handed to the repository as the desired state of an edit.
    /// </summary>
    /// <param name="context">The context that tracks the stored library.</param>
    /// <param name="libraryId">The Id of the library to load.</param>
    /// <returns>The detached copy of the library.</returns>
    private static async Task<LibraryEntity> LoadDetachedLibraryAsync(LuminaDbContext context, Guid libraryId)
    {
        return await context.Libraries
            .AsNoTracking()
            .Include(library => library.ContentLocations)
            .FirstAsync(library => library.Id == libraryId);
    }
}
