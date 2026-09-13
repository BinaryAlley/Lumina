#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;

/// <summary>
/// Contains unit tests for the <see cref="GetBookQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetBookQuery> _mockValidator;
    private readonly GetBookQueryHandler _sut;
    private readonly Guid _userId;
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookContributorEntityFixture _bookContributorEntityFixture = new();
    private readonly GetBookQueryFixture _getBookQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookQueryHandlerTests"/> class.
    /// </summary>
    public GetBookQueryHandlerTests()
    {
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetBookQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated and the library ownership policy allows access.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetBookQuery>()).Returns([]);

        _sut = new GetBookQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenBookExistsWithoutContributors_ShouldReturnMappedBookResponse()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid bookId = Guid.Parse(query.BookId!);
        BookEntity bookEntity = _bookEntityFixture.Create(id: bookId, libraryId: libraryId);
        _mockBookRepository.GetByIdAsync(bookId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(bookEntity));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(bookId, result.Value.Id);
        Assert.Equal(bookEntity.Title, result.Value.Metadata!.Title);
        Assert.NotNull(result.Value.Contributors);
        Assert.Empty(result.Value.Contributors!);
    }

    [Fact]
    public async Task HandleAsync_WhenBookHasContributors_ShouldReturnResponseWithContributorReferences()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid bookId = Guid.Parse(query.BookId!);
        BookEntity bookEntity = _bookEntityFixture.Create(id: bookId, libraryId: libraryId);
        Guid contributorId = Guid.NewGuid();
        bookEntity.BookContributors =
        [
            _bookContributorEntityFixture.Create(bookId: bookId, mediaContributorId: contributorId, role: MediaContributorRole.Author)
        ];
        _mockBookRepository.GetByIdAsync(bookId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(bookEntity));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value.Contributors);
        Assert.Single(result.Value.Contributors!);
        Assert.Equal(contributorId, result.Value.Contributors![0].ContributorId);
        Assert.Equal(MediaContributorRole.Author, result.Value.Contributors![0].Role);
    }

    [Fact]
    public async Task HandleAsync_WhenBookNotFound_ShouldReturnBookNotFoundError()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();
        Guid bookId = Guid.Parse(query.BookId!);
        _mockBookRepository.GetByIdAsync(bookId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(null));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookBelongsToAnotherLibrary_ShouldReturnBookNotFoundError()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();
        Guid bookId = Guid.Parse(query.BookId!);
        BookEntity bookEntity = _bookEntityFixture.Create(id: bookId, libraryId: Guid.NewGuid());
        _mockBookRepository.GetByIdAsync(bookId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(bookEntity));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();
        Guid bookId = Guid.Parse(query.BookId!);
        _mockBookRepository.GetByIdAsync(bookId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid bookId = Guid.Parse(query.BookId!);
        BookEntity bookEntity = _bookEntityFixture.Create(id: bookId, libraryId: libraryId);
        _mockBookRepository.GetByIdAsync(bookId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(bookEntity));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == bookEntity.LibraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetBookQuery>()).Returns([Errors.WrittenContent.BookIdCannotBeEmpty]);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookIdCannotBeEmpty, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }
}
