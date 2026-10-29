#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Commands.ClearResumePoints;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.ClearResumePoints;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.ClearResumePoints;

/// <summary>
/// Contains unit tests for the <see cref="ClearResumePointsCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IMediaConsumptionRecordRepository _mockMediaConsumptionRecordRepository;
    private readonly IMediaResumePointRepository _mockMediaResumePointRepository;
    private readonly IValidator<ClearResumePointsCommand> _mockValidator;
    private readonly ClearResumePointsCommandHandler _sut;
    private readonly ClearResumePointsCommandFixture _clearResumePointsCommandFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearResumePointsCommandHandlerTests"/> class.
    /// </summary>
    public ClearResumePointsCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockMediaConsumptionRecordRepository = Substitute.For<IMediaConsumptionRecordRepository>();
        _mockMediaResumePointRepository = Substitute.For<IMediaResumePointRepository>();

        _mockUnitOfWork.MediaConsumptionRecordRepository.Returns(_mockMediaConsumptionRecordRepository);
        _mockUnitOfWork.MediaResumePointRepository.Returns(_mockMediaResumePointRepository);
        // Default stubs: saving changes succeeds, the current user is authenticated, the target is accessible and the resume points are deleted.
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(_userId, Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockMediaResumePointRepository.DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockMediaResumePointRepository.DeleteAllForUserInLibraryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);

        _mockValidator = Substitute.For<IValidator<ClearResumePointsCommand>>();
        _mockValidator.Validate(Arg.Any<ClearResumePointsCommand>()).Returns([]);

        _sut = new ClearResumePointsCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotDeleteAnything()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<ClearResumePointsCommand>())
            .Returns([DomainErrors.MediaConsumption.TargetUserIdIsInvalid]);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.TargetUserIdIsInvalid, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserInLibraryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotDeleteAnything()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserInLibraryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccessToTheTarget_ShouldReturnNotAuthorizedAndNotDeleteAnything()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(_userId, Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Is<ConsumptionActivityAccessPolicyContext>(context => context.TargetUserId == Guid.Parse(command.TargetUserId!)),
            Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserInLibraryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoLibraryIsSupplied_ShouldDeleteEveryResumePointOfTheUser()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(includeTargetUserId: false, includeLibraryId: false);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        await _mockMediaResumePointRepository.Received(1).DeleteAllForUserAsync(_userId, Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserInLibraryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenALibraryIsSupplied_ShouldDeleteOnlyTheResumePointsOfThatLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(
            includeTargetUserId: false,
            libraryId: libraryId.ToString());

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaResumePointRepository.Received(1).DeleteAllForUserInLibraryAsync(_userId, libraryId, Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingEveryResumePointFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(includeLibraryId: false);
        Error error = Error.Failure("Persistence.Failure", "The resume points could not be deleted.");
        _mockMediaResumePointRepository.DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingTheResumePointsOfALibraryFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The resume points of the library could not be deleted.");
        _mockMediaResumePointRepository.DeleteAllForUserInLibraryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnThatError()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create();
        Error error = ApplicationErrors.Persistence.ConcurrencyConflict;
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTargetUserIsSupplied_ShouldDeleteTheResumePointsOfTheTargetUser()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(
            targetUserId: targetUserId.ToString(),
            libraryId: libraryId.ToString());

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Is<ConsumptionActivityAccessPolicyContext>(context => context.TargetUserId == targetUserId),
            Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.Received(1).DeleteAllForUserInLibraryAsync(targetUserId, libraryId, Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserInLibraryAsync(_userId, Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenClearingResumePoints_ShouldNotTouchTheConsumptionHistory()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(includeLibraryId: false);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheCancellationTokenToEveryDependency()
    {
        // Arrange
        ClearResumePointsCommand command = _clearResumePointsCommandFixture.Create(includeTargetUserId: false, includeLibraryId: false);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Any<ConsumptionActivityAccessPolicyContext>(),
            Arg.Is(cancellationToken));
        await _mockMediaResumePointRepository.Received(1).DeleteAllForUserAsync(_userId, Arg.Is(cancellationToken));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
    }
}
