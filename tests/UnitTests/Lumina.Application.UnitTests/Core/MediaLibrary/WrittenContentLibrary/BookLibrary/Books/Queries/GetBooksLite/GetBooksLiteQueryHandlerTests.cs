#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;

/// <summary>
/// Contains unit tests for the <see cref="GetBooksLiteQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetBooksLiteQuery> _mockValidator;
    private readonly GetBooksLiteQueryHandler _sut;
    private readonly Guid _userId;
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly GetBooksLiteQueryFixture _getBooksLiteQueryFixture = new();
    private readonly PaginatedResultDtoFixture<BookEntity> _paginatedResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksLiteQueryHandlerTests"/> class.
    /// </summary>
    public GetBooksLiteQueryHandlerTests()
    {
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetBooksLiteQuery>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and the library ownership policy allows access
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetBooksLiteQuery>()).Returns([]);

        _sut = new GetBooksLiteQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyAllowsAccess_ShouldReturnMappedPaginatedResponses()
    {
        // Arrange
        GetBooksLiteQuery query = _getBooksLiteQueryFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<BookEntity> bookEntities = _bookEntityFixture.CreateMany(2);
        PaginatedResultDto<BookEntity> paginatedBooks = _paginatedResultDtoFixture.Create(data: bookEntities, currentPage: 1, perPage: 10, count: 2, numberOfPages: 1);
        _mockBookRepository.GetPaginatedAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedBooks));

        // Act
        Result<PaginatedResponse<BookLiteResponse>> result = await _sut.HandleAsync(query, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(bookEntities.Count, result.Value.Data.Count);
        Assert.Equal(paginatedBooks.CurrentPage, result.Value.CurrentPage);
        Assert.Equal(paginatedBooks.PerPage, result.Value.PerPage);
        Assert.Equal(paginatedBooks.Count, result.Value.Count);
        Assert.Equal(paginatedBooks.NumberOfPages, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsEmptyPage_ShouldReturnEmptyPaginatedResponses()
    {
        // Arrange
        GetBooksLiteQuery query = _getBooksLiteQueryFixture.Create();
        PaginatedResultDto<BookEntity> paginatedBooks = _paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 10, count: 0, numberOfPages: 0);
        _mockBookRepository.GetPaginatedAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedBooks));

        // Act
        Result<PaginatedResponse<BookLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.Data);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetBooksLiteQuery query = _getBooksLiteQueryFixture.Create();
        _mockBookRepository.GetPaginatedAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Errors.Library.FilterMustIncludeLibraryId);

        // Act
        Result<PaginatedResponse<BookLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldQueryRepositoryWithMappedFilterAndPagination()
    {
        // Arrange
        GetBooksLiteQuery query = _getBooksLiteQueryFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PaginatedResultDto<BookEntity> paginatedBooks = _paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 10, count: 0, numberOfPages: 0);
        _mockBookRepository.GetPaginatedAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedBooks));

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockBookRepository.Received(1).GetPaginatedAsync(
            Arg.Is<PaginationDataDto>(paginationData => paginationData.CurrentPage == query.PaginationData!.CurrentPage &&
                                                        paginationData.PerPage == query.PaginationData.PerPage),
            Arg.Is(query.SortBy),
            Arg.Is(query.SortOrder),
            Arg.Is<LibraryFilterDto>(filter => filter.LibraryId == query.Filter.LibraryId && filter.SearchTerm == query.Filter.SearchTerm),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenQueryHasNoPaginationData_ShouldQueryRepositoryWithoutPagination()
    {
        // Arrange
        GetBooksLiteQuery query = _getBooksLiteQueryFixture.Create(includePaginationData: false, libraryId: Guid.NewGuid(), sortOrder: SortOrder.Ascending);
        PaginatedResultDto<BookEntity> paginatedBooks = _paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 10, count: 0, numberOfPages: 0);
        _mockBookRepository.GetPaginatedAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedBooks));

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockBookRepository.Received(1).GetPaginatedAsync(
            Arg.Is<PaginationDataDto?>(paginationData => paginationData == null),
            Arg.Is(query.SortBy),
            Arg.Is(query.SortOrder),
            Arg.Is<LibraryFilterDto>(filter => filter.LibraryId == query.Filter.LibraryId && filter.SearchTerm == query.Filter.SearchTerm),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetBooksLiteQuery query = _getBooksLiteQueryFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PaginatedResponse<BookLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == query.Filter.LibraryId), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetPaginatedAsync(Arg.Any<PaginationDataDto?>(), Arg.Any<string?>(), Arg.Any<SortOrder?>(), Arg.Any<LibraryFilterDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetBooksLiteQuery query = _getBooksLiteQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PaginatedResponse<BookLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetPaginatedAsync(Arg.Any<PaginationDataDto?>(), Arg.Any<string?>(), Arg.Any<SortOrder?>(), Arg.Any<LibraryFilterDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        GetBooksLiteQuery query = _getBooksLiteQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetBooksLiteQuery>()).Returns([Errors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<PaginatedResponse<BookLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetPaginatedAsync(Arg.Any<PaginationDataDto?>(), Arg.Any<string?>(), Arg.Any<SortOrder?>(), Arg.Any<LibraryFilterDto>(), Arg.Any<CancellationToken>());
    }
}
