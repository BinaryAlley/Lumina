#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtists;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtists;

/// <summary>
/// Contains unit tests for the <see cref="GetArtistsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetArtistsQuery> _mockValidator;
    private readonly GetArtistsQueryHandler _sut;
    private readonly Guid _userId;
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly GetArtistsQueryFixture _getArtistsQueryFixture = new();
    private readonly PaginatedResultDtoFixture<ArtistEntity> _paginatedResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsQueryHandlerTests"/> class.
    /// </summary>
    public GetArtistsQueryHandlerTests()
    {
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetArtistsQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access and the validator passes.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetArtistsQuery>()).Returns([]);

        _sut = new GetArtistsQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyAllowsAccess_ShouldReturnMappedPaginatedResponses()
    {
        // Arrange
        GetArtistsQuery query = _getArtistsQueryFixture.Create();
        List<ArtistEntity> artistEntities = _artistEntityFixture.CreateMany(2);
        PaginatedResultDto<ArtistEntity> paginatedArtists = _paginatedResultDtoFixture.Create(data: artistEntities, currentPage: 1, perPage: 10, count: 2, numberOfPages: 1);
        _mockArtistRepository.GetAllAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                shouldTrackEntities: false,
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedArtists));

        // Act
        Result<PaginatedResponse<ArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(artistEntities.Count, result.Value.Data.Count);
        Assert.Equal(paginatedArtists.CurrentPage, result.Value.CurrentPage);
        Assert.Equal(paginatedArtists.PerPage, result.Value.PerPage);
        Assert.Equal(paginatedArtists.Count, result.Value.Count);
        Assert.Equal(paginatedArtists.NumberOfPages, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsEmptyPage_ShouldReturnEmptyPaginatedResponses()
    {
        // Arrange
        GetArtistsQuery query = _getArtistsQueryFixture.Create();
        PaginatedResultDto<ArtistEntity> paginatedArtists = _paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 10, count: 0, numberOfPages: 0);
        _mockArtistRepository.GetAllAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                shouldTrackEntities: false,
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedArtists));

        // Act
        Result<PaginatedResponse<ArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.Data);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetArtistsQuery query = _getArtistsQueryFixture.Create();
        _mockArtistRepository.GetAllAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                shouldTrackEntities: false,
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.FilterMustIncludeLibraryId);

        // Act
        Result<PaginatedResponse<ArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldQueryRepositoryWithMappedFilterPaginationAndSearchTerm()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        GetArtistsQuery query = _getArtistsQueryFixture.Create(libraryId: libraryId.ToString(), searchTerm: "Queen");
        CancellationToken cancellationToken = CancellationToken.None;
        PaginatedResultDto<ArtistEntity> paginatedArtists = _paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 10, count: 0, numberOfPages: 0);
        _mockArtistRepository.GetAllAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                shouldTrackEntities: false,
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedArtists));

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockArtistRepository.Received(1).GetAllAsync(
            Arg.Is<PaginationDataDto>(paginationData => paginationData.CurrentPage == query.PaginationData!.CurrentPage && paginationData.PerPage == query.PaginationData.PerPage),
            Arg.Any<string?>(),
            Arg.Any<SortOrder?>(),
            Arg.Is<LibraryFilterDto>(filter => filter.LibraryId == libraryId && filter.SearchTerm == "Queen"),
            shouldTrackEntities: false,
            cancellationToken: Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenQueryHasNoPaginationData_ShouldQueryRepositoryWithoutPagination()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        GetArtistsQuery query = _getArtistsQueryFixture.Create(libraryId: libraryId.ToString(), includePaginationData: false, searchTerm: "Queen");
        PaginatedResultDto<ArtistEntity> paginatedArtists = _paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 10, count: 0, numberOfPages: 0);
        _mockArtistRepository.GetAllAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                shouldTrackEntities: false,
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedArtists));

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockArtistRepository.Received(1).GetAllAsync(
            Arg.Is<PaginationDataDto?>(paginationData => paginationData == null),
            Arg.Any<string?>(),
            Arg.Any<SortOrder?>(),
            Arg.Is<LibraryFilterDto>(filter => filter.LibraryId == libraryId && filter.SearchTerm == "Queen"),
            shouldTrackEntities: false,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotOwnTheLibrary_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        GetArtistsQuery query = _getArtistsQueryFixture.Create(libraryId: libraryId.ToString());
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PaginatedResponse<ArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().GetAllAsync(Arg.Any<PaginationDataDto?>(), Arg.Any<string?>(), Arg.Any<SortOrder?>(), Arg.Any<LibraryFilterDto>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetArtistsQuery query = _getArtistsQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PaginatedResponse<ArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().GetAllAsync(Arg.Any<PaginationDataDto?>(), Arg.Any<string?>(), Arg.Any<SortOrder?>(), Arg.Any<LibraryFilterDto>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        GetArtistsQuery query = _getArtistsQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetArtistsQuery>()).Returns([Errors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<PaginatedResponse<ArtistResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().GetAllAsync(Arg.Any<PaginationDataDto?>(), Arg.Any<string?>(), Arg.Any<SortOrder?>(), Arg.Any<LibraryFilterDto>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
    }
}
