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
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingManifestQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IBookRepository _mockBookRepository;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IUserSettingsRepository _mockUserSettingsRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IBookReadingService _mockBookReadingService;
    private readonly IValidator<GetReadingManifestQuery> _mockValidator;
    private readonly GetReadingManifestQueryHandler _sut;
    private readonly Guid _userId;
    private readonly GetReadingManifestQueryFixture _getReadingManifestQueryFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly ReadingManifestResponseFixture _readingManifestResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingManifestQueryHandlerTests"/> class.
    /// </summary>
    public GetReadingManifestQueryHandlerTests()
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
        _mockValidator = Substitute.For<IValidator<GetReadingManifestQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated and the library ownership policy allows access.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetReadingManifestQuery>()).Returns([]);
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(Result.From<UserSettingsEntity?>(null));

        _sut = new GetReadingManifestQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockBookReadingService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReturnManifestResponse()
    {
        // Arrange
        GetReadingManifestQuery query = _getReadingManifestQueryFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookEntity book = _bookEntityFixture.Create(id: query.BookId);
        LibraryEntity library = _libraryEntityFixture.Create(id: book.LibraryId, libraryType: LibraryType.EBook);
        ReadingManifestResponse expectedResponse = _readingManifestResponseFixture.Create();
        _mockBookRepository.GetByIdAsync(query.BookId, cancellationToken).Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(book.LibraryId, cancellationToken).Returns(Result.From<LibraryEntity?>(library));
        _mockBookReadingService.GetManifestAsync(book.Id, book.LibraryId, book.Path, library.LibraryType, shouldRenderPdfAsImages: false, cancellationToken).Returns(expectedResponse);

        // Act
        Result<ReadingManifestResponse> result = await _sut.HandleAsync(query, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedResponse, result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasRenderPdfAsImagesEnabled_ShouldUseTheSetting()
    {
        // Arrange
        GetReadingManifestQuery query = _getReadingManifestQueryFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookEntity book = _bookEntityFixture.Create(id: query.BookId);
        LibraryEntity library = _libraryEntityFixture.Create(id: book.LibraryId, libraryType: LibraryType.EBook);
        UserSettingsEntity settings = _userSettingsEntityFixture.Create(userId: _userId, shouldRenderPdfAsImages: true);
        ReadingManifestResponse expectedResponse = _readingManifestResponseFixture.Create();
        _mockUserSettingsRepository.GetByUserIdAsync(_userId, cancellationToken).Returns(Result.From<UserSettingsEntity?>(settings));
        _mockBookRepository.GetByIdAsync(query.BookId, cancellationToken).Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(book.LibraryId, cancellationToken).Returns(Result.From<LibraryEntity?>(library));
        _mockBookReadingService.GetManifestAsync(book.Id, book.LibraryId, book.Path, library.LibraryType, shouldRenderPdfAsImages: true, cancellationToken).Returns(expectedResponse);

        // Act
        Result<ReadingManifestResponse> result = await _sut.HandleAsync(query, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedResponse, result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnBookNotFoundError()
    {
        // Arrange
        GetReadingManifestQuery query = _getReadingManifestQueryFixture.Create();
        _mockBookRepository.GetByIdAsync(query.BookId, Arg.Any<CancellationToken>()).Returns(Result.From<BookEntity?>(null));

        // Act
        Result<ReadingManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Reading.BookNotFound, result.FirstError);
        await _mockBookReadingService.DidNotReceive().GetManifestAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryDoesNotExist_ShouldReturnLibraryNotFoundError()
    {
        // Arrange
        GetReadingManifestQuery query = _getReadingManifestQueryFixture.Create();
        BookEntity book = _bookEntityFixture.Create(id: query.BookId);
        _mockBookRepository.GetByIdAsync(query.BookId, Arg.Any<CancellationToken>()).Returns(Result.From<BookEntity?>(book));
        _mockLibraryRepository.GetByIdAsync(book.LibraryId, Arg.Any<CancellationToken>()).Returns(Result.From<LibraryEntity?>(null));

        // Act
        Result<ReadingManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Library.LibraryNotFound, result.FirstError);
        await _mockBookReadingService.DidNotReceive().GetManifestAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesAccess_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetReadingManifestQuery query = _getReadingManifestQueryFixture.Create();
        BookEntity book = _bookEntityFixture.Create(id: query.BookId);
        _mockBookRepository.GetByIdAsync(query.BookId, Arg.Any<CancellationToken>()).Returns(Result.From<BookEntity?>(book));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<ReadingManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookReadingService.DidNotReceive().GetManifestAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetReadingManifestQuery query = _getReadingManifestQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ReadingManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutCheckingAvailability()
    {
        // Arrange
        GetReadingManifestQuery query = _getReadingManifestQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetReadingManifestQuery>()).Returns([Errors.Reading.BookIdCannotBeEmpty]);

        // Act
        Result<ReadingManifestResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Reading.BookIdCannotBeEmpty, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockBookReadingService.DidNotReceive().GetManifestAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LibraryType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
}
