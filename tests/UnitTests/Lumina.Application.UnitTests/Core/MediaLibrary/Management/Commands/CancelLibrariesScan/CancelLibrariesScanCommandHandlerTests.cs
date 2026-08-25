#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;

/// <summary>
/// Contains unit tests for the <see cref="CancelLibrariesScanCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibrariesScanCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly CancelLibrariesScanCommandHandler _sut;
    private readonly CancelLibrariesScanCommandFixture _cancelLibrariesScanCommandFixture = new();
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibrariesScanCommandHandlerTests"/> class.
    /// </summary>
    public CancelLibrariesScanCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and is an admin
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);
        _mockLibraryScanRepository.UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _sut = new CancelLibrariesScanCommandHandler(_mockCurrentUserService, _mockAuthorizationService, _mockDomainEventsQueue, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAdmin_ShouldCancelAllRunningScans()
    {
        // Arrange
        List<LibraryScanEntity> runningScans =
        [
            _libraryScanEntityFixture.Create(userId: _userId),
            _libraryScanEntityFixture.Create(userId: _userId)
        ];
        _mockLibraryScanRepository.GetRunningScansAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryScanEntity>>(runningScans));

        // Act
        Result<Success> result = await _sut.HandleAsync(_cancelLibrariesScanCommandFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        await _mockLibraryScanRepository.Received(2).UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(2).Enqueue(Arg.Any<LibraryScanCancelledDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldOnlyCancelScansOwnedByTheUser()
    {
        // Arrange
        Guid otherUserId = Guid.NewGuid();
        List<LibraryScanEntity> runningScans =
        [
            _libraryScanEntityFixture.Create(userId: _userId),
            _libraryScanEntityFixture.Create(userId: otherUserId)
        ];
        _mockLibraryScanRepository.GetRunningScansAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryScanEntity>>(runningScans));
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<Success> result = await _sut.HandleAsync(_cancelLibrariesScanCommandFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockLibraryScanRepository.Received(1).UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(_cancelLibrariesScanCommandFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().GetRunningScansAsync(Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetRunningScansFails_ShouldReturnError()
    {
        // Arrange
        _mockLibraryScanRepository.GetRunningScansAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get running scans"));

        // Act
        Result<Success> result = await _sut.HandleAsync(_cancelLibrariesScanCommandFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockLibraryScanRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldReturnError()
    {
        // Arrange
        List<LibraryScanEntity> runningScans = [_libraryScanEntityFixture.Create(userId: _userId)];
        _mockLibraryScanRepository.GetRunningScansAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryScanEntity>>(runningScans));
        _mockLibraryScanRepository.UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to update library scan"));

        // Act
        Result<Success> result = await _sut.HandleAsync(_cancelLibrariesScanCommandFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScanIsNotRunning_ShouldReturnError()
    {
        // Arrange
        List<LibraryScanEntity> runningScans = [_libraryScanEntityFixture.Create(userId: _userId, status: LibraryScanJobStatus.Completed)];
        _mockLibraryScanRepository.GetRunningScansAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryScanEntity>>(runningScans));

        // Act
        Result<Success> result = await _sut.HandleAsync(_cancelLibrariesScanCommandFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Lumina.Domain.Common.Errors.Errors.LibraryScanning.CanOnlyCancelRunningScans, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
