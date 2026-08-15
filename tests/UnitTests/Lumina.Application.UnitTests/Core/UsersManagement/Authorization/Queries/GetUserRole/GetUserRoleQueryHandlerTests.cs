#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
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
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly GetUserRoleQueryFixture _getUserRoleQueryFixture = new();
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
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.UserRepository.Returns(_mockUserRepository);

        IValidator<GetUserRoleQuery> mockValidator = Substitute.For<IValidator<GetUserRoleQuery>>();
        mockValidator.Validate(Arg.Any<GetUserRoleQuery>())
            .Returns([]);
        _sut = new GetUserRoleQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork,
            mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<RoleResponse?> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockUserRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetUserByIdFails_ShouldReturnError()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get user");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RoleResponse?> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ShouldReturnUsernameDoesNotExistError()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.Create();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        // Act
        Result<RoleResponse?> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authentication.UsernameDoesNotExist, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasNoRole_ShouldReturnNull()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.Create();
        UserEntity user = _userEntityFixture.Create();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        Result<RoleResponse?> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasRole_ShouldReturnRole()
    {
        // Arrange
        GetUserRoleQuery query = _getUserRoleQueryFixture.Create();
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
        Result<RoleResponse?> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Contains("TestRole", result.Value!.RoleName);
    }
}
