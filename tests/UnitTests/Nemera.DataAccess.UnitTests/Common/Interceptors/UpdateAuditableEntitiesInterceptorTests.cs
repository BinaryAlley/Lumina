#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Common;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Time;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.DataAccess.Common.Interceptors;
using Nemera.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.UnitTests.Common.Interceptors;

/// <summary>
/// Contains unit tests for the <see cref="UpdateAuditableEntitiesInterceptor"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAuditableEntitiesInterceptorTests
{
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDateTimeProvider _mockDateTimeProvider;
    private readonly UpdateAuditableEntitiesInterceptor _sut;
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryScanResultEntityFixture _libraryScanResultEntityFixture = new();
    private readonly Guid _userId;
    private readonly DateTime _fixedUtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAuditableEntitiesInterceptorTests"/> class.
    /// </summary>
    public UpdateAuditableEntitiesInterceptorTests()
    {
        _userId = Guid.NewGuid();
        _fixedUtcNow = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockDateTimeProvider = Substitute.For<IDateTimeProvider>();
        _mockDateTimeProvider.UtcNow.Returns(_fixedUtcNow);
        _sut = new UpdateAuditableEntitiesInterceptor(_mockCurrentUserService, _mockDateTimeProvider);
    }

    [Fact]
    public async Task SavingChangesAsync_WhenAddingAuditableEntity_ShouldSetCreationProperties()
    {
        // Arrange
        NemeraDbContext context = CreateContext();
        BookEntity book = _bookEntityFixture.Create();

        // Act
        context.Books.Add(book);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(_fixedUtcNow, book.CreatedOnUtc);
        Assert.Equal(_userId, book.CreatedBy);
        Assert.Null(book.UpdatedOnUtc);
        Assert.Null(book.UpdatedBy);
    }

    [Fact]
    public async Task SavingChangesAsync_WhenModifyingAuditableEntity_ShouldSetModificationProperties()
    {
        // Arrange
        NemeraDbContext context = CreateContext();
        BookEntity book = _bookEntityFixture.Create();
        context.Books.Add(book);
        await context.SaveChangesAsync();

        // Act
        book.Title = "Updated Title";
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(_fixedUtcNow, book.UpdatedOnUtc);
        Assert.Equal(_userId, book.UpdatedBy);
        Assert.Equal(_fixedUtcNow, book.CreatedOnUtc);
        Assert.Equal(_userId, book.CreatedBy);
    }

    [Fact]
    public async Task SavingChangesAsync_WhenNoUserIsAuthenticated_ShouldLeaveCreationAndModificationUnattributed()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);
        NemeraDbContext context = CreateContext();
        BookEntity book = _bookEntityFixture.Create();
        context.Books.Add(book);
        await context.SaveChangesAsync();

        // Act
        book.Title = "Updated Title";
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(Guid.Empty, book.CreatedBy);
        Assert.Equal(Guid.Empty, book.UpdatedBy);
        Assert.Equal(_fixedUtcNow, book.UpdatedOnUtc);
    }

    [Fact]
    public async Task SavingChangesAsync_WhenInitializationSave_ShouldPreserveTheExplicitAuditValues()
    {
        // Arrange
        NemeraDbContext context = CreateContext();
        BookEntity book = _bookEntityFixture.Create();
        DateTime initializationUtcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        book.CreatedOnUtc = initializationUtcNow;
        book.CreatedBy = AuditIdentities.s_systemUserId;
        book.UpdatedOnUtc = null;
        book.UpdatedBy = null;

        // Act
        context.IsInitializationSave = true;
        context.Books.Add(book);
        await context.SaveChangesAsync();
        context.IsInitializationSave = false;

        // Assert
        Assert.Equal(initializationUtcNow, book.CreatedOnUtc);
        Assert.Equal(AuditIdentities.s_systemUserId, book.CreatedBy);
        Assert.Null(book.UpdatedOnUtc);
        Assert.Null(book.UpdatedBy);
    }

    [Fact]
    public void SavingChanges_WhenAddingAuditableEntity_ShouldSetCreationProperties()
    {
        // Arrange
        NemeraDbContext context = CreateContext();
        BookEntity book = _bookEntityFixture.Create();

        // Act
        context.Books.Add(book);
        context.SaveChanges();

        // Assert
        Assert.Equal(_fixedUtcNow, book.CreatedOnUtc);
        Assert.Equal(_userId, book.CreatedBy);
        Assert.Null(book.UpdatedOnUtc);
        Assert.Null(book.UpdatedBy);
    }

    [Fact]
    public async Task SavingChangesAsync_WhenTrackingNonAuditableEntity_ShouldNotThrow()
    {
        // Arrange
        NemeraDbContext context = CreateContext();
        LibraryScanResultEntity scanResult = _libraryScanResultEntityFixture.Create();

        // Act
        context.LibraryScanResults.Add(scanResult);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(EntityState.Unchanged, context.Entry(scanResult).State);
    }

    /// <summary>
    /// Creates a <see cref="NemeraDbContext"/> that uses the interceptor under test.
    /// </summary>
    /// <returns>A configured <see cref="NemeraDbContext"/> instance.</returns>
    private NemeraDbContext CreateContext()
    {
        DbContextOptions<NemeraDbContext> options = new DbContextOptionsBuilder<NemeraDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(_sut)
            .Options;
        return new NemeraDbContext(options);
    }
}
