#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Domain.Common.Errors;
using Lumina.Infrastructure.Core.Authorization;
using Lumina.Infrastructure.Core.Authorization.Policies.Common.Factory;
using Lumina.Infrastructure.UnitTests.Core.Authorization.Fixtures;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AuthorizationService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationServiceTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IUserRepository _mockUserRepository;
    private readonly IAuthorizationPolicyFactory _mockAuthorizationPolicyFactory;
    private readonly AuthorizationService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationServiceTests"/> class.
    /// </summary>
    public AuthorizationServiceTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _mockAuthorizationPolicyFactory = Substitute.For<IAuthorizationPolicyFactory>();

        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);

        _sut = new AuthorizationService(_mockUnitOfWork, _mockAuthorizationPolicyFactory);
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(null));

        // Act
        bool result = await _sut.HasPermissionAsync(userId, AuthorizationPermission.CanViewUsers, CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockUserRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasPermissionAsync_WhenGetUserFails_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Error error = Error.Failure("Database.Error", "Failed to retrieve user");
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        bool result = await _sut.HasPermissionAsync(userId, AuthorizationPermission.CanViewUsers, CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockUserRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserHasDirectPermission_ShouldReturnTrue()
    {
        // Arrange
        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions(
            directPermissions: [AuthorizationPermission.CanViewUsers]);

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        bool result = await _sut.HasPermissionAsync(user.Id, AuthorizationPermission.CanViewUsers, CancellationToken.None);

        // Assert
        Assert.True(result);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserHasPermissionThroughRole_ShouldReturnTrue()
    {
        // Arrange
        Dictionary<string, IEnumerable<AuthorizationPermission>> rolePermissions = new()
        {
            { "Admin", new[] { AuthorizationPermission.CanViewUsers } }
        };

        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions(
            rolePermissions: rolePermissions);

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        bool result = await _sut.HasPermissionAsync(user.Id, AuthorizationPermission.CanViewUsers, CancellationToken.None);

        // Assert
        Assert.True(result);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserHasNoPermissions_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions();

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        bool result = await _sut.HasPermissionAsync(user.Id, AuthorizationPermission.CanViewUsers, CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserHasDifferentPermission_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions(
            directPermissions: [AuthorizationPermission.CanDeleteUsers]);

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        bool result = await _sut.HasPermissionAsync(user.Id, AuthorizationPermission.CanViewUsers, CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserHasPermissionBothDirectlyAndThroughRole_ShouldReturnTrue()
    {
        // Arrange
        Dictionary<string, IEnumerable<AuthorizationPermission>> rolePermissions = new()
        {
            { "Admin", new[] { AuthorizationPermission.CanViewUsers } }
        };

        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions(
            directPermissions: [AuthorizationPermission.CanViewUsers],
            rolePermissions: rolePermissions);

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        bool result = await _sut.HasPermissionAsync(user.Id, AuthorizationPermission.CanViewUsers, CancellationToken.None);

        // Assert
        Assert.True(result);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsInRoleAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(null));

        // Act
        bool result = await _sut.IsInRoleAsync(userId, "Admin", CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockUserRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsInRoleAsync_WhenGetUserFails_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Error error = Error.Failure("Database.Error", "Failed to retrieve user");
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        bool result = await _sut.IsInRoleAsync(userId, "Admin", CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockUserRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsInRoleAsync_WhenUserHasRole_ShouldReturnTrue()
    {
        // Arrange
        Dictionary<string, IEnumerable<AuthorizationPermission>> rolePermissions = new()
        {
            { "Admin", Array.Empty<AuthorizationPermission>() }
        };

        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions(
            rolePermissions: rolePermissions);

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        bool result = await _sut.IsInRoleAsync(user.Id, "Admin", CancellationToken.None);

        // Assert
        Assert.True(result);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsInRoleAsync_WhenUserHasDifferentRole_ShouldReturnFalse()
    {
        // Arrange
        Dictionary<string, IEnumerable<AuthorizationPermission>> rolePermissions = new()
        {
            { "", Array.Empty<AuthorizationPermission>() }
        };

        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions(
            rolePermissions: rolePermissions);

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        bool result = await _sut.IsInRoleAsync(user.Id, "Admin", CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsInRoleAsync_WhenUserHasNoRoles_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions();

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        bool result = await _sut.IsInRoleAsync(user.Id, "Admin", CancellationToken.None);

        // Assert
        Assert.False(result);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WhenPolicyEvaluationSucceeds_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        IAuthorizationPolicy mockPolicy = Substitute.For<IAuthorizationPolicy>();
        mockPolicy.EvaluateAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _mockAuthorizationPolicyFactory.CreatePolicy<IAuthorizationPolicy>()
            .Returns(mockPolicy);

        // Act
        bool result = await _sut.EvaluatePolicyAsync<IAuthorizationPolicy>(userId, CancellationToken.None);

        // Assert
        Assert.True(result);
        await mockPolicy.Received(1).EvaluateAsync(userId, Arg.Any<CancellationToken>());
        _mockAuthorizationPolicyFactory.Received(1).CreatePolicy<IAuthorizationPolicy>();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WhenPolicyEvaluationFails_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        IAuthorizationPolicy mockPolicy = Substitute.For<IAuthorizationPolicy>();
        mockPolicy.EvaluateAsync(userId, Arg.Any<CancellationToken>())
            .Returns(false);

        _mockAuthorizationPolicyFactory.CreatePolicy<IAuthorizationPolicy>()
            .Returns(mockPolicy);

        // Act
        bool result = await _sut.EvaluatePolicyAsync<IAuthorizationPolicy>(userId, CancellationToken.None);

        // Assert
        Assert.False(result);
        await mockPolicy.Received(1).EvaluateAsync(userId, Arg.Any<CancellationToken>());
        _mockAuthorizationPolicyFactory.Received(1).CreatePolicy<IAuthorizationPolicy>();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WhenPolicyThrowsException_ShouldPropagateException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        IAuthorizationPolicy mockPolicy = Substitute.For<IAuthorizationPolicy>();
        mockPolicy.EvaluateAsync(userId, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Policy evaluation failed"));

        _mockAuthorizationPolicyFactory.CreatePolicy<IAuthorizationPolicy>()
            .Returns(mockPolicy);

        // Act
        Func<Task> act = async () => await _sut.EvaluatePolicyAsync<IAuthorizationPolicy>(userId, CancellationToken.None);

        // Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Equal("Policy evaluation failed", exception.Message);
        await mockPolicy.Received(1).EvaluateAsync(userId, Arg.Any<CancellationToken>());
        _mockAuthorizationPolicyFactory.Received(1).CreatePolicy<IAuthorizationPolicy>();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WhenFactoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _mockAuthorizationPolicyFactory.CreatePolicy<IAuthorizationPolicy>()
            .Throws(new InvalidOperationException("Policy creation failed"));

        // Act
        Func<Task> act = async () => await _sut.EvaluatePolicyAsync<IAuthorizationPolicy>(userId, CancellationToken.None);

        // Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Equal("Policy creation failed", exception.Message);
        _mockAuthorizationPolicyFactory.Received(1).CreatePolicy<IAuthorizationPolicy>();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WhenCancellationRequested_ShouldPropagateToken()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        IAuthorizationPolicy mockPolicy = Substitute.For<IAuthorizationPolicy>();
        CancellationToken cancellationToken = new(true);

        _mockAuthorizationPolicyFactory.CreatePolicy<IAuthorizationPolicy>()
            .Returns(mockPolicy);

        // Act
        await _sut.EvaluatePolicyAsync<IAuthorizationPolicy>(userId, cancellationToken);

        // Assert
        await mockPolicy.Received(1).EvaluateAsync(userId, cancellationToken);
        _mockAuthorizationPolicyFactory.Received(1).CreatePolicy<IAuthorizationPolicy>();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WithConcretePolicy_ShouldCreateCorrectPolicyType()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        IAuthorizationPolicy mockPolicy = Substitute.For<ITestAuthorizationPolicy>();
        mockPolicy.EvaluateAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);

        _mockAuthorizationPolicyFactory.CreatePolicy<ITestAuthorizationPolicy>()
            .Returns(mockPolicy);

        // Act
        bool result = await _sut.EvaluatePolicyAsync<ITestAuthorizationPolicy>(userId, CancellationToken.None);

        // Assert
        Assert.True(result);
        await mockPolicy.Received(1).EvaluateAsync(userId, Arg.Any<CancellationToken>());
        _mockAuthorizationPolicyFactory.Received(1).CreatePolicy<ITestAuthorizationPolicy>();
    }

    [Fact]
    public async Task GetUserAuthorizationAsync_WhenUserDoesNotExist_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(null));

        // Act
        ErrorOr<UserAuthorizationEntity> result = await _sut.GetUserAuthorizationAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Users.UserDoesNotExist, result.FirstError);
        await _mockUserRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserAuthorizationAsync_WhenGetUserFails_ShouldReturnError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Error error = Error.Failure("Database.Error", "Failed to retrieve user");
        _mockUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<UserAuthorizationEntity> result = await _sut.GetUserAuthorizationAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
        await _mockUserRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserAuthorizationAsync_WhenUserHasNoRolesOrPermissions_ShouldReturnEmptySets()
    {
        // Arrange
        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions();
        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        ErrorOr<UserAuthorizationEntity> result = await _sut.GetUserAuthorizationAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Null(result.Value.Role);
        Assert.Empty(result.Value.Permissions);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserAuthorizationAsync_WhenUserHasOnlyDirectPermissions_ShouldReturnCorrectPermissions()
    {
        // Arrange
        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions(
            directPermissions:
            [
                AuthorizationPermission.CanViewUsers,
                AuthorizationPermission.CanDeleteUsers
            ]);

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        ErrorOr<UserAuthorizationEntity> result = await _sut.GetUserAuthorizationAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Null(result.Value.Role);
        Assert.Equal([AuthorizationPermission.CanViewUsers, AuthorizationPermission.CanDeleteUsers],
            result.Value.Permissions);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserAuthorizationAsync_WhenUserHasOnlyRolePermissions_ShouldReturnCorrectRolesAndPermissions()
    {
        // Arrange
        Dictionary<string, IEnumerable<AuthorizationPermission>> rolePermissions = new()
        {
            {
                "Admin",
                new[] { AuthorizationPermission.CanViewUsers, AuthorizationPermission.CanDeleteUsers }
            }
        };

        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions(
            rolePermissions: rolePermissions);

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        ErrorOr<UserAuthorizationEntity> result = await _sut.GetUserAuthorizationAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal("Admin", result.Value.Role);
        Assert.Equal([AuthorizationPermission.CanViewUsers, AuthorizationPermission.CanDeleteUsers],
            result.Value.Permissions);
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserAuthorizationAsync_WhenUserHasBothDirectAndRolePermissions_ShouldReturnCombinedPermissions()
    {
        // Arrange
        Dictionary<string, IEnumerable<AuthorizationPermission>> rolePermissions = new()
        {
            {
                "Admin",
                new[] { AuthorizationPermission.CanViewUsers, AuthorizationPermission.CanDeleteUsers }
            }
        };

        UserEntity user = AuthorizationServiceFixture.CreateUserWithPermissions(
            directPermissions: [AuthorizationPermission.CanRegisterUsers],
            rolePermissions: rolePermissions);

        _mockUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<UserEntity?>(user));

        // Act
        ErrorOr<UserAuthorizationEntity> result = await _sut.GetUserAuthorizationAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal("Admin", result.Value.Role);
        Assert.Equivalent(
            new HashSet<AuthorizationPermission>
            {
        AuthorizationPermission.CanViewUsers,
        AuthorizationPermission.CanDeleteUsers,
        AuthorizationPermission.CanRegisterUsers
            },
            result.Value.Permissions
        );
        await _mockUserRepository.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
    }
}
