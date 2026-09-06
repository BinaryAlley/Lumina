#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetBookQuery> _mockValidator;
    private readonly GetBookQueryHandler _sut;
    private readonly Guid _userId;
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly BookContributorEntityFixture _bookContributorEntityFixture = new();
    private readonly GetBookQueryFixture _getBookQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookQueryHandlerTests"/> class.
    /// </summary>
    public GetBookQueryHandlerTests()
    {
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);
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
        BookEntity bookEntity = _bookEntityFixture.Create(id: query.Id);
        _mockBookRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(bookEntity));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(query.Id, result.Value.Id);
        Assert.Equal(bookEntity.Title, result.Value.Metadata!.Title);
        Assert.Null(result.Value.Contributors);
        await _mockMediaContributorRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookHasContributors_ShouldReturnResponseWithResolvedContributorNames()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();
        BookEntity bookEntity = _bookEntityFixture.Create(id: query.Id);
        MediaContributorEntity contributor = _mediaContributorEntityFixture.Create();
        bookEntity.BookContributors =
        [
            _bookContributorEntityFixture.Create(bookId: query.Id, mediaContributorId: contributor.Id, roleName: "author", roleCategory: MediaContributorRoleCategory.Author)
        ];
        _mockBookRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(bookEntity));
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaContributorEntity>>([contributor]));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value.Contributors);
        Assert.Single(result.Value.Contributors!);
        Assert.Equal(contributor.DisplayName, result.Value.Contributors![0].Name!.DisplayName);
        Assert.Equal("author", result.Value.Contributors![0].Role!.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenBookNotFound_ShouldReturnBookNotFoundError()
    {
        // Arrange
        GetBookQuery query = _getBookQueryFixture.Create();
        _mockBookRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(null));

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
        _mockBookRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
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
        BookEntity bookEntity = _bookEntityFixture.Create(id: query.Id);
        _mockBookRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
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
        await _mockMediaContributorRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
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
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
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
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }
}
