#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookCoverCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IBookArtworkService _mockBookArtworkService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<UpdateBookCoverCommand> _mockValidator;
    private readonly UpdateBookCoverCommandHandler _sut;
    private readonly Guid _userId;
    private readonly UpdateBookCoverCommandFixture _commandFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly BookContributorEntityFixture _bookContributorEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverCommandHandlerTests"/> class.
    /// </summary>
    public UpdateBookCoverCommandHandlerTests()
    {
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);
        _mockBookArtworkService = Substitute.For<IBookArtworkService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<UpdateBookCoverCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access,
        // the validator passes and the artwork storage succeeds.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<UpdateBookCoverCommand>()).Returns([]);
        _mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<System.IO.Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("/media/books/cover.jpg"));

        _sut = new UpdateBookCoverCommandHandler(_mockUnitOfWork, _mockBookArtworkService, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenBookHasNoExistingCover_ShouldStoreArtworkAndAddCoverArtwork()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.BookId);
        existingBook.BookArtwork = [];
        LibraryEntity library = _libraryEntityFixture.Create(id: existingBook.LibraryId);
        _mockBookRepository.GetByIdAsync(command.BookId, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockLibraryRepository.GetByIdAsync(existingBook.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<string> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("/media/books/cover.jpg", result.Value);
        Assert.Single(existingBook.BookArtwork);
        Assert.Equal(ArtworkType.Cover, existingBook.BookArtwork[0].ArtworkType);
        Assert.Equal(result.Value, existingBook.BookArtwork[0].FileName);
        Assert.Equal(ArtworkStatus.Enriched, existingBook.BookArtwork[0].Status);
        await _mockBookArtworkService.Received(1).SaveBookArtworkAsync(
            existingBook.LibraryId,
            existingBook.Id,
            library.Title,
            string.Empty,
            existingBook.Title,
            Arg.Any<System.IO.Stream>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookHasExistingCover_ShouldReplaceTheStoredCoverFile()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.BookId);
        BookArtworkEntity existingCover = _bookArtworkEntityFixture.Create(bookId: command.BookId, artworkType: ArtworkType.Cover, fileName: "/media/books/old-cover.jpg");
        existingBook.BookArtwork = [existingCover];
        LibraryEntity library = _libraryEntityFixture.Create(id: existingBook.LibraryId);
        _mockBookRepository.GetByIdAsync(command.BookId, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockLibraryRepository.GetByIdAsync(existingBook.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));

        // Act
        Result<string> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("/media/books/cover.jpg", result.Value);
        Assert.Single(existingBook.BookArtwork);
        Assert.Equal(result.Value, existingCover.FileName);
        Assert.Equal(ArtworkStatus.Enriched, existingCover.Status);
        Assert.Equal(_userId, existingCover.UpdatedBy);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookHasAuthorContributors_ShouldPassTheResolvedAuthorNameToArtworkService()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.BookId);
        MediaContributorEntity author = _mediaContributorEntityFixture.Create(displayName: "J.R.R. Tolkien");
        MediaContributorEntity illustrator = _mediaContributorEntityFixture.Create(displayName: "Alan Lee");
        existingBook.BookContributors =
        [
            _bookContributorEntityFixture.Create(bookId: command.BookId, mediaContributorId: author.Id, roleCategory: MediaContributorRoleCategory.Author),
            _bookContributorEntityFixture.Create(bookId: command.BookId, mediaContributorId: illustrator.Id, roleCategory: MediaContributorRoleCategory.Illustrator)
        ];
        LibraryEntity library = _libraryEntityFixture.Create(id: existingBook.LibraryId);
        _mockBookRepository.GetByIdAsync(command.BookId, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockLibraryRepository.GetByIdAsync(existingBook.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaContributorEntity>>([author, illustrator]));

        // Act
        Result<string> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookArtworkService.Received(1).SaveBookArtworkAsync(
            existingBook.LibraryId,
            existingBook.Id,
            library.Title,
            "J.R.R. Tolkien",
            existingBook.Title,
            Arg.Any<System.IO.Stream>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAuthorResolutionFails_ShouldStoreArtworkWithEmptyAuthorName()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.BookId);
        existingBook.BookContributors =
        [
            _bookContributorEntityFixture.Create(bookId: command.BookId, roleCategory: MediaContributorRoleCategory.Author)
        ];
        LibraryEntity library = _libraryEntityFixture.Create(id: existingBook.LibraryId);
        _mockBookRepository.GetByIdAsync(command.BookId, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockLibraryRepository.GetByIdAsync(existingBook.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<string> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockBookArtworkService.Received(1).SaveBookArtworkAsync(
            existingBook.LibraryId,
            existingBook.Id,
            library.Title,
            string.Empty,
            existingBook.Title,
            Arg.Any<System.IO.Stream>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorFails_ShouldReturnValidationErrorsWithoutStoringArtwork()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();
        _mockValidator.Validate(Arg.Any<UpdateBookCoverCommand>()).Returns([Errors.WrittenContent.BookCoverCannotBeNull]);

        // Act
        Result<string> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookCoverCannotBeNull, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<string> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookArtworkService.DidNotReceive().SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<System.IO.Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnBookNotFoundError()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();
        _mockBookRepository.GetByIdAsync(command.BookId, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(null));

        // Act
        Result<string> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookNotFound, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedErrorWithoutStoringArtwork()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.BookId);
        _mockBookRepository.GetByIdAsync(command.BookId, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<string> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookArtworkService.DidNotReceive().SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<System.IO.Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.BookId);
        _mockBookRepository.GetByIdAsync(command.BookId, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockLibraryRepository.GetByIdAsync(existingBook.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<string> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockBookArtworkService.DidNotReceive().SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<System.IO.Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtworkStorageFails_ShouldReturnFailureResultWithoutSaving()
    {
        // Arrange
        UpdateBookCoverCommand command = _commandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.BookId);
        LibraryEntity library = _libraryEntityFixture.Create(id: existingBook.LibraryId);
        _mockBookRepository.GetByIdAsync(command.BookId, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockLibraryRepository.GetByIdAsync(existingBook.LibraryId, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(library));
        _mockBookArtworkService.SaveBookArtworkAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<System.IO.Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Errors.FileSystemManagement.FileTooLarge);

        // Act
        Result<string> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileTooLarge, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
