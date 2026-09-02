#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingSectionQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IUserSettingsRepository _mockUserSettingsRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IBookReadingService _mockBookReadingService;
    private readonly IValidator<GetReadingSectionQuery> _mockValidator;
    private readonly GetReadingSectionQueryHandler _sut;
    private readonly Guid _userId;
    private readonly GetReadingSectionQueryFixture _getReadingSectionQueryFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly ReadingSectionDtoFixture _readingSectionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionQueryHandlerTests"/> class.
    /// </summary>
    public GetReadingSectionQueryHandlerTests()
    {
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUserSettingsRepository = Substitute.For<IUserSettingsRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockUnitOfWork.UserSettingsRepository.Returns(_mockUserSettingsRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockBookReadingService = Substitute.For<IBookReadingService>();
        _mockValidator = Substitute.For<IValidator<GetReadingSectionQuery>>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and the library ownership policy allows access
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetReadingSectionQuery>()).Returns([]);
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(Result.From<UserSettingsEntity?>(null));

        _sut = new GetReadingSectionQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockBookReadingService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReturnSectionDto()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookEntity book = _bookEntityFixture.Create(id: query.BookId);
        LibraryEntity library = _libraryEntityFixture.Create(id: book.LibraryId, libraryType: LibraryType.EBook);
        ReadingSectionDto expectedResponse = _readingSectionDtoFixture.Create(locationRef: query.LocationRef);
        _mockBookRepository.GetByIdAsync(query.BookId, cancellationToken).Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(book.LibraryId, cancellationToken).Returns(Result.From<LibraryEntity?>(library));
        _mockBookReadingService.GetSectionAsync(book.Id, book.LibraryId, book.Path, library.LibraryType, query.LocationRef, shouldRenderPdfAsImages: false, shouldPreserveStyles: true, cancellationToken).Returns(expectedResponse);

        // Act
        Result<ReadingSectionDto> result = await _sut.HandleAsync(query, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedResponse, result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasRenderPdfAsImagesEnabled_ShouldUseTheSetting()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookEntity book = _bookEntityFixture.Create(id: query.BookId);
        LibraryEntity library = _libraryEntityFixture.Create(id: book.LibraryId, libraryType: LibraryType.EBook);
        UserSettingsEntity settings = _userSettingsEntityFixture.Create(userId: _userId, shouldRenderPdfAsImages: true, shouldPreserveBookStyles: true);
        ReadingSectionDto expectedResponse = _readingSectionDtoFixture.Create(locationRef: query.LocationRef);
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, cancellationToken).Returns(Result.From<UserSettingsEntity?>(settings));
        _mockBookRepository.GetByIdAsync(query.BookId, cancellationToken).Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(book.LibraryId, cancellationToken).Returns(Result.From<LibraryEntity?>(library));
        _mockBookReadingService.GetSectionAsync(book.Id, book.LibraryId, book.Path, library.LibraryType, query.LocationRef, shouldRenderPdfAsImages: true, shouldPreserveStyles: true, cancellationToken).Returns(expectedResponse);

        // Act
        Result<ReadingSectionDto> result = await _sut.HandleAsync(query, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedResponse, result.Value);
    }

    [Theory]
    [InlineData(true)] // the user chose to preserve the styles of the books
    [InlineData(false)] // the user chose to strip the styles of the books
    public async Task HandleAsync_WhenUserHasPreserveBookStylesSetting_ShouldPassItToTheService(bool shouldPreserveBookStyles)
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookEntity book = _bookEntityFixture.Create(id: query.BookId);
        LibraryEntity library = _libraryEntityFixture.Create(id: book.LibraryId, libraryType: LibraryType.EBook);
        UserSettingsEntity settings = _userSettingsEntityFixture.Create(userId: _userId, shouldRenderPdfAsImages: false, shouldPreserveBookStyles: shouldPreserveBookStyles);
        ReadingSectionDto expectedResponse = _readingSectionDtoFixture.Create(locationRef: query.LocationRef);
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, cancellationToken).Returns(Result.From<UserSettingsEntity?>(settings));
        _mockBookRepository.GetByIdAsync(query.BookId, cancellationToken).Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(book.LibraryId, cancellationToken).Returns(Result.From<LibraryEntity?>(library));
        _mockBookReadingService.GetSectionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        Result<ReadingSectionDto> result = await _sut.HandleAsync(query, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedResponse, result.Value);
        await _mockBookReadingService.Received(1).GetSectionAsync(book.Id, book.LibraryId, book.Path, library.LibraryType, query.LocationRef, shouldRenderPdfAsImages: false, shouldPreserveStyles: shouldPreserveBookStyles, cancellationToken);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSettingsExist_ShouldDefaultToPreservingBookStyles()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookEntity book = _bookEntityFixture.Create(id: query.BookId);
        LibraryEntity library = _libraryEntityFixture.Create(id: book.LibraryId, libraryType: LibraryType.EBook);
        ReadingSectionDto expectedResponse = _readingSectionDtoFixture.Create(locationRef: query.LocationRef);
        _mockBookRepository.GetByIdAsync(query.BookId, cancellationToken).Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(book.LibraryId, cancellationToken).Returns(Result.From<LibraryEntity?>(library));
        _mockBookReadingService.GetSectionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        Result<ReadingSectionDto> result = await _sut.HandleAsync(query, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedResponse, result.Value);
        await _mockBookReadingService.Received(1).GetSectionAsync(book.Id, book.LibraryId, book.Path, library.LibraryType, query.LocationRef, shouldRenderPdfAsImages: false, shouldPreserveStyles: true, cancellationToken);
        await _mockBookReadingService.DidNotReceive().GetSectionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Is<bool>(false), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnBookNotFoundError()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();
        _mockBookRepository.GetByIdAsync(query.BookId, Arg.Any<CancellationToken>()).Returns(Result.From<BookEntity?>(null));

        // Act
        Result<ReadingSectionDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Reading.BookNotFound, result.FirstError);
        await _mockBookReadingService.DidNotReceive().GetSectionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();
        BookEntity book = _bookEntityFixture.Create(id: query.BookId);
        _mockBookRepository.GetByIdAsync(query.BookId, Arg.Any<CancellationToken>()).Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(book.LibraryId, Arg.Any<CancellationToken>()).Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<ReadingSectionDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        await _mockBookReadingService.DidNotReceive().GetSectionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();
        BookEntity book = _bookEntityFixture.Create(id: query.BookId);
        _mockBookRepository.GetByIdAsync(query.BookId, Arg.Any<CancellationToken>()).Returns(Result.From<BookEntity?>(book));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<ReadingSectionDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookReadingService.DidNotReceive().GetSectionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ReadingSectionDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutFetchingTheSection()
    {
        // Arrange
        GetReadingSectionQuery query = _getReadingSectionQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetReadingSectionQuery>()).Returns([Errors.Reading.LocationRefCannotBeEmpty]);

        // Act
        Result<ReadingSectionDto> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.LocationRefCannotBeEmpty, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookReadingService.DidNotReceive().GetSectionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
}
