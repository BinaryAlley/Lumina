#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetEnabledLibraries;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Queries.GetEnabledLibraries;

/// <summary>
/// Contains unit tests for the <see cref="GetEnabledLibrariesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetEnabledLibrariesQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryRepository _mockLibraryRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly GetEnabledLibrariesQueryHandler _sut;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEnabledLibrariesQueryHandlerTests"/> class.
    /// </summary>
    public GetEnabledLibrariesQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryRepository = Substitute.For<ILibraryRepository>();
        _mockUnitOfWork.LibraryRepository.Returns(_mockLibraryRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _userId = Guid.NewGuid();

        // default stubs: the current user is authenticated and is an admin
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(true);

        _sut = new GetEnabledLibrariesQueryHandler(_mockAuthorizationService, _mockCurrentUserService, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAdmin_ShouldReturnAllEnabledLibraries()
    {
        // Arrange
        List<LibraryEntity> libraries =
        [
            _libraryEntityFixture.Create(userId: _userId),
            _libraryEntityFixture.Create(userId: Guid.NewGuid())
        ];
        _mockLibraryRepository.GetAllEnabledAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryEntity>>(libraries));

        // Act
        Result<LibraryResponse[]> result = await _sut.HandleAsync(new GetEnabledLibrariesQuery(), CancellationToken.None);

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
        _mockLibraryRepository.GetAllEnabledAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<LibraryEntity>>(libraries));
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<LibraryResponse[]> result = await _sut.HandleAsync(new GetEnabledLibrariesQuery(), CancellationToken.None);

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
        Result<LibraryResponse[]> result = await _sut.HandleAsync(new GetEnabledLibrariesQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockLibraryRepository.DidNotReceive().GetAllEnabledAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllEnabledFails_ShouldReturnError()
    {
        // Arrange
        _mockLibraryRepository.GetAllEnabledAsync(Arg.Any<CancellationToken>())
            .Returns(Error.Failure(description: "Failed to get enabled libraries"));

        // Act
        Result<LibraryResponse[]> result = await _sut.HandleAsync(new GetEnabledLibrariesQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }
}
