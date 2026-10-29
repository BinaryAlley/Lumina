#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;
using Lumina.Application.Fixtures.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;
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

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;

/// <summary>
/// Contains unit tests for the <see cref="DeleteConsumptionItemHistoryCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionItemHistoryCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IMediaConsumptionRecordRepository _mockMediaConsumptionRecordRepository;
    private readonly IMediaResumePointRepository _mockMediaResumePointRepository;
    private readonly IValidator<DeleteConsumptionItemHistoryCommand> _mockValidator;
    private readonly DeleteConsumptionItemHistoryCommandHandler _sut;
    private readonly DeleteConsumptionItemHistoryCommandFixture _deleteConsumptionItemHistoryCommandFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionItemHistoryCommandHandlerTests"/> class.
    /// </summary>
    public DeleteConsumptionItemHistoryCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockMediaConsumptionRecordRepository = Substitute.For<IMediaConsumptionRecordRepository>();
        _mockMediaResumePointRepository = Substitute.For<IMediaResumePointRepository>();

        _mockUnitOfWork.MediaConsumptionRecordRepository.Returns(_mockMediaConsumptionRecordRepository);
        _mockUnitOfWork.MediaResumePointRepository.Returns(_mockMediaResumePointRepository);
        // Default stubs: saving changes succeeds, the current user is authenticated, the target is accessible and the records are deleted.
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockMediaConsumptionRecordRepository.DeleteAllByMediaItemIdForUserAsync(
            Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);

        _mockValidator = Substitute.For<IValidator<DeleteConsumptionItemHistoryCommand>>();
        _mockValidator.Validate(Arg.Any<DeleteConsumptionItemHistoryCommand>()).Returns([]);

        _sut = new DeleteConsumptionItemHistoryCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotDeleteAnything()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<DeleteConsumptionItemHistoryCommand>())
            .Returns([DomainErrors.MediaConsumption.MediaItemIdCannotBeEmpty]);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.MediaItemIdCannotBeEmpty, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllByMediaItemIdForUserAsync(
            Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotDeleteAnything()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllByMediaItemIdForUserAsync(
            Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccessToTheTarget_ShouldReturnNotAuthorizedAndNotDeleteAnything()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
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
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllByMediaItemIdForUserAsync(
            Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecordsCannotBeDeleted_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption records could not be deleted.");
        _mockMediaConsumptionRecordRepository.DeleteAllByMediaItemIdForUserAsync(
            Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
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
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create();
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
    public async Task HandleAsync_WhenTheTargetUserIsNotSupplied_ShouldDeleteTheHistoryOfTheCurrentUser()
    {
        // Arrange
        Guid mediaItemId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(
            includeTargetUserId: false,
            mediaKind: nameof(MediaKind.Movie),
            mediaItemId: mediaItemId.ToString(),
            libraryId: libraryId.ToString());

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Is<ConsumptionActivityAccessPolicyContext>(context => context.TargetUserId == _userId),
            Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).DeleteAllByMediaItemIdForUserAsync(
            _userId,
            MediaKind.Movie,
            mediaItemId,
            libraryId,
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTargetUserIsSupplied_ShouldDeleteTheHistoryOfTheTargetUser()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(
            targetUserId: targetUserId.ToString(),
            mediaKind: nameof(MediaKind.Book),
            mediaItemId: mediaItemId.ToString(),
            libraryId: libraryId.ToString());

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Is<ConsumptionActivityAccessPolicyContext>(context => context.TargetUserId == targetUserId),
            Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.Received(1).DeleteAllByMediaItemIdForUserAsync(
            targetUserId,
            MediaKind.Book,
            mediaItemId,
            libraryId,
            Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllByMediaItemIdForUserAsync(
            _userId, Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldNotUseAnyOtherRepository()
    {
        // Arrange
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(includeTargetUserId: false);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaConsumptionRecordRepository.DidNotReceive().DeleteAllForUserAsync(
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteForUserAndMediaItemAsync(
            Arg.Any<Guid>(), Arg.Any<MediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserAsync(
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMediaResumePointRepository.DidNotReceive().DeleteAllForUserInLibraryAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheCancellationTokenToEveryDependency()
    {
        // Arrange
        Guid mediaItemId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        DeleteConsumptionItemHistoryCommand command = _deleteConsumptionItemHistoryCommandFixture.Create(
            includeTargetUserId: false,
            mediaKind: nameof(MediaKind.TvEpisode),
            mediaItemId: mediaItemId.ToString(),
            libraryId: libraryId.ToString());
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Any<ConsumptionActivityAccessPolicyContext>(),
            Arg.Is(cancellationToken));
        await _mockMediaConsumptionRecordRepository.Received(1).DeleteAllByMediaItemIdForUserAsync(
            _userId,
            MediaKind.TvEpisode,
            mediaItemId,
            libraryId,
            Arg.Is(cancellationToken));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
    }
}
