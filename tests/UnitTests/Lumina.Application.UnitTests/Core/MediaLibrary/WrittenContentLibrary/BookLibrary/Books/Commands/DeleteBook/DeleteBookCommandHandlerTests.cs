#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;
using Lumina.Domain.Common.Primitives;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;

/// <summary>
/// Contains unit tests for the <see cref="DeleteBookCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteBookCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IBookArtworkService _mockBookArtworkService;
    private readonly ILogger<DeleteBookCommandHandler> _mockLogger;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<DeleteBookCommand> _mockValidator;
    private readonly DeleteBookCommandHandler _sut;
    private readonly Guid _userId;
    private readonly DeleteBookCommandFixture _deleteBookCommandFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookCommandHandlerTests"/> class.
    /// </summary>
    public DeleteBookCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockBookArtworkService = Substitute.For<IBookArtworkService>();
        _mockLogger = Substitute.For<ILogger<DeleteBookCommandHandler>>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<DeleteBookCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the command is valid, the current user is authenticated, the ownership policy allows the deletion, the book has no author, the library exists, the artwork deletion succeeds, the book deletion succeeds and saving changes succeeds.
        _mockValidator.Validate(Arg.Any<DeleteBookCommand>()).Returns([]);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(_libraryEntityFixture.Create()));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?>()));
        _mockBookArtworkService.DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result.Deleted);
        _mockBookRepository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new DeleteBookCommandHandler(_mockUnitOfWork, _mockBookArtworkService, _mockLogger, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenBookExists_ShouldDeleteBookAndReturnDeleted()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid bookId = Guid.Parse(command.BookId!);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        _mockBookRepository.GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        // The stored artwork of the book is removed before the book row, so that no artwork file is orphaned.
        Received.InOrder(() =>
        {
            _mockBookArtworkService.DeleteBookArtwork(libraryId, bookId, Arg.Any<string>(), Arg.Any<string>(), book.Title);
            _mockBookRepository.DeleteByIdAsync(bookId, Arg.Any<CancellationToken>());
        });
        await _mockBookRepository.Received(1).DeleteByIdAsync(bookId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookHasAnAuthor_ShouldDeleteTheArtworkUnderTheAuthorSegments()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid bookId = Guid.Parse(command.BookId!);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId);
        _mockBookRepository.GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyDictionary<Guid, string?>>(new Dictionary<Guid, string?> { [bookId] = "J.R.R. Tolkien" }));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockBookArtworkService.Received(1).DeleteBookArtwork(libraryId, bookId, library.Title, "J.R.R. Tolkien", book.Title);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutTouchingAnyRepository()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<DeleteBookCommand>())
            .Returns([DomainErrors.WrittenContent.BookIdCannotBeEmpty]);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.BookIdCannotBeEmpty, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        _mockBookArtworkService.DidNotReceive().DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _mockBookRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookRepositoryFails_ShouldReturnFailureResultWithoutDeleting()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockBookRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnBookNotFoundError()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<BookEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.BookNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookBelongsToAnotherLibrary_ShouldReturnBookNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        Guid bookId = Guid.Parse(command.BookId!);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: Guid.NewGuid(), includeMetadata: false);
        _mockBookRepository.GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // Resource scoping is enforced before authorization, and the mismatch is reported as not found, without disclosing that the book exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.BookNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid bookId = Guid.Parse(command.BookId!);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        _mockBookRepository.GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        _mockBookArtworkService.DidNotReceive().DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _mockBookRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryRepositoryFails_ShouldReturnFailureResultWithoutDeleting()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid bookId = Guid.Parse(command.BookId!);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        _mockBookRepository.GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        _mockBookArtworkService.DidNotReceive().DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _mockBookRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid bookId = Guid.Parse(command.BookId!);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        _mockBookRepository.GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        _mockBookArtworkService.DidNotReceive().DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _mockBookRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAuthorLookupFails_ShouldReturnErrorWithoutDeleting()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid bookId = Guid.Parse(command.BookId!);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        _mockBookRepository.GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockBookRepository.GetAuthorsDisplayNamesByBookIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        _mockBookArtworkService.DidNotReceive().DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _mockBookRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtworkDeletionFails_ShouldStillDeleteTheBook()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid bookId = Guid.Parse(command.BookId!);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        _mockBookRepository.GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockBookArtworkService.DeleteBookArtwork(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Error.Failure(description: "Failed to delete the artwork"));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // A stale cover file must not prevent the book from being removed.
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        await _mockBookRepository.Received(1).DeleteByIdAsync(bookId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteBookFails_ShouldReturnErrorWithoutSaving()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid bookId = Guid.Parse(command.BookId!);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        _mockBookRepository.GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockBookRepository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockBookRepository.Received(1).DeleteByIdAsync(bookId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesFails_ShouldReturnError()
    {
        // Arrange
        DeleteBookCommand command = _deleteBookCommandFixture.Create();
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid bookId = Guid.Parse(command.BookId!);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        _mockBookRepository.GetByIdAsync(bookId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(book));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockBookRepository.Received(1).DeleteByIdAsync(bookId, Arg.Any<CancellationToken>());
    }
}
