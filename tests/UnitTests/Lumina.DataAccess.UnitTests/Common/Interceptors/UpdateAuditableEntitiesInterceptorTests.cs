#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Interceptors;

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
        LuminaDbContext context = CreateContext();
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
        LuminaDbContext context = CreateContext();
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
    public async Task SavingChangesAsync_WhenNoUserIsAuthenticated_ShouldUseGuidEmptyForCreationAndSystemUserForModification()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);
        LuminaDbContext context = CreateContext();
        BookEntity book = _bookEntityFixture.Create();
        context.Books.Add(book);
        await context.SaveChangesAsync();

        // Act
        book.Title = "Updated Title";
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(Guid.Empty, book.CreatedBy);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), book.UpdatedBy);
        Assert.Equal(_fixedUtcNow, book.UpdatedOnUtc);
    }

    [Fact]
    public void SavingChanges_WhenAddingAuditableEntity_ShouldSetCreationProperties()
    {
        // Arrange
        LuminaDbContext context = CreateContext();
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
        LuminaDbContext context = CreateContext();
        LibraryScanResultEntity scanResult = _libraryScanResultEntityFixture.Create();

        // Act
        context.LibraryScanResults.Add(scanResult);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(EntityState.Unchanged, context.Entry(scanResult).State);
    }

    /// <summary>
    /// Creates a <see cref="LuminaDbContext"/> that uses the interceptor under test.
    /// </summary>
    /// <returns>A configured <see cref="LuminaDbContext"/> instance.</returns>
    private LuminaDbContext CreateContext()
    {
        DbContextOptions<LuminaDbContext> options = new DbContextOptionsBuilder<LuminaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(_sut)
            .Options;
        return new LuminaDbContext(options);
    }
}
