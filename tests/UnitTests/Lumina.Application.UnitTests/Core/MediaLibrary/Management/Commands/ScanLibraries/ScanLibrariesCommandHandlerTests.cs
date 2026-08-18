#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Commands.ScanLibraries;

/// <summary>
/// Contains unit tests for the <see cref="ScanLibrariesCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibrariesCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly ScanLibrariesCommandHandler _sut;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesCommandHandlerTests"/> class.
    /// </summary>
    public ScanLibrariesCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and is an admin
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);
        _mockLibraryScanRepository.GetPastMonthScansByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<LibraryScanEntity>>([]));
        _mockLibraryScanRepository.InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        _sut = new ScanLibrariesCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockDomainEventsQueue, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAdmin_ShouldStartScansForAllEnabledAndUnlockedLibraries()
    {
        // Arrange
        List<LibraryEntity> libraries =
        [
            _libraryEntityFixture.Create(userId: _userId),
            _libraryEntityFixture.Create(userId: _userId)
        ];
        _mockLibraryRepository.GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryEntity>>(libraries));

        // Act
        Result<IEnumerable<MediaLibraryScanResponse>> result = await _sut.HandleAsync(new ScanLibrariesCommand(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(libraries[0].Id, result.Value.First().LibraryId);
        Assert.Equal(libraries[1].Id, result.Value.ElementAt(1).LibraryId);
        await _mockLibraryScanRepository.Received(2).InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(2).Enqueue(Arg.Any<LibraryScanQueuedDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldOnlyStartScansForLibrariesOwnedByTheUser()
    {
        // Arrange
        Guid otherUserId = Guid.NewGuid();
        List<LibraryEntity> libraries =
        [
            _libraryEntityFixture.Create(userId: _userId),
            _libraryEntityFixture.Create(userId: otherUserId)
        ];
        _mockLibraryRepository.GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryEntity>>(libraries));
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<IEnumerable<MediaLibraryScanResponse>> result = await _sut.HandleAsync(new ScanLibrariesCommand(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        MediaLibraryScanResponse response = Assert.Single(result.Value);
        Assert.Equal(libraries[0].Id, response.LibraryId);
        await _mockLibraryScanRepository.Received(1).InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IEnumerable<MediaLibraryScanResponse>> result = await _sut.HandleAsync(new ScanLibrariesCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllEnabledAndUnlockedFails_ShouldReturnError()
    {
        // Arrange
        _mockLibraryRepository.GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get libraries"));

        // Act
        Result<IEnumerable<MediaLibraryScanResponse>> result = await _sut.HandleAsync(new ScanLibrariesCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsAlreadyBeingScanned_ShouldSkipThatLibraryAndContinue()
    {
        // Arrange
        LibraryEntity firstLibrary = _libraryEntityFixture.Create(userId: _userId);
        LibraryEntity secondLibrary = _libraryEntityFixture.Create(userId: _userId);
        _mockLibraryRepository.GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<LibraryEntity>>([firstLibrary, secondLibrary]));

        LibraryScanEntity runningScan = _libraryScanEntityFixture.Create(libraryId: firstLibrary.Id, userId: _userId, status: Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary.LibraryScanJobStatus.Running);
        _mockLibraryScanRepository.GetPastMonthScansByLibraryIdAsync(firstLibrary.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<LibraryScanEntity>>([runningScan]));

        // Act
        Result<IEnumerable<MediaLibraryScanResponse>> result = await _sut.HandleAsync(new ScanLibrariesCommand(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        MediaLibraryScanResponse response = Assert.Single(result.Value);
        Assert.Equal(secondLibrary.Id, response.LibraryId);
        await _mockLibraryScanRepository.Received(1).InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertFails_ShouldReturnError()
    {
        // Arrange
        List<LibraryEntity> libraries = [_libraryEntityFixture.Create(userId: _userId)];
        _mockLibraryRepository.GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryEntity>>(libraries));
        _mockLibraryScanRepository.InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to insert library scan"));

        // Act
        Result<IEnumerable<MediaLibraryScanResponse>> result = await _sut.HandleAsync(new ScanLibrariesCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPastMonthScansRetrievalFails_ShouldReturnError()
    {
        // Arrange
        List<LibraryEntity> libraries = [_libraryEntityFixture.Create(userId: _userId)];
        _mockLibraryRepository.GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryEntity>>(libraries));
        _mockLibraryScanRepository.GetPastMonthScansByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get past month scans"));

        // Act
        Result<IEnumerable<MediaLibraryScanResponse>> result = await _sut.HandleAsync(new ScanLibrariesCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDomainConversionFails_ShouldReturnError()
    {
        // Arrange
        List<LibraryEntity> libraries = [_libraryEntityFixture.Create(userId: _userId, contentLocations: [""])];
        _mockLibraryRepository.GetAllEnabledAndUnlockedAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryEntity>>(libraries));

        // Act
        Result<IEnumerable<MediaLibraryScanResponse>> result = await _sut.HandleAsync(new ScanLibrariesCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.FileSystemManagement.InvalidPath, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
