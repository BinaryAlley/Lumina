#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Common.DataAccess.Repositories.BookLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetDeletedBooks;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetDeletedBooks;
using Nemera.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetDeletedBooks;

/// <summary>
/// Contains unit tests for the <see cref="GetDeletedBooksQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedBooksQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetDeletedBooksQuery> _mockValidator;
    private readonly GetDeletedBooksQueryHandler _sut;
    private readonly Guid _userId;
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();
    private readonly GetDeletedBooksQueryFixture _getDeletedBooksQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedBooksQueryHandlerTests"/> class.
    /// </summary>
    public GetDeletedBooksQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetDeletedBooksQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the query is valid, the current user is authenticated and the library ownership policy allows access.
        _mockValidator.Validate(Arg.Any<GetDeletedBooksQuery>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockBookRepository.GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([]));

        _sut = new GetDeletedBooksQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenDeletedBooksExist_ShouldReturnTheirMappedResponses()
    {
        // Arrange
        GetDeletedBooksQuery query = _getDeletedBooksQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        string coverFileName = $"covers/{Guid.NewGuid()}.jpg";
        BookEntity firstBook = _bookEntityFixture.Create(libraryId: libraryId, isDeleted: true, includeMetadata: false);
        firstBook.Artwork = [_bookArtworkEntityFixture.Create(bookId: firstBook.Id, artworkType: ArtworkType.Cover, ordinal: 0, fileName: coverFileName)];
        BookEntity secondBook = _bookEntityFixture.Create(libraryId: libraryId, isDeleted: true, includeMetadata: false);
        _mockBookRepository.GetSoftDeletedByLibraryIdAsync(libraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>([firstBook, secondBook]));

        // Act
        Result<IReadOnlyList<DeletedBookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(firstBook.Id, result.Value[0].Id);
        Assert.Equal(firstBook.LibraryId, result.Value[0].LibraryId);
        Assert.Equal(firstBook.Title, result.Value[0].Title);
        Assert.Equal(firstBook.OriginalTitle, result.Value[0].OriginalTitle);
        Assert.Equal(firstBook.Path, result.Value[0].Path);
        Assert.Equal(firstBook.DeletedOnUtc ?? firstBook.CreatedOnUtc, result.Value[0].DeletedOnUtc);
        Assert.Equal(coverFileName, result.Value[0].CoverPath);
        Assert.Equal(secondBook.Id, result.Value[1].Id);
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoDeletedBooks_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        GetDeletedBooksQuery query = _getDeletedBooksQueryFixture.Create();

        // Act
        Result<IReadOnlyList<DeletedBookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        GetDeletedBooksQuery query = _getDeletedBooksQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetDeletedBooksQuery>())
            .Returns([DomainErrors.Library.LibraryIdCannotBeEmpty]);

        // Act
        Result<IReadOnlyList<DeletedBookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetDeletedBooksQuery query = _getDeletedBooksQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<DeletedBookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetDeletedBooksQuery query = _getDeletedBooksQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IReadOnlyList<DeletedBookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookRepositoryFails_ShouldReturnFailureResult()
    {
        // Arrange
        GetDeletedBooksQuery query = _getDeletedBooksQueryFixture.Create();
        _mockBookRepository.GetSoftDeletedByLibraryIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<IReadOnlyList<DeletedBookResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
    }
}
