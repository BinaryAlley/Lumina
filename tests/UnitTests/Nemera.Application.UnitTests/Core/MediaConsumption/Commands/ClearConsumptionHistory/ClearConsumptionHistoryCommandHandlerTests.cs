#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Commands.ClearConsumptionHistory;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.ClearConsumptionHistory;
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

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.ClearConsumptionHistory;

/// <summary>
/// Contains unit tests for the <see cref="ClearConsumptionHistoryCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearConsumptionHistoryCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IMediaConsumptionRecordRepository _mockMediaConsumptionRecordRepository;
    private readonly IMediaResumePointRepository _mockMediaResumePointRepository;
    private readonly IValidator<ClearConsumptionHistoryCommand> _mockValidator;
    private readonly ClearConsumptionHistoryCommandHandler _sut;
    private readonly ClearConsumptionHistoryCommandFixture _clearConsumptionHistoryCommandFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearConsumptionHistoryCommandHandlerTests"/> class.
    /// </summary>
    public ClearConsumptionHistoryCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockMediaConsumptionRecordRepository = Substitute.For<IMediaConsumptionRecordRepository>();
        _mockMediaResumePointRepository = Substitute.For<IMediaResumePointRepository>();

        _mockUnitOfWork.MediaConsumptionRecordRepository.Returns(_mockMediaConsumptionRecordRepository);
        _mockUnitOfWork.MediaResumePointRepository.Returns(_mockMediaResumePointRepository);
        // Default stubs: saving changes succeeds, the current user is authenticated, the target is accessible and the history is deleted.
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(_userId, Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockMediaConsumptionRecordRepository.DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);

        _mockValidator = Substitute.For<IValidator<ClearConsumptionHistoryCommand>>();
        _mockValidator.Validate(Arg.Any<ClearConsumptionHistoryCommand>()).Returns([]);

        _sut = new ClearConsumptionHistoryCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotDeleteAnything()
    {
        // Arrange
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<ClearConsumptionHistoryCommand>())
            .Returns([DomainErrors.MediaConsumption.TargetUserIdIsInvalid]);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.TargetUserIdIsInvalid, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserInLibraryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotDeleteAnything()
    {
        // Arrange
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccessToTheTarget_ShouldReturnNotAuthorizedAndNotDeleteAnything()
    {
        // Arrange
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create();
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
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenHistoryDeletionFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption history could not be deleted.");
        _mockMediaConsumptionRecordRepository.DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
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
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create();
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
    public async Task HandleAsync_WhenSuccessfulForTheCurrentUser_ShouldDeleteOnlyTheHistoryAndReturnDeleted()
    {
        // Arrange
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create(includeTargetUserId: false);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        await _mockMediaConsumptionRecordRepository.Received(1).DeleteAllForUserAsync(_userId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserInLibraryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTargetUserIsSupplied_ShouldDeleteTheHistoryOfTheTargetUser()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create(targetUserId: targetUserId.ToString());

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Is<ConsumptionActivityAccessPolicyContext>(context => context.TargetUserId == targetUserId),
            Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).DeleteAllForUserAsync(targetUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheCancellationTokenToEveryDependency()
    {
        // Arrange
        ClearConsumptionHistoryCommand command = _clearConsumptionHistoryCommandFixture.Create(includeTargetUserId: false);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Any<ConsumptionActivityAccessPolicyContext>(),
            Arg.Is(cancellationToken));
        await _mockMediaConsumptionRecordRepository.Received(1).DeleteAllForUserAsync(_userId, Arg.Is(cancellationToken));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
    }
}
