#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;
using Lumina.Contracts.Entities.Common;
using Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;

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
    private readonly AddBookCommandFixture _commandBookFixture;

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

        _mockUnitOfWork.GetRepository<IBookRepository>().Returns(_mockBookRepository);

        _sut = new AddBookCommandHandler(_mockUnitOfWork);
        _commandBookFixture = new AddBookCommandFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();

        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        ErrorOr<BookResponse> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<BookResponse>();
        await _mockBookRepository.Received(1).InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBookCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata! with { Title = null } };

        // Act
        ErrorOr<BookResponse> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Description == Errors.Metadata.TitleCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryInsertFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();

        _mockBookRepository.InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>())
            .Returns(Errors.WrittenContent.BookAlreadyExists);

        // Act
        ErrorOr<BookResponse> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(Errors.WrittenContent.BookAlreadyExists);
        await _mockBookRepository.Received(1).InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCalledWithInvalidISBN_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ISBNs = [new IsbnEntity("invalid", IsbnFormat.Isbn13)] };

        // Act
        ErrorOr<BookResponse> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Description == Errors.WrittenContent.InvalidIsbn13Format.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCalledWithInvalidRating_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Ratings = [new BookRatingEntity(-1, 5, null, null)] };

        // Act
        ErrorOr<BookResponse> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Description == Errors.Metadata.RatingValueMustBePositive.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGenreCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                Genres = [new GenreEntity("")]
            }
        };

        // Act
        ErrorOr<BookResponse> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                Tags = [new TagEntity("")]
            }
        };

        // Act
        ErrorOr<BookResponse> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Description == Errors.Metadata.TagNameCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReleaseInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                ReleaseInfo = new ReleaseInfoEntity(
                    OriginalReleaseDate: new DateOnly(2025, 1, 1),
                    ReReleaseDate: new DateOnly(2024, 1, 1),
                    OriginalReleaseYear: null,
                    ReReleaseYear: null,
                    ReleaseCountry: null,
                    ReleaseVersion: null
                )
            }
        };

        // Act
        ErrorOr<BookResponse> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Description == Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLanguageInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                Language = new LanguageInfoEntity("", "English", null)
            }
        };

        // Act
        ErrorOr<BookResponse> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Description == Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOriginalLanguageInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata! with
            {
                OriginalLanguage = new LanguageInfoEntity("", "English", null)
            }
        };

        // Act
        ErrorOr<BookResponse> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Description == Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
