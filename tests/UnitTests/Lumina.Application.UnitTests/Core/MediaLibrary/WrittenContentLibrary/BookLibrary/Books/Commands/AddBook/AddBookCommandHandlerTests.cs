#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
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
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.Setup;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;

/// <summary>
/// Contains unit tests for the <see cref="AddBookCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<AddBookCommand> _mockValidator;
    private readonly IPathService _mockPathService;
    private readonly Guid _userId;
    private readonly AddBookCommandHandler _sut;
    private readonly AddBookCommandFixture _addBookCommandFixture = new();
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
    /// Initializes a new instance of the <see cref="AddBookCommandHandlerTests"/> class.
    /// </summary>
    public AddBookCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
        _fixture.Customizations.Add(new NullableDateOnlySpecimenBuilder());

        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockPathService = Substitute.For<IPathService>();
        _userId = Guid.NewGuid();

        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);
        // Default stub: saving changes succeeds.
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        // Default stubs: the current user is authenticated and the library ownership policy allows access.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        // Default stub: the book path is inside the content locations of the library.
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(_libraryEntityFixture.Create()));
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);
        // Default stub: every referenced media contributor already exists.
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                IReadOnlyCollection<Guid> contributorIds = callInfo.Arg<IReadOnlyCollection<Guid>>();
                return Result.From<IReadOnlyList<MediaContributorEntity>>([.. contributorIds.Select(contributorId => _mediaContributorEntityFixture.Create(id: contributorId, displayName: contributorId.ToString()))]);
            });

        IValidator<AddBookCommand> mockValidator = Substitute.For<IValidator<AddBookCommand>>();
        mockValidator.Validate(Arg.Any<AddBookCommand>())
            .Returns([]);
        _mockValidator = mockValidator;
        _sut = new AddBookCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator, _mockPathService);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsErrors_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create();
        _mockValidator.Validate(Arg.Any<AddBookCommand>())
            .Returns([Errors.WrittenContent.IsbnListCannotBeNull]);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.IsbnListCannotBeNull, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create();

        BookEntity? insertedEntity = null;
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                insertedEntity = callInfo.Arg<BookEntity>();
                return Result.Created;
            });
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<BookEntity?>(insertedEntity));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.IsType<BookResponse>(result.Value);
        await _mockBookRepository.Received(1).InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryInsertFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create();

        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Errors.WrittenContent.BookAlreadyExists);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(Errors.WrittenContent.BookAlreadyExists, result.Errors);
        await _mockBookRepository.Received(1).InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesDetectsAConcurrentDuplicatePath_ShouldReturnThePersistenceError()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create();
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithInvalidISBN_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create(isbns: [_isbnDtoFixture.Create(value: "invalid", format: IsbnFormat.Isbn13)]);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.WrittenContent.InvalidIsbn13Format.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockMediaContributorRepository.DidNotReceive().FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithInvalidRating_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: -1, maxValue: 5, includeSource: false, includeVoteCount: false)]);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.RatingValueMustBePositive.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockMediaContributorRepository.DidNotReceive().FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGenreCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: "")]));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockMediaContributorRepository.DidNotReceive().FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTagCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: "")]));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.TagNameCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockMediaContributorRepository.DidNotReceive().FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenReleaseInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(
                    originalReleaseDate: new DateOnly(2025, 1, 1),
                    originalReleaseYear: 2025,
                    reReleaseDate: new DateOnly(2024, 1, 1),
                    reReleaseYear: 2024)));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockMediaContributorRepository.DidNotReceive().FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorsAreLinked_ShouldAttachBookContributorRowsAndReturnThemInTheResponse()
    {
        // Arrange
        Guid firstContributorId = Guid.NewGuid();
        Guid secondContributorId = Guid.NewGuid();
        AddBookCommand bookCommand = _addBookCommandFixture.Create(contributors:
            [
                _mediaContributorReferenceDtoFixture.Create(contributorId: firstContributorId, role: MediaContributorRole.Author),
                _mediaContributorReferenceDtoFixture.Create(contributorId: secondContributorId, role: MediaContributorRole.Illustrator)
            ]);
        BookEntity? insertedEntity = null;
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                insertedEntity = callInfo.Arg<BookEntity>();
                return Result.Created;
            });
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<BookEntity?>(insertedEntity));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(insertedEntity);
        Assert.Equal(2, insertedEntity!.Contributors.Count);
        Assert.Contains(insertedEntity.Contributors, contributor => contributor.MediaContributorId == firstContributorId && contributor.Role == MediaContributorRole.Author);
        Assert.Contains(insertedEntity.Contributors, contributor => contributor.MediaContributorId == secondContributorId && contributor.Role == MediaContributorRole.Illustrator);
        await _mockBookRepository.Received(1).InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorsHaveTheSameIdAndRole_ShouldLinkThemOnlyOnce()
    {
        // Arrange
        Guid contributorId = Guid.NewGuid();
        AddBookCommand bookCommand = _addBookCommandFixture.Create(contributors:
            [
                _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Author),
                _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Author),
                _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Illustrator)
            ]);
        BookEntity? insertedEntity = null;
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                insertedEntity = callInfo.Arg<BookEntity>();
                return Result.Created;
            });
        _mockBookRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<BookEntity?>(insertedEntity));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(insertedEntity);
        Assert.Equal(2, insertedEntity!.Contributors.Count);
        Assert.All(insertedEntity.Contributors, linkedContributor => Assert.Equal(contributorId, linkedContributor.MediaContributorId));
        Assert.Contains(insertedEntity.Contributors, linkedContributor => linkedContributor.Role == MediaContributorRole.Author);
        Assert.Contains(insertedEntity.Contributors, linkedContributor => linkedContributor.Role == MediaContributorRole.Illustrator);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotOwnTheLibrary_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == Guid.Parse(bookCommand.LibraryId!)), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create();
        _mockLibraryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<LibraryEntity?>(null));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.LibraryNotFound, result.FirstError);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookPathIsNotWithinTheLibraryContentLocations_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create();
        _mockPathService.IsPathWithin(Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookPathMustBeWithinLibraryContentLocations, result.FirstError);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAReferencedContributorDoesNotExist_ShouldReturnFailureResultWithoutPersisting()
    {
        // Arrange
        AddBookCommand bookCommand = _addBookCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create()]);
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaContributorEntity>>([]));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaContributor.MediaContributorNotFound, result.FirstError);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
