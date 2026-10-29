#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionRecord;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.DeleteConsumptionRecord;
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

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.DeleteConsumptionRecord;

/// <summary>
/// Contains unit tests for the <see cref="DeleteConsumptionRecordCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionRecordCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IMediaConsumptionRecordRepository _mockMediaConsumptionRecordRepository;
    private readonly IMediaResumePointRepository _mockMediaResumePointRepository;
    private readonly IValidator<DeleteConsumptionRecordCommand> _mockValidator;
    private readonly DeleteConsumptionRecordCommandHandler _sut;
    private readonly DeleteConsumptionRecordCommandFixture _deleteConsumptionRecordCommandFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionRecordCommandHandlerTests"/> class.
    /// </summary>
    public DeleteConsumptionRecordCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockMediaConsumptionRecordRepository = Substitute.For<IMediaConsumptionRecordRepository>();
        _mockMediaResumePointRepository = Substitute.For<IMediaResumePointRepository>();

        _mockUnitOfWork.MediaConsumptionRecordRepository.Returns(_mockMediaConsumptionRecordRepository);
        _mockUnitOfWork.MediaResumePointRepository.Returns(_mockMediaResumePointRepository);
        // Default stubs: saving changes succeeds, the current user is authenticated, the target is accessible and the record is deleted.
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(_userId, Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockMediaConsumptionRecordRepository.DeleteForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);

        _mockValidator = Substitute.For<IValidator<DeleteConsumptionRecordCommand>>();
        _mockValidator.Validate(Arg.Any<DeleteConsumptionRecordCommand>()).Returns([]);

        _sut = new DeleteConsumptionRecordCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotDeleteAnything()
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<DeleteConsumptionRecordCommand>())
            .Returns([DomainErrors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty]);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotDeleteAnything()
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccessToTheTarget_ShouldReturnNotAuthorizedAndNotDeleteAnything()
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create();
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
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRecordDeletionFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption record could not be deleted.");
        _mockMediaConsumptionRecordRepository.DeleteForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
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
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create();
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
    public async Task HandleAsync_WhenSuccessfulForTheCurrentUser_ShouldDeleteTheRecordOfTheCurrentUserAndReturnDeleted()
    {
        // Arrange
        Guid consumptionRecordId = Guid.NewGuid();
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create(
            consumptionRecordId: consumptionRecordId.ToString(),
            includeTargetUserId: false);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        await _mockMediaConsumptionRecordRepository.Received(1).DeleteForUserAsync(consumptionRecordId, _userId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserInLibraryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTargetUserIsSupplied_ShouldDeleteTheRecordOfTheTargetUser()
    {
        // Arrange
        Guid consumptionRecordId = Guid.NewGuid();
        Guid targetUserId = Guid.NewGuid();
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create(
            consumptionRecordId: consumptionRecordId.ToString(),
            targetUserId: targetUserId.ToString());

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Is<ConsumptionActivityAccessPolicyContext>(context => context.TargetUserId == targetUserId),
            Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).DeleteForUserAsync(consumptionRecordId, targetUserId, Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteForUserAsync(consumptionRecordId, _userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheCancellationTokenToEveryDependency()
    {
        // Arrange
        DeleteConsumptionRecordCommand command = _deleteConsumptionRecordCommandFixture.Create(includeTargetUserId: false);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Any<ConsumptionActivityAccessPolicyContext>(),
            Arg.Is(cancellationToken));
        await _mockMediaConsumptionRecordRepository.Received(1).DeleteForUserAsync(Arg.Any<Guid>(), _userId, Arg.Is(cancellationToken));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
    }
}
