#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.Setup;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
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
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly Guid _userId;
    private readonly AddBookCommandHandler _sut;
    private readonly AddBookCommandFixture _commandBookFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();

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
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _userId = Guid.NewGuid();

        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_mediaContributorEntityFixture.Create()));
        // Default stubs: the current user is authenticated and the library ownership policy allows access.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);

        IValidator<AddBookCommand> mockValidator = Substitute.For<IValidator<AddBookCommand>>();
        mockValidator.Validate(Arg.Any<AddBookCommand>())
            .Returns([]);
        _sut = new AddBookCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();

        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

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
        AddBookCommand bookCommand = _commandBookFixture.Create();

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
    public async Task HandleAsync_WhenCalledWithInvalidISBN_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: "invalid", format: IsbnFormat.Isbn13)]);

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
        AddBookCommand bookCommand = _commandBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: -1, maxValue: 5, includeOptionalProperties: false)]);

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
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: "")]));

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
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: "")]));

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
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(
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
    public async Task HandleAsync_WhenLanguageInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: "", languageName: "English")));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockMediaContributorRepository.DidNotReceive().FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenOriginalLanguageInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: "", languageName: "English")));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockMediaContributorRepository.DidNotReceive().FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorsAreLinked_ShouldAttachBookContributorRowsAndReturnThemInTheResponse()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors:
            [
                _mediaContributorDtoFixture.Create(displayName: "Author One", legalName: "Author One Legal", roleName: "author", roleCategory: MediaContributorRoleCategory.Author),
                _mediaContributorDtoFixture.Create(displayName: "Author Two", legalName: "Author Two Legal", roleName: "illustrator", roleCategory: MediaContributorRoleCategory.Illustrator)
            ]);
        MediaContributorEntity firstContributor = _mediaContributorEntityFixture.Create();
        MediaContributorEntity secondContributor = _mediaContributorEntityFixture.Create();
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.From(callInfo.ArgAt<string>(0) == "Author One" ? firstContributor : secondContributor));
        BookEntity? insertedEntity = null;
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                insertedEntity = callInfo.Arg<BookEntity>();
                return Result.Created;
            });

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(bookCommand.Contributors, result.Value.Contributors);
        Assert.NotNull(insertedEntity);
        Assert.Equal(2, insertedEntity!.BookContributors.Count);
        Assert.Equal(firstContributor.Id, insertedEntity.BookContributors[0].MediaContributorId);
        Assert.Equal("author", insertedEntity.BookContributors[0].RoleName);
        Assert.Equal(MediaContributorRoleCategory.Author, insertedEntity.BookContributors[0].RoleCategory);
        Assert.Equal(secondContributor.Id, insertedEntity.BookContributors[1].MediaContributorId);
        Assert.Equal("illustrator", insertedEntity.BookContributors[1].RoleName);
        Assert.Equal(MediaContributorRoleCategory.Illustrator, insertedEntity.BookContributors[1].RoleCategory);
        await _mockBookRepository.Received(1).InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorsShareTheSameDisplayName_ShouldResolveContributorOnceAndLinkEveryRole()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors:
            [
                _mediaContributorDtoFixture.Create(displayName: "John Doe", legalName: "John Smith Doe", roleName: "author", roleCategory: MediaContributorRoleCategory.Author),
                _mediaContributorDtoFixture.Create(displayName: "John Doe", legalName: "John Smith Doe", roleName: "illustrator", roleCategory: MediaContributorRoleCategory.Illustrator)
            ]);
        MediaContributorEntity contributor = _mediaContributorEntityFixture.Create();
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(contributor));
        BookEntity? insertedEntity = null;
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                insertedEntity = callInfo.Arg<BookEntity>();
                return Result.Created;
            });

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaContributorRepository.Received(1).FindOrCreateByDisplayNameAsync(
            "John Doe", "John Smith Doe", Arg.Any<CancellationToken>());
        Assert.NotNull(insertedEntity);
        Assert.Equal(2, insertedEntity!.BookContributors.Count);
        Assert.All(insertedEntity.BookContributors, linkedContributor => Assert.Equal(contributor.Id, linkedContributor.MediaContributorId));
        Assert.Contains(insertedEntity.BookContributors, linkedContributor => linkedContributor.RoleName == "author");
        Assert.Contains(insertedEntity.BookContributors, linkedContributor => linkedContributor.RoleName == "illustrator");
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorHasNullDisplayName_ShouldSkipIt()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors:
            [
                _mediaContributorDtoFixture.Create(displayName: "John Doe", legalName: "John Smith Doe", roleName: "author", roleCategory: MediaContributorRoleCategory.Author),
                _mediaContributorDtoFixture.Create(includeName: false, roleName: "unknown", roleCategory: MediaContributorRoleCategory.Other)
            ]);
        MediaContributorEntity contributor = _mediaContributorEntityFixture.Create();
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(contributor));
        BookEntity? insertedEntity = null;
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                insertedEntity = callInfo.Arg<BookEntity>();
                return Result.Created;
            });

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockMediaContributorRepository.Received(1).FindOrCreateByDisplayNameAsync(
            "John Doe", "John Smith Doe", Arg.Any<CancellationToken>());
        Assert.NotNull(insertedEntity);
        Assert.Single(insertedEntity!.BookContributors);
        Assert.Equal(contributor.Id, insertedEntity.BookContributors[0].MediaContributorId);
    }

    [Fact]
    public async Task HandleAsync_WhenContributorFindOrCreateFails_ShouldReturnFailureResultWithoutInserting()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        Error findOrCreateError = Error.Validation("MediaContributor.FindOrCreate", "Could not find or create the media contributor.");
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(findOrCreateError);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(findOrCreateError, result.FirstError);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
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
        AddBookCommand bookCommand = _commandBookFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == bookCommand.LibraryId), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithValidCommand_ShouldStampTheCurrentUserAsTheAuditActor()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create()]);
        BookEntity? insertedEntity = null;
        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                insertedEntity = callInfo.Arg<BookEntity>();
                return Result.Created;
            });

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(insertedEntity);
        Assert.Equal(_userId, insertedEntity!.CreatedBy);
        Assert.NotEmpty(insertedEntity.BookContributors);
        Assert.All(insertedEntity.BookContributors, contributor => Assert.Equal(_userId, contributor.CreatedBy));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
