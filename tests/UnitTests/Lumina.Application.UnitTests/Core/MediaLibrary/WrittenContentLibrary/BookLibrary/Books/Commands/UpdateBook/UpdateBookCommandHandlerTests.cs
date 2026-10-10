#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<UpdateBookCommand> _mockValidator;
    private readonly UpdateBookCommandHandler _sut;
    private readonly Guid _userId;
    private readonly UpdateBookCommandFixture _updateBookCommandFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCommandHandlerTests"/> class.
    /// </summary>
    public UpdateBookCommandHandlerTests()
    {
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<UpdateBookCommand>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access,
        // the validator passes and the existing book is found in the repository.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<UpdateBookCommand>()).Returns([]);
        _mockBookRepository.UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        // Default stub: the library of the book exists.
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(_libraryEntityFixture.Create()));
        // Default stub: every referenced media contributor already exists.
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                IReadOnlyCollection<Guid> contributorIds = callInfo.Arg<IReadOnlyCollection<Guid>>();
                return Result.From<IReadOnlyList<MediaContributorEntity>>([.. contributorIds.Select(contributorId => _mediaContributorEntityFixture.Create(id: contributorId, displayName: contributorId.ToString()))]);
            });

        _sut = new UpdateBookCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsValid_ShouldUpdateBookAndReturnResponse()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        BookEntity? updatedBook = null;
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.From<BookEntity?>(callInfo.ArgAt<bool>(2) ? existingBook : updatedBook));
        _mockBookRepository.UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                updatedBook = callInfo.Arg<BookEntity>();
                return Result.Updated;
            });

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Guid.Parse(command.BookId!), result.Value.Id);
        Assert.Equal(command.Metadata!.Title, result.Value.Metadata!.Title);
        Assert.Equal(command.Contributors!.Count, result.Value.Contributors!.Count);
        await _mockBookRepository.Received(1).UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorFails_ShouldReturnValidationErrorsWithoutUpdating()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<UpdateBookCommand>()).Returns([Errors.WrittenContent.BookIdCannotBeEmpty]);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookIdCannotBeEmpty, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnBookNotFoundError()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create();
        _mockBookRepository.GetByIdAsync(Guid.Parse(command.BookId!), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(null));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsError_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create();
        _mockBookRepository.GetByIdAsync(Guid.Parse(command.BookId!), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Errors.Library.LibraryIdCannotBeEmpty);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryIdCannotBeEmpty, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedErrorWithoutUpdating()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        _mockBookRepository.GetByIdAsync(Guid.Parse(command.BookId!), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == existingBook.LibraryId), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryUpdateFails_ShouldReturnFailureResultWithoutSaving()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        _mockBookRepository.GetByIdAsync(Guid.Parse(command.BookId!), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockBookRepository.UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Errors.WrittenContent.BookNotFound);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookNotFound, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorsHaveTheSameIdAndRole_ShouldLinkThemOnce()
    {
        // Arrange
        Guid contributorId = Guid.NewGuid();
        UpdateBookCommand command = _updateBookCommandFixture.Create(
            contributors:
            [
                _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Author),
                _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Author),
                _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Illustrator)
            ]);
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        BookEntity? updatedEntity = null;
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.From<BookEntity?>(callInfo.ArgAt<bool>(2) ? existingBook : updatedEntity));
        _mockBookRepository.UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                updatedEntity = callInfo.Arg<BookEntity>();
                return Result.Updated;
            });

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(updatedEntity);
        Assert.Equal(2, updatedEntity!.Contributors.Count);
        Assert.All(updatedEntity.Contributors, linkedContributor => Assert.Equal(contributorId, linkedContributor.MediaContributorId));
        Assert.Contains(updatedEntity.Contributors, linkedContributor => linkedContributor.Role == MediaContributorRole.Author);
        Assert.Contains(updatedEntity.Contributors, linkedContributor => linkedContributor.Role == MediaContributorRole.Illustrator);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorsHaveDistinctIds_ShouldLinkEachContributor()
    {
        // Arrange
        Guid firstContributorId = Guid.NewGuid();
        Guid secondContributorId = Guid.NewGuid();
        UpdateBookCommand command = _updateBookCommandFixture.Create(
            contributors:
            [
                _mediaContributorReferenceDtoFixture.Create(contributorId: firstContributorId, role: MediaContributorRole.Author),
                _mediaContributorReferenceDtoFixture.Create(contributorId: secondContributorId, role: MediaContributorRole.Illustrator)
            ]);
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        BookEntity? updatedEntity = null;
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.From<BookEntity?>(callInfo.ArgAt<bool>(2) ? existingBook : updatedEntity));
        _mockBookRepository.UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                updatedEntity = callInfo.Arg<BookEntity>();
                return Result.Updated;
            });

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(updatedEntity);
        Assert.Equal(2, updatedEntity!.Contributors.Count);
        Assert.Equal(firstContributorId, updatedEntity.Contributors[0].MediaContributorId);
        Assert.Equal(secondContributorId, updatedEntity.Contributors[1].MediaContributorId);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithInvalidIsbn_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create(isbns: [_isbnDtoFixture.Create(value: "invalid", format: IsbnFormat.Isbn13)]);
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.WrittenContent.InvalidIsbn13Format.Description);
        await _mockBookRepository.DidNotReceive().UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithInvalidRating_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: -1, maxValue: 5, includeSource: false, includeVoteCount: false)]);
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.RatingValueMustBePositive.Description);
        await _mockBookRepository.DidNotReceive().UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGenreCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: "")]));
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTagCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: "")]));
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.TagNameCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenReleaseInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(
                    originalReleaseDate: new DateOnly(2025, 1, 1),
                    originalReleaseYear: 2025,
                    reReleaseDate: new DateOnly(2024, 1, 1),
                    reReleaseYear: 2024)));
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Description);
        await _mockBookRepository.DidNotReceive().UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAReferencedContributorDoesNotExist_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create()]);
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaContributorEntity>>([]));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaContributor.MediaContributorNotFound, result.FirstError);
        await _mockBookRepository.DidNotReceive().UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookBelongsToAnotherLibrary_ShouldReturnBookNotFoundError()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create();
        // The stored book belongs to a different library than the one named by the route.
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.NewGuid());
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundErrorWithoutUpdating()
    {
        // Arrange
        UpdateBookCommand command = _updateBookCommandFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: Guid.Parse(command.BookId!), libraryId: Guid.Parse(command.LibraryId!));
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockBookRepository.DidNotReceive().UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
