#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.GetUsers;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Core.UsersManagement.Authentication.Queries.GetUsers;
using Lumina.Contracts.Responses.UsersManagement.Users;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.GetUsers;

/// <summary>
/// Contains unit tests for the <see cref="GetUsersQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUsersQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IUserRepository _mockUserRepository;
    private readonly GetUsersQueryHandler _sut;
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly GetUsersQueryFixture _getUsersQueryFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersQueryHandlerTests"/> class.
    /// </summary>
    public GetUsersQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.UserRepository.Returns(_mockUserRepository);

        _sut = new GetUsersQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetUsersQuery query = _getUsersQueryFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IEnumerable<UserResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockUserRepository.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetUsersFails_ShouldReturnError()
    {
        // Arrange
        GetUsersQuery query = _getUsersQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get users");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IEnumerable<UserResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenAllOperationsSucceed_ShouldReturnUsers()
    {
        // Arrange
        GetUsersQuery query = _getUsersQueryFixture.Create();
        IEnumerable<UserEntity> users = _userEntityFixture.CreateMany(2);


        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(users));

        // Act
        Result<IEnumerable<UserResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
    }
}
