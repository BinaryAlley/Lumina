#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Repositories.MediaLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DomainEvents;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using Nemera.Domain.Common.Events;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.Management.Commands.CancelLibraryScan;

/// <summary>
/// Contains unit tests for the <see cref="CancelLibraryScanCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibraryScanCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IValidator<CancelLibraryScanCommand> _mockValidator;
    private readonly CancelLibraryScanCommandHandler _sut;
    private readonly CancelLibraryScanCommandFixture _cancelLibraryScanCommandFixture = new();
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanCommandHandlerTests"/> class.
    /// </summary>
    public CancelLibraryScanCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockValidator = Substitute.For<IValidator<CancelLibraryScanCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated and the ownership policy allows the cancellation.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockLibraryScanRepository.UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockValidator.Validate(Arg.Any<CancelLibraryScanCommand>()).Returns([]);

        _sut = new CancelLibraryScanCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockDomainEventsQueue, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenRunningScanExists_ShouldCancelScanAndReturnSuccess()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        LibraryScanEntity runningScan = _libraryScanEntityFixture.Create(id: command.ScanId, libraryId: command.LibraryId, userId: _userId);
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(runningScan));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        await _mockLibraryScanRepository.Received(1).UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Any<LibraryScanCancelledDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenPendingScanExists_ShouldCancelScanAndReturnSuccess()
    {
        // Arrange
        // A scan that is still waiting in the scan queue can be cancelled as well.
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        LibraryScanEntity pendingScan = _libraryScanEntityFixture.Create(id: command.ScanId, libraryId: command.LibraryId, userId: _userId, status: LibraryScanJobStatus.Pending);
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(pendingScan));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockLibraryScanRepository.Received(1).UpdateAsync(
            Arg.Is<LibraryScanEntity>(libraryScan => libraryScan.Status == LibraryScanJobStatus.Canceled),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Any<LibraryScanCancelledDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenScanDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result<LibraryScanEntity?>.Success(null));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.LibraryScanning.LibraryScanNotFound, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdFails_ShouldReturnError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get library scan"));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockLibraryScanRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        LibraryScanEntity runningScan = _libraryScanEntityFixture.Create(id: command.ScanId, libraryId: command.LibraryId, userId: _userId);
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(runningScan));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScanIsInATerminalState_ShouldReturnError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        LibraryScanEntity completedScan = _libraryScanEntityFixture.Create(id: command.ScanId, libraryId: command.LibraryId, userId: _userId, status: LibraryScanJobStatus.Completed);
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(completedScan));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.LibraryScanning.CanOnlyCancelPendingOrRunningScans, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        LibraryScanEntity runningScan = _libraryScanEntityFixture.Create(id: command.ScanId, libraryId: command.LibraryId, userId: _userId);
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(runningScan));
        _mockLibraryScanRepository.UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to update library scan"));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<CancelLibraryScanCommand>()).Returns([DomainErrors.LibraryScanning.LibraryScanIdCannotBeEmpty]);

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.LibraryScanning.LibraryScanIdCannotBeEmpty, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockLibraryScanRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
