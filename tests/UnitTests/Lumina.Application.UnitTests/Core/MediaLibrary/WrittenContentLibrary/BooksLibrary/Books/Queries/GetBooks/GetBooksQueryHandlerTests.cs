#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;

/// <summary>
/// Contains unit tests for the <see cref="GetBooksQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetBooksQuery> _mockValidator;
    private readonly GetBooksQueryHandler _sut;
    private readonly Guid _userId;
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly GetBooksQueryFixture _getBooksQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksQueryHandlerTests"/> class.
    /// </summary>
    public GetBooksQueryHandlerTests()
    {
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetBooksQuery>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is not an admin, but owns the requested library
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From((LibraryEntity?)new LibraryEntity
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                Title = "Test Library",
                LibraryType = LibraryType.EBook,
                ContentLocations = [],
                CreatedBy = _userId,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedBy = null
            }));
        _mockValidator.Validate(Arg.Any<GetBooksQuery>()).Returns([]);

        _sut = new GetBooksQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsPaginatedBooks_ShouldReturnMappedPaginatedResponses()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<BookEntity> bookEntities = _bookEntityFixture.CreateMany(2);
        PaginatedResultDto<BookEntity> paginatedBooks = new()
        {
            Data = bookEntities,
            CurrentPage = 1,
            PerPage = 10,
            Count = 2,
            NumberOfPages = 1
        };
        _mockBookRepository.GetPaginatedAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedBooks));

        // Act
        Result<PaginatedResponse<BookResponse>> result = await _sut.HandleAsync(query, cancellationToken);

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
        GetBooksQuery query = _getBooksQueryFixture.Create();
        PaginatedResultDto<BookEntity> paginatedBooks = new()
        {
            Data = [],
            CurrentPage = 1,
            PerPage = 10,
            Count = 0,
            NumberOfPages = 0
        };
        _mockBookRepository.GetPaginatedAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedBooks));

        // Act
        Result<PaginatedResponse<BookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.Data);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create();
        _mockBookRepository.GetPaginatedAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Errors.Library.FilterMustIncludeLibraryId);

        // Act
        Result<PaginatedResponse<BookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldQueryRepositoryWithMappedFilterAndPagination()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PaginatedResultDto<BookEntity> paginatedBooks = new()
        {
            Data = [],
            CurrentPage = 1,
            PerPage = 10,
            Count = 0,
            NumberOfPages = 0
        };
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
        GetBooksQuery query = new(
            PaginationData: null,
            Filter: new LibraryFilterDto { LibraryId = Guid.NewGuid() },
            SortBy: null,
            SortOrder: SortOrder.Ascending
        );
        PaginatedResultDto<BookEntity> paginatedBooks = new()
        {
            Data = [],
            CurrentPage = 1,
            PerPage = 10,
            Count = 0,
            NumberOfPages = 0
        };
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
    public async Task HandleAsync_WhenUserIsAdmin_ShouldBypassLibraryOwnershipCheck()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);
        PaginatedResultDto<BookEntity> paginatedBooks = new()
        {
            Data = [],
            CurrentPage = 1,
            PerPage = 10,
            Count = 0,
            NumberOfPages = 0
        };
        _mockBookRepository.GetPaginatedAsync(
                Arg.Any<PaginationDataDto?>(),
                Arg.Any<string?>(),
                Arg.Any<SortOrder?>(),
                Arg.Any<LibraryFilterDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedBooks));

        // Act
        Result<PaginatedResponse<BookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockUnitOfWork.LibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotOwnLibrary_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create();
        Guid otherUserId = Guid.NewGuid();
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From((LibraryEntity?)new LibraryEntity
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId,
                Title = "Someone else's library",
                LibraryType = LibraryType.EBook,
                ContentLocations = [],
                CreatedBy = otherUserId,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedBy = null
            }));

        // Act
        Result<PaginatedResponse<BookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetPaginatedAsync(Arg.Any<PaginationDataDto?>(), Arg.Any<string?>(), Arg.Any<SortOrder?>(), Arg.Any<LibraryFilterDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create();
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<LibraryEntity?>.Success(null));

        // Act
        Result<PaginatedResponse<BookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetPaginatedAsync(Arg.Any<PaginationDataDto?>(), Arg.Any<string?>(), Arg.Any<SortOrder?>(), Arg.Any<LibraryFilterDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        GetBooksQuery query = _getBooksQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetBooksQuery>()).Returns([Errors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<PaginatedResponse<BookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetPaginatedAsync(Arg.Any<PaginationDataDto?>(), Arg.Any<string?>(), Arg.Any<SortOrder?>(), Arg.Any<LibraryFilterDto>(), Arg.Any<CancellationToken>());
    }
}
