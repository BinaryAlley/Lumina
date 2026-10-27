#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionRecord;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetConsumptionRecord;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetConsumptionRecord;

/// <summary>
/// Contains unit tests for the <see cref="GetConsumptionRecordQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionRecordQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IMediaConsumptionRecordRepository _mockMediaConsumptionRecordRepository;
    private readonly IValidator<GetConsumptionRecordQuery> _mockValidator;
    private readonly GetConsumptionRecordQueryHandler _sut;
    private readonly GetConsumptionRecordQueryFixture _getConsumptionRecordQueryFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionRecordQueryHandlerTests"/> class.
    /// </summary>
    public GetConsumptionRecordQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockMediaConsumptionRecordRepository = Substitute.For<IMediaConsumptionRecordRepository>();

        _mockUnitOfWork.MediaConsumptionRecordRepository.Returns(_mockMediaConsumptionRecordRepository);
        // Default stub: the current user is authenticated.
        _mockCurrentUserService.UserId.Returns(_userId);

        _mockValidator = Substitute.For<IValidator<GetConsumptionRecordQuery>>();
        _mockValidator.Validate(Arg.Any<GetConsumptionRecordQuery>()).Returns([]);

        _sut = new GetConsumptionRecordQueryHandler(_mockUnitOfWork, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadTheRecord()
    {
        // Arrange
        GetConsumptionRecordQuery query = _getConsumptionRecordQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetConsumptionRecordQuery>())
            .Returns([DomainErrors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty]);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadTheRecord()
    {
        // Arrange
        GetConsumptionRecordQuery query = _getConsumptionRecordQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockMediaConsumptionRecordRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRecordReadFails_ShouldReturnThatError()
    {
        // Arrange
        GetConsumptionRecordQuery query = _getConsumptionRecordQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption record could not be read.");
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenRecordDoesNotExist_ShouldReturnConsumptionRecordNotFound()
    {
        // Arrange
        GetConsumptionRecordQuery query = _getConsumptionRecordQueryFixture.Create();
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(null));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.ConsumptionRecordNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReturnTheRecordOfTheCurrentUserMappedToItsResponse()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime startedOnUtc = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        DateTime lastActivityOnUtc = new(2026, 1, 2, 4, 5, 6, DateTimeKind.Utc);
        DateTime endedOnUtc = new(2026, 1, 2, 5, 6, 7, DateTimeKind.Utc);
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.Completed,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            endedOnUtc: endedOnUtc,
            consumedDurationMilliseconds: 90_500,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 30_000,
            lastProgressFraction: 0.5,
            consecutivePagesRead: 0);
        GetConsumptionRecordQuery query = _getConsumptionRecordQueryFixture.Create(
            consumptionRecordId: recordId.ToString());
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(record));
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(query, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        ConsumptionRecordResponse response = result.Value;
        Assert.Equal(recordId, response.Id);
        Assert.Equal(libraryId, response.LibraryId);
        Assert.Equal(mediaItemId, response.MediaItemId);
        Assert.Equal(MediaKind.MusicTrack, response.MediaKind);
        Assert.Equal(ConsumptionStatus.Completed, response.Status);
        Assert.Equal(startedOnUtc, response.StartedOnUtc);
        Assert.Equal(lastActivityOnUtc, response.LastActivityOnUtc);
        Assert.Equal(endedOnUtc, response.EndedOnUtc);
        Assert.Equal(90.5, response.ConsumedDurationInSeconds);
        Assert.NotNull(response.LastPosition);
        Assert.Equal(MediaPositionKind.TimeOffset, response.LastPosition.Kind);
        Assert.Equal(30d, response.LastPosition.TimeOffsetSeconds!.Value);
        Assert.Null(response.LastPosition.Location);
        Assert.Equal(0.5, response.LastProgressFraction);
        Assert.Equal(0, response.ConsecutivePagesRead);
        await _mockMediaConsumptionRecordRepository.Received(1).GetByIdForUserAsync(
            recordId,
            _userId,
            shouldTrackEntities: false,
            cancellationToken: Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecordHasNoEndTimeAndNoPosition_ShouldMapThemAsNull()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress,
            includeEndedOnUtc: false,
            includeLastPositionKind: false);
        GetConsumptionRecordQuery query = _getConsumptionRecordQueryFixture.Create(
            consumptionRecordId: recordId.ToString());
        _mockMediaConsumptionRecordRepository.GetByIdForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<MediaConsumptionRecordEntity?>(record));

        // Act
        Result<ConsumptionRecordResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value.EndedOnUtc);
        Assert.Null(result.Value.LastPosition);
    }
}
