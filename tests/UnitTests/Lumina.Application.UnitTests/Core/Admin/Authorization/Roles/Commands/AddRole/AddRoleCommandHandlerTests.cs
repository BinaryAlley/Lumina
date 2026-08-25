#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Core.Admin.Authorization.Roles.Commands.AddRole;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.AddRole;

/// <summary>
/// Contains unit tests for the <see cref="AddRoleCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IRoleRepository _mockRoleRepository;
    private readonly AddRoleCommandHandler _sut;
    private readonly AddRoleCommandFixture _addRoleCommandFixture = new();
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly RolePermissionEntityFixture _rolePermissionEntityFixture = new();
    private readonly PermissionEntityFixture _permissionEntityFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleCommandHandlerTests"/> class.
    /// </summary>
    public AddRoleCommandHandlerTests()
    {
        IValidator<AddRoleCommand> mockValidator = Substitute.For<IValidator<AddRoleCommand>>();
        mockValidator.Validate(Arg.Any<AddRoleCommand>())
            .Returns([]);
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockRoleRepository = Substitute.For<IRoleRepository>();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.RoleRepository.Returns(_mockRoleRepository);

        _sut = new AddRoleCommandHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork,
            mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockRoleRepository.DidNotReceive().InsertAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockRoleRepository.DidNotReceive().InsertAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertRoleFails_ShouldReturnError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to insert role");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.InsertAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetRoleByNameFails_ShouldReturnError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get role");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.InsertAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockRoleRepository.GetByNameAsync(command.RoleName, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenGetRoleByNameReturnsNull_ShouldReturnError()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.InsertAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockRoleRepository.GetByNameAsync(command.RoleName, Arg.Any<CancellationToken>())
            .Returns((RoleEntity?)null);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Persistence.ErrorPersistingAuthorizationRole, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenAllOperationsSucceed_ShouldReturnSuccessResponse()
    {
        // Arrange
        AddRoleCommand command = _addRoleCommandFixture.Create();
        Guid roleId = Guid.NewGuid();
        List<RolePermissionEntity> rolePermissions = [.. command.Permissions.Select(permissionId =>
            _rolePermissionEntityFixture.Create(
                roleId: roleId,
                permissionId: permissionId,
                permission: _permissionEntityFixture.Create(id: permissionId, permissionName: AuthorizationPermission.CanViewUsers)))];
        RoleEntity role = _roleEntityFixture.Create(id: roleId, roleName: command.RoleName, includeRolePermissions: true, rolePermissions: rolePermissions);

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.InsertAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);
        _mockRoleRepository.GetByNameAsync(command.RoleName, Arg.Any<CancellationToken>())
            .Returns(role);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(command.RoleName, result.Value.Role.RoleName);
        Assert.Equal(command.Permissions.Count, result.Value.Permissions.Length);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
