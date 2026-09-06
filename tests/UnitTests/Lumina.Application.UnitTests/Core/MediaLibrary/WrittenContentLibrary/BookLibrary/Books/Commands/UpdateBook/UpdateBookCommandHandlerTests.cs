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
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
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
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<UpdateBookCommand> _mockValidator;
    private readonly UpdateBookCommandHandler _sut;
    private readonly Guid _userId;
    private static readonly Error s_findOrCreateError = Error.Validation("MediaContributor.FindOrCreate", "Could not find or create the media contributor.");
    private readonly UpdateBookCommandFixture _commandBookFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCommandHandlerTests"/> class.
    /// </summary>
    public UpdateBookCommandHandlerTests()
    {
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
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
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_mediaContributorEntityFixture.Create()));
        _mockBookRepository.UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _sut = new UpdateBookCommandHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsValid_ShouldUpdateBookAndReturnResponse()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.Id);
        _mockBookRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(command.Id, result.Value.Id);
        Assert.Equal(command.Metadata!.Title, result.Value.Metadata!.Title);
        Assert.Equal(command.Contributors, result.Value.Contributors);
        await _mockBookRepository.Received(1).UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorFails_ShouldReturnValidationErrorsWithoutUpdating()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create();
        _mockValidator.Validate(Arg.Any<UpdateBookCommand>()).Returns([Errors.WrittenContent.BookIdCannotBeEmpty]);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookIdCannotBeEmpty, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnBookNotFoundError()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create();
        _mockBookRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
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
        UpdateBookCommand command = _commandBookFixture.Create();
        _mockBookRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
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
        UpdateBookCommand command = _commandBookFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.Id);
        _mockBookRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
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
    public async Task HandleAsync_WhenContributorFindOrCreateFails_ShouldReturnFailureResultWithoutSaving()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.Id);
        _mockBookRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(s_findOrCreateError);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(s_findOrCreateError, result.FirstError);
        await _mockBookRepository.DidNotReceive().UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryUpdateFails_ShouldReturnFailureResultWithoutSaving()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.Id);
        _mockBookRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
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
    public async Task HandleAsync_WhenContributorsShareTheSameDisplayName_ShouldResolveContributorOnceAndLinkEveryRole()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create(
            contributors:
            [
                _mediaContributorDtoFixture.Create(displayName: "John Doe", legalName: "John Smith Doe", roleName: "author", roleCategory: MediaContributorRoleCategory.Author),
                _mediaContributorDtoFixture.Create(displayName: "John Doe", legalName: "John Smith Doe", roleName: "illustrator", roleCategory: MediaContributorRoleCategory.Illustrator)
            ]);
        BookEntity existingBook = _bookEntityFixture.Create(id: command.Id);
        _mockBookRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        MediaContributorEntity contributor = _mediaContributorEntityFixture.Create();
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(contributor));
        BookEntity? updatedEntity = null;
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
        await _mockMediaContributorRepository.Received(1).FindOrCreateByDisplayNameAsync(
            "John Doe", "John Smith Doe", Arg.Any<CancellationToken>());
        Assert.NotNull(updatedEntity);
        Assert.Equal(2, updatedEntity!.BookContributors.Count);
        Assert.All(updatedEntity.BookContributors, linkedContributor => Assert.Equal(contributor.Id, linkedContributor.MediaContributorId));
        Assert.Contains(updatedEntity.BookContributors, linkedContributor => linkedContributor.RoleName == "author");
        Assert.Contains(updatedEntity.BookContributors, linkedContributor => linkedContributor.RoleName == "illustrator");
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenContributorsHaveDistinctDisplayNames_ShouldResolveEachContributorOnce()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create(
            contributors:
            [
                _mediaContributorDtoFixture.Create(displayName: "Author One", legalName: "Author One Legal", roleName: "author", roleCategory: MediaContributorRoleCategory.Author),
                _mediaContributorDtoFixture.Create(displayName: "Author Two", legalName: "Author Two Legal", roleName: "illustrator", roleCategory: MediaContributorRoleCategory.Illustrator)
            ]);
        BookEntity existingBook = _bookEntityFixture.Create(id: command.Id);
        _mockBookRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        MediaContributorEntity firstContributor = _mediaContributorEntityFixture.Create();
        MediaContributorEntity secondContributor = _mediaContributorEntityFixture.Create();
        _mockMediaContributorRepository.FindOrCreateByDisplayNameAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.From(callInfo.ArgAt<string>(0) == "Author One" ? firstContributor : secondContributor));
        BookEntity? updatedEntity = null;
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
        await _mockMediaContributorRepository.Received(1).FindOrCreateByDisplayNameAsync(
            "Author One", "Author One Legal", Arg.Any<CancellationToken>());
        await _mockMediaContributorRepository.Received(1).FindOrCreateByDisplayNameAsync(
            "Author Two", "Author Two Legal", Arg.Any<CancellationToken>());
        Assert.NotNull(updatedEntity);
        Assert.Equal(2, updatedEntity!.BookContributors.Count);
        Assert.Equal(firstContributor.Id, updatedEntity.BookContributors[0].MediaContributorId);
        Assert.Equal(secondContributor.Id, updatedEntity.BookContributors[1].MediaContributorId);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenStoredBookHasEnrichmentAndCover_ShouldPreserveThemInTheResponse()
    {
        // Arrange
        UpdateBookCommand command = _commandBookFixture.Create();
        BookEntity existingBook = _bookEntityFixture.Create(id: command.Id);
        existingBook.MetadataStatus = MetadataStatus.Enriched;
        existingBook.LastMetadataUpdateUtc = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        existingBook.MetadataProvider = "TestProvider";
        existingBook.BookArtwork.Add(new BookArtworkEntity
        {
            Id = Guid.NewGuid(),
            BookId = existingBook.Id,
            ArtworkType = ArtworkType.Cover,
            Ordinal = 0,
            FileName = "/media/covers/cover.jpg",
            Status = ArtworkStatus.Enriched,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = null
        });
        _mockBookRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<BookEntity?>(existingBook));
        _mockBookRepository.UpdateAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(MetadataStatus.Enriched, result.Value.MetadataStatus);
        Assert.Equal(existingBook.LastMetadataUpdateUtc, result.Value.LastMetadataUpdateUtc);
        Assert.Equal(existingBook.MetadataProvider, result.Value.MetadataProvider);
        Assert.Equal("/media/covers/cover.jpg", result.Value.CoverPath);
    }
}
