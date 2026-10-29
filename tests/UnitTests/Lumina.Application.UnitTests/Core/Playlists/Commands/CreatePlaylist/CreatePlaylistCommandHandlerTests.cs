#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Playlists.Commands.CreatePlaylist;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Common.DTO.Playlists;
using Lumina.Application.Fixtures.Core.Playlists.Commands.CreatePlaylist;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.CreatePlaylist;

/// <summary>
/// Contains unit tests for the <see cref="CreatePlaylistCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IPlaylistRepository _mockPlaylistRepository;
    private readonly IValidator<CreatePlaylistCommand> _mockValidator;
    private readonly CreatePlaylistCommandHandler _sut;
    private readonly CreatePlaylistCommandFixture _createPlaylistCommandFixture = new();
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PlaylistDetailsDtoFixture _playlistDetailsDtoFixture = new();
    private readonly PaginatedResultDtoFixture<PlaylistEntryDto> _paginatedResultDtoFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistCommandHandlerTests"/> class.
    /// </summary>
    public CreatePlaylistCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockPlaylistRepository = Substitute.For<IPlaylistRepository>();

        _mockUnitOfWork.PlaylistRepository.Returns(_mockPlaylistRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockPlaylistRepository.InsertAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.From<PlaylistDetailsDto?>(_playlistDetailsDtoFixture.Create(
                playlist: _playlistEntityFixture.Create(id: callInfo.ArgAt<Guid>(0), userId: _userId))));

        _mockValidator = Substitute.For<IValidator<CreatePlaylistCommand>>();
        _mockValidator.Validate(Arg.Any<CreatePlaylistCommand>()).Returns([]);

        _sut = new CreatePlaylistCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockDomainEventsQueue, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotTouchTheRepositories()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create();
        Error error = DomainErrors.Playlists.PlaylistNameCannotBeEmpty;
        _mockValidator.Validate(Arg.Any<CreatePlaylistCommand>()).Returns([error]);

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockPlaylistRepository.DidNotReceive().InsertAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotInsert()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().InsertAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The playlist could not be inserted.");
        _mockPlaylistRepository.InsertAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFails_ShouldReturnThatErrorAndNotRaiseAnEvent()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The changes could not be saved.");
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheDetailsReadFails_ShouldReturnThatError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The created playlist could not be read.");
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheDetailsAreMissing_ShouldReturnPlaylistNotFound()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create();
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistDetailsDto?>(null));

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldInsertSaveEnqueueOnceReadTheDefaultPageAndReturnThePersistedPlaylist()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create(
            name: "Road Trip",
            description: "Driving songs",
            type: PlaylistType.Music);
        PlaylistEntryDto entry = _playlistEntryDtoFixture.Create(displayPosition: 0);
        PaginatedResultDto<PlaylistEntryDto> entries = _paginatedResultDtoFixture.Create(
            data: [entry],
            currentPage: 1,
            perPage: 200,
            count: 1,
            numberOfPages: 1);
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.From<PlaylistDetailsDto?>(_playlistDetailsDtoFixture.Create(
                playlist: _playlistEntityFixture.Create(
                    id: callInfo.ArgAt<Guid>(0),
                    userId: _userId,
                    name: command.Name!,
                    description: command.Description,
                    type: command.Type!.Value),
                entries: entries)));
        PlaylistEntity? insertedEntity = null;
        _mockPlaylistRepository.InsertAsync(Arg.Do<PlaylistEntity>(playlist => insertedEntity = playlist), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(insertedEntity);
        Assert.NotEqual(Guid.Empty, insertedEntity!.Id);
        Assert.Equal(_userId, insertedEntity.UserId);
        Assert.Equal("Road Trip", insertedEntity.Name);
        Assert.Equal("Driving songs", insertedEntity.Description);
        Assert.Equal(PlaylistType.Music, insertedEntity.Type);
        Assert.Equal(0, insertedEntity.EntryCount);
        Assert.Equal(Guid.Empty, insertedEntity.CreatedBy);
        Assert.Null(insertedEntity.UpdatedOnUtc);
        Assert.Null(insertedEntity.UpdatedBy);
        Assert.Equal(insertedEntity.Id, result.Value.Id);
        Assert.Equal("Road Trip", result.Value.Name);
        Assert.Equal("Driving songs", result.Value.Description);
        Assert.Equal(PlaylistType.Music, result.Value.Type);
        Assert.Single(result.Value.Entries.Data);
        Assert.Equal(1, result.Value.Entries.CurrentPage);
        Assert.Equal(200, result.Value.Entries.PerPage);
        await _mockPlaylistRepository.Received(1).InsertAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is PlaylistCreatedDomainEvent));
        await _mockPlaylistRepository.Received(1).GetDetailsForUserAsync(
            insertedEntity.Id,
            _userId,
            Arg.Is<PaginationDataDto>(paginationData => paginationData.CurrentPage == 1 && paginationData.PerPage == 200),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheCancellationTokenToEveryDependency()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create();
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockPlaylistRepository.Received(1).InsertAsync(Arg.Any<PlaylistEntity>(), Arg.Is(cancellationToken));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
        await _mockPlaylistRepository.Received(1).GetDetailsForUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<PaginationDataDto?>(), Arg.Is(cancellationToken));
    }
}
