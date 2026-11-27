#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Repositories.MediaLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Core.MediaLibrary.Management.Queries.GetLibraries;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Common.DTO.Pagination;
using Nemera.Application.Fixtures.Core.MediaLibrary.Management.Queries.GetLibraries;
using Nemera.Contracts.Responses.MediaLibrary.Management;
using Nemera.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.Management.Queries.GetLibraries;

/// <summary>
/// Contains unit tests for the <see cref="GetLibrariesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibrariesQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly GetLibrariesQueryHandler _sut;
    private readonly GetLibrariesQueryFixture _getLibrariesQueryFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly PaginatedResultDtoFixture<LibraryEntity> _paginatedResultDtoFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibrariesQueryHandlerTests"/> class.
    /// </summary>
    public GetLibrariesQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated and is an admin.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _sut = new GetLibrariesQueryHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAdmin_ShouldReturnAllLibraries()
    {
        // Arrange
        List<LibraryEntity> libraries =
        [
            _libraryEntityFixture.Create(userId: _userId),
            _libraryEntityFixture.Create(userId: Guid.NewGuid())
        ];
        _mockLibraryRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>()).Returns(Result.From(_paginatedResultDtoFixture.Create(data: libraries, currentPage: 1, perPage: libraries.Count, count: libraries.Count, numberOfPages: 1)));

        // Act
        Result<LibraryResponse[]> result = await _sut.HandleAsync(_getLibrariesQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Length);
        Assert.Equal(libraries[0].Id, result.Value[0].Id);
        Assert.Equal(libraries[1].Id, result.Value[1].Id);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnOnlyLibrariesOwnedByTheUser()
    {
        // Arrange
        Guid otherUserId = Guid.NewGuid();
        List<LibraryEntity> libraries =
        [
            _libraryEntityFixture.Create(userId: _userId),
            _libraryEntityFixture.Create(userId: otherUserId)
        ];
        _mockLibraryRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>()).Returns(Result.From(_paginatedResultDtoFixture.Create(data: libraries, currentPage: 1, perPage: libraries.Count, count: libraries.Count, numberOfPages: 1)));
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<LibraryResponse[]> result = await _sut.HandleAsync(_getLibrariesQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        LibraryResponse response = Assert.Single(result.Value);
        Assert.Equal(libraries[0].Id, response.Id);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<LibraryResponse[]> result = await _sut.HandleAsync(_getLibrariesQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllFails_ShouldReturnError()
    {
        // Arrange
        _mockLibraryRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get libraries"));

        // Act
        Result<LibraryResponse[]> result = await _sut.HandleAsync(_getLibrariesQueryFixture.Create(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }
}
