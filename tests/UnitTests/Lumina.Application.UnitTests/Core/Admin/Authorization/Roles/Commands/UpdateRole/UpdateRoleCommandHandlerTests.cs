#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.UpdateRole.Fixtures;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.UpdateRole;

/// <summary>
/// Contains unit tests for the <see cref="UpdateRoleCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IRoleRepository _mockRoleRepository;
    private readonly UpdateRoleCommandHandler _sut;
    private readonly UpdateRoleCommandFixture _updateRoleCommandFixture;
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleCommandHandlerTests"/> class.
    /// </summary>
    public UpdateRoleCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockRoleRepository = Substitute.For<IRoleRepository>();
        _updateRoleCommandFixture = new UpdateRoleCommandFixture();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(_mockRoleRepository);

        _sut = new UpdateRoleCommandHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        UpdateRoleCommand command = _updateRoleCommandFixture.CreateCommand();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        ErrorOr<RolePermissionsResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockRoleRepository.DidNotReceive().UpdateAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUpdateRoleFails_ShouldReturnError()
    {
        // Arrange
        UpdateRoleCommand command = _updateRoleCommandFixture.CreateCommand();
        Error error = Error.Failure("Database.Error", "Failed to update role");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.UpdateAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<RolePermissionsResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetRoleByIdFails_ShouldReturnError()
    {
        // Arrange
        UpdateRoleCommand command = _updateRoleCommandFixture.CreateCommand();
        Error error = Error.Failure("Database.Error", "Failed to get role");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.UpdateAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockRoleRepository.GetByIdAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<RolePermissionsResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenGetRoleByIdReturnsNull_ShouldReturnError()
    {
        // Arrange
        UpdateRoleCommand command = _updateRoleCommandFixture.CreateCommand();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.UpdateAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockRoleRepository.GetByIdAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns((RoleEntity?)null);

        // Act
        ErrorOr<RolePermissionsResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Persistence.ErrorPersistingAuthorizationRole, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenAllOperationsSucceed_ShouldReturnSuccessResponse()
    {
        // Arrange
        UpdateRoleCommand command = _updateRoleCommandFixture.CreateCommand();
        RoleEntity role = new()
        {
            Id = command.RoleId,
            RoleName = command.RoleName,
            RolePermissions = command.Permissions.Select(p => new RolePermissionEntity
            {
                RoleId = command.RoleId,
                PermissionId = p,
                Role = null!,
                Permission = new()
                {
                    Id = p,
                    PermissionName = AuthorizationPermission.CanViewUsers
                }
            }).ToList()
        };

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.UpdateAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockRoleRepository.GetByIdAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns(role);

        // Act
        ErrorOr<RolePermissionsResponse> result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(command.RoleName, result.Value.Role.RoleName);
        Assert.Equal(command.Permissions.Count, result.Value.Permissions.Length);
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
