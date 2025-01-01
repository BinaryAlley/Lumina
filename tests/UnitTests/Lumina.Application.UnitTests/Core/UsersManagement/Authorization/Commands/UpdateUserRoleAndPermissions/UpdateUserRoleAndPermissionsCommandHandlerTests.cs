#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions.Fixtures;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserRoleAndPermissionsCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IUserRepository _mockUserRepository;
    private readonly IRoleRepository _mockRoleRepository;
    private readonly IPermissionRepository _mockPermissionRepository;
    private readonly UpdateUserRoleAndPermissionsCommandHandler _sut;
    private readonly UpdateUserRoleAndPermissionsCommandFixture _updateUserRoleAndPermissionsCommandFixture;
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsCommandHandlerTests"/> class.
    /// </summary>
    public UpdateUserRoleAndPermissionsCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _mockRoleRepository = Substitute.For<IRoleRepository>();
        _mockPermissionRepository = Substitute.For<IPermissionRepository>();
        _updateUserRoleAndPermissionsCommandFixture = new UpdateUserRoleAndPermissionsCommandFixture();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(_mockRoleRepository);
        _mockUnitOfWork.GetRepository<IPermissionRepository>().Returns(_mockPermissionRepository);

        _sut = new UpdateUserRoleAndPermissionsCommandHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.CreateCommand();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ApplicationErrors.Authorization.NotAuthorized);
        await _mockUserRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnUserDoesNotExistError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.CreateCommand();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(DomainErrors.Users.UserDoesNotExist);
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ShouldReturnRoleNotFoundError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.CreateCommand(roleId: Guid.NewGuid());
        UserEntity user = UserEntityFixture.CreateUserEntity();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _mockRoleRepository.GetByIdAsync(command.RoleId!.Value, Arg.Any<CancellationToken>())
            .Returns((RoleEntity?)null);

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ApplicationErrors.Authorization.RoleNotFound);
    }

    [Fact]
    public async Task Handle_WhenRemovingLastAdmin_ShouldReturnCannotRemoveLastAdminError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.CreateCommand(roleId: Guid.NewGuid());
        RoleEntity adminRole = new() { RoleName = "Admin" };
        Guid userId = Guid.NewGuid();

        UserEntity user = new()
        {
            Id = userId,
            Username = "testAdmin",
            Password = "hashedPassword",
            Libraries = [],
            UserPermissions = [],
            UserRole = new UserRoleEntity
            {
                UserId = userId,
                RoleId = Guid.NewGuid(),
                User = null!,
                Role = adminRole
            },
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = userId
        };

        RoleEntity newRole = new() { RoleName = "User" };
        IEnumerable<UserEntity> users = [user];

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _mockRoleRepository.GetByIdAsync(command.RoleId!.Value, Arg.Any<CancellationToken>())
            .Returns(newRole);
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(users));

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ApplicationErrors.Authorization.CannotRemoveLastAdmin);
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_ShouldReturnError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.CreateCommand(roleId: Guid.NewGuid());
        UserEntity user = UserEntityFixture.CreateUserEntity();
        RoleEntity role = new() { RoleName = "TestRole" };
        IEnumerable<PermissionEntity> permissions = command.Permissions.Select(p =>
            new PermissionEntity { Id = p, PermissionName = AuthorizationPermission.CanViewUsers });
        Error error = Error.Failure("Database.Error", "Failed to update user");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _mockRoleRepository.GetByIdAsync(command.RoleId!.Value, Arg.Any<CancellationToken>())
            .Returns(role);
        _mockPermissionRepository.GetByIdsAsync(command.Permissions, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(permissions));
        _mockUserRepository.UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAllOperationsSucceed_ShouldReturnSuccessResponse()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.CreateCommand(roleId: Guid.NewGuid());
        UserEntity user = new()
        {
            Id = command.UserId,
            Username = "testUser",
            Password = "hashedPassword",
            Libraries = [],
            UserPermissions = command.Permissions.Select((p, i) => new UserPermissionEntity
            {
                UserId = command.UserId,
                PermissionId = p,
                User = null!,
                Permission = new PermissionEntity
                {
                    Id = p,
                    PermissionName = (AuthorizationPermission)(i + 1) 
                }
            }).ToList(),
            UserRole = null,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = command.UserId
        };
        RoleEntity role = new() { RoleName = "TestRole" };
        IEnumerable<PermissionEntity> permissions = command.Permissions.Select((p, i) =>
            new PermissionEntity { Id = p, PermissionName = (AuthorizationPermission)(i + 1) });

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _mockRoleRepository.GetByIdAsync(command.RoleId!.Value, Arg.Any<CancellationToken>())
            .Returns(role);
        _mockPermissionRepository.GetByIdsAsync(command.Permissions, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(permissions));
        _mockUserRepository.UpdateAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UserId.Should().Be(command.UserId);
        result.Value.Role.Should().Be(role.RoleName);
        result.Value.Permissions.Should().HaveCount(command.Permissions.Count);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetAllUsersFails_ShouldReturnError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.CreateCommand(roleId: Guid.NewGuid());
        UserEntity user = new()
        {
            Id = command.UserId,
            Username = "testUser",
            Password = "hashedPassword",
            Libraries = [],
            UserPermissions = [],
            UserRole = new UserRoleEntity
            {
                UserId = command.UserId,
                RoleId = Guid.NewGuid(),
                User = null!,
                Role = new RoleEntity { RoleName = "Admin" }
            },
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = command.UserId
        };
        Error error = Error.Failure("Database.Error", "Failed to get users");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _mockRoleRepository.GetByIdAsync(command.RoleId!.Value, Arg.Any<CancellationToken>())
            .Returns(new RoleEntity { RoleName = "User" });
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
    }

    [Fact]
    public async Task Handle_WhenGetPermissionsFails_ShouldReturnError()
    {
        // Arrange
        UpdateUserRoleAndPermissionsCommand command = _updateUserRoleAndPermissionsCommandFixture.CreateCommand(roleId: Guid.NewGuid());
        UserEntity user = new()
        {
            Id = command.UserId,
            Username = "testUser",
            Password = "hashedPassword",
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = command.UserId
        };
        Error error = Error.Failure("Database.Error", "Failed to get permissions");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _mockRoleRepository.GetByIdAsync(command.RoleId!.Value, Arg.Any<CancellationToken>())
            .Returns(new RoleEntity { RoleName = "User" });
        _mockPermissionRepository.GetByIdsAsync(command.Permissions, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
    }
}
