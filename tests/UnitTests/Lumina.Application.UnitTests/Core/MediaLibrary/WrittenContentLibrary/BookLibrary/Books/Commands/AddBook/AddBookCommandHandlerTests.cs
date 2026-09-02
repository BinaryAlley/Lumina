#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Application.Fixtures.Common.Setup;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly AddBookCommandHandler _sut;
    private readonly AddBookCommandFixture _commandBookFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();

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

        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);

        IValidator<AddBookCommand> mockValidator = Substitute.For<IValidator<AddBookCommand>>();
        mockValidator.Validate(Arg.Any<AddBookCommand>())
            .Returns([]);
        _sut = new AddBookCommandHandler(_mockUnitOfWork, mockValidator);
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
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { ISBNs = [_isbnDtoFixture.Create(value: "invalid", format: IsbnFormat.Isbn13)] };

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.WrittenContent.InvalidIsbn13Format.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalledWithInvalidRating_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with { Ratings = [_bookRatingDtoFixture.Create(value: -1, maxValue: 5, includeOptionalProperties: false)] };

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.RatingValueMustBePositive.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGenreCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                Genres = [_genreDtoFixture.Create(name: "")]
            }
        };

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTagCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                Tags = [_tagDtoFixture.Create(name: "")]
            }
        };

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.TagNameCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenReleaseInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                ReleaseInfo = _releaseInfoDtoFixture.Create(
                    originalReleaseDate: new DateOnly(2025, 1, 1),
                    reReleaseDate: new DateOnly(2024, 1, 1))
            }
        };

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLanguageInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                Language = _languageInfoDtoFixture.Create(languageCode: "", languageName: "English")
            }
        };

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenOriginalLanguageInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.Create();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                OriginalLanguage = _languageInfoDtoFixture.Create(languageCode: "", languageName: "English")
            }
        };

        // Act
        Result<BookResponse> result = await _sut.HandleAsync(bookCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Description == Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
