#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Contracts.Models.Common;
using Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using MapsterMapper;
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
    private readonly IMapper _mockMapper;
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
        _mockMapper = Substitute.For<IMapper>();
        _mockBookRepository = Substitute.For<IBookRepository>();

        _mockUnitOfWork.GetRepository<IBookRepository>().Returns(_mockBookRepository);

        _sut = new AddBookCommandHandler(_mockUnitOfWork, _mockMapper);
        _commandBookFixture = new AddBookCommandFixture();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        BookModel bookModel = _fixture.Create<BookModel>();

        _mockMapper.Map<BookModel>(Arg.Any<Book>()).Returns(bookModel);
        _mockBookRepository.InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        ErrorOr<Book> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<Book>();
        await _mockBookRepository.Received(1).InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBookCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Metadata = bookCommand.Metadata with { Title = null } };

        // Act
        ErrorOr<Book> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == Errors.Metadata.TitleCannotBeEmpty.Code);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryInsertFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        BookModel bookModel = _fixture.Create<BookModel>();

        _mockMapper.Map<BookModel>(Arg.Any<Book>()).Returns(bookModel);
        _mockBookRepository.InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>())
            .Returns(Errors.WrittenContent.BookAlreadyExists);

        // Act
        ErrorOr<Book> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(Errors.WrittenContent.BookAlreadyExists);
        await _mockBookRepository.Received(1).InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCalledWithInvalidISBN_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { ISBNs = [new IsbnModel("invalid", IsbnFormat.Isbn13)] };

        // Act
        ErrorOr<Book> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == Errors.WrittenContent.InvalidIsbn13Format.Code);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCalledWithInvalidRating_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with { Ratings = [new BookRatingModel(-1, 5, null, null)] };

        // Act
        ErrorOr<Book> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == Errors.Metadata.RatingValueMustBePositive.Code);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGenreCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata with
            {
                Genres = [new GenreModel("")]
            }
        };

        // Act
        ErrorOr<Book> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == Errors.Metadata.GenreNameCannotBeEmpty.Code);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata with
            {
                Tags = [new TagModel("")]
            }
        };

        // Act
        ErrorOr<Book> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == Errors.Metadata.TagNameCannotBeEmpty.Code);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReleaseInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata with
            {
                ReleaseInfo = new ReleaseInfoModel(
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
        ErrorOr<Book> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Code);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLanguageInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata with
            {
                Language = new LanguageInfoModel("", "English", null)
            }
        };

        // Act
        ErrorOr<Book> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == Errors.Metadata.LanguageCodeCannotBeEmpty.Code);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOriginalLanguageInfoCreationFails_ShouldReturnFailureResult()
    {
        // Arrange
        AddBookCommand bookCommand = _commandBookFixture.CreateCommandBook();
        bookCommand = bookCommand with
        {
            Metadata = bookCommand.Metadata with
            {
                OriginalLanguage = new LanguageInfoModel("", "English", null)
            }
        };

        // Act
        ErrorOr<Book> result = await _sut.Handle(bookCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == Errors.Metadata.LanguageCodeCannotBeEmpty.Code);
        await _mockBookRepository.DidNotReceive().InsertAsync(Arg.Any<BookModel>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}