#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetUserRole.Fixtures;
using Lumina.Contracts.Responses.Authorization;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetUserRole;

/// <summary>
/// Contains unit tests for the <see cref="GetUserRoleQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IUserRepository _mockUserRepository;
    private readonly GetUserRoleQueryHandler _sut;
    private readonly GetUserRoleQueryFixture _getUserRoleQueryFixture;
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleQueryHandlerTests"/> class.
    /// </summary>
    public GetUserRoleQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _getUserRoleQueryFixture = new GetUserRoleQueryFixture();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);

        _sut = new GetUserRoleQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.CreateQuery();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        ErrorOr<RoleResponse?> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockUserRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetUserByIdFails_ShouldReturnError()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.CreateQuery();
        Error error = Error.Failure("Database.Error", "Failed to get user");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<RoleResponse?> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnUsernameDoesNotExistError()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.CreateQuery();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        // Act
        ErrorOr<RoleResponse?> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authentication.UsernameDoesNotExist, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoRole_ShouldReturnNull()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.CreateQuery();
        UserEntity user = UserEntityFixture.CreateUserEntity();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        ErrorOr<RoleResponse?> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Handle_WhenUserHasRole_ShouldReturnRole()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.CreateQuery();
        UserEntity user = new()
        {
            Id = query.UserId!.Value,
            Username = "testUser",
            Password = "hashedPassword",
            Libraries = [],
            UserPermissions = [],
            UserRole = new()
            {
                UserId = query.UserId!.Value,
                RoleId = Guid.NewGuid(),
                User = null!,
                Role = new() { Id = Guid.NewGuid(), RoleName = "TestRole" }
            },
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = query.UserId!.Value
        };

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        ErrorOr<RoleResponse?> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Contains("TestRole", result.Value!.RoleName);
    }
}
