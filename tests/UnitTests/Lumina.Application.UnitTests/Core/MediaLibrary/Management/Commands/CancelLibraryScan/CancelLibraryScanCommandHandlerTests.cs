#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.CancelLibraryScan;
using Lumina.Domain.Common.Events;
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
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Commands.CancelLibraryScan;

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
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockValidator = Substitute.For<IValidator<CancelLibraryScanCommand>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and the ownership policy allows the cancellation
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
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, Arg.Any<CancellationToken>())
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
    public async Task HandleAsync_WhenScanDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, Arg.Any<CancellationToken>())
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
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, Arg.Any<CancellationToken>())
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
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, Arg.Any<CancellationToken>())
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
        await _mockLibraryScanRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScanIsNotRunning_ShouldReturnError()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        LibraryScanEntity completedScan = _libraryScanEntityFixture.Create(id: command.ScanId, libraryId: command.LibraryId, userId: _userId, status: LibraryScanJobStatus.Completed);
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(completedScan));

        // Act
        Result<Success> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.LibraryScanning.CanOnlyCancelRunningScans, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        CancelLibraryScanCommand command = _cancelLibraryScanCommandFixture.Create();
        LibraryScanEntity runningScan = _libraryScanEntityFixture.Create(id: command.ScanId, libraryId: command.LibraryId, userId: _userId);
        _mockLibraryScanRepository.GetByIdAsync(command.ScanId, Arg.Any<CancellationToken>())
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
        await _mockLibraryScanRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryScanRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
