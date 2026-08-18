#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Core.MediaLibrary.Management.Commands.ScanLibrary;
using Lumina.Contracts.Responses.MediaLibrary.Management;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Commands.ScanLibrary;

/// <summary>
/// Contains unit tests for the <see cref="ScanLibraryCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IValidator<ScanLibraryCommand> _mockValidator;
    private readonly ScanLibraryCommandHandler _sut;
    private readonly ScanLibraryCommandFixture _scanLibraryCommandFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryCommandHandlerTests"/> class.
    /// </summary>
    public ScanLibraryCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockValidator = Substitute.For<IValidator<ScanLibraryCommand>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and the ownership policy allows the scan
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockLibraryScanRepository.GetPastMonthScansByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<LibraryScanEntity>>([]));
        _mockLibraryScanRepository.InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockValidator.Validate(Arg.Any<ScanLibraryCommand>()).Returns([]);

        _sut = new ScanLibraryCommandHandler(_mockAuthorizationService, _mockCurrentUserService, _mockDomainEventsQueue, _mockUnitOfWork, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsEnabledAndUnlocked_ShouldQueueScanAndReturnResponse()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: command.Id, userId: _userId);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(command.Id, result.Value.LibraryId);
        await _mockLibraryScanRepository.Received(1).InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Any<LibraryScanQueuedDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result<LibraryEntity?>.Success(null));

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdFails_ShouldReturnError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get library"));

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: command.Id, userId: _userId);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsDisabled_ShouldReturnCannotScanDisabledLibraryError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: command.Id, userId: _userId, isEnabled: false);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.CannotScanDisabledLibrary, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsLocked_ShouldReturnCannotScanLockedLibraryError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: command.Id, userId: _userId, isLocked: true);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.CannotScanLockedLibrary, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDomainConversionFails_ShouldReturnError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: command.Id, userId: _userId, contentLocations: [""]);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.FileSystemManagement.InvalidPath, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPastMonthScansRetrievalFails_ShouldReturnError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: command.Id, userId: _userId);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockLibraryScanRepository.GetPastMonthScansByLibraryIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get past month scans"));

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIsAlreadyBeingScanned_ShouldReturnError()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: command.Id, userId: _userId);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        LibraryScanEntity runningScan = _libraryScanEntityFixture.Create(libraryId: command.Id, userId: _userId, status: LibraryScanJobStatus.Running);
        _mockLibraryScanRepository.GetPastMonthScansByLibraryIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<IEnumerable<LibraryScanEntity>>([runningScan]));

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.LibraryScanning.LibraryAlreadyBeingScanned, result.FirstError);
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        LibraryEntity library = _libraryEntityFixture.Create(id: command.Id, userId: _userId);
        _mockLibraryRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockLibraryScanRepository.InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to insert library scan"));

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        ScanLibraryCommand command = _scanLibraryCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<ScanLibraryCommand>()).Returns([DomainErrors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<MediaLibraryScanResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryScanRepository.DidNotReceive().InsertAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
