#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Queries.GetRolePermissions.Fixtures;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;

/// <summary>
/// Contains unit tests for the <see cref="GetRolePermissionsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IRoleRepository _mockRoleRepository;
    private readonly GetRolePermissionsQueryHandler _sut;
    private readonly GetRolePermissionsQueryFixture _getRolePermissionsQueryFixture;
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsQueryHandlerTests"/> class.
    /// </summary>
    public GetRolePermissionsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockRoleRepository = Substitute.For<IRoleRepository>();
        _getRolePermissionsQueryFixture = new GetRolePermissionsQueryFixture();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.GetRepository<IRoleRepository>().Returns(_mockRoleRepository);

        _sut = new GetRolePermissionsQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.CreateQuery();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        ErrorOr<RolePermissionsResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.NotAuthorized);
        await _mockRoleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetRoleByIdFails_ShouldReturnError()
    {
        // Arrange
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.CreateQuery();
        Error error = Error.Failure("Database.Error", "Failed to get role");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetByIdAsync(query.RoleId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<RolePermissionsResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnRoleNotFoundError()
    {
        // Arrange
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.CreateQuery();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetByIdAsync(query.RoleId, Arg.Any<CancellationToken>())
            .Returns((RoleEntity?)null);

        // Act
        ErrorOr<RolePermissionsResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.RoleNotFound);
    }

    [Fact]
    public async Task Handle_WhenAllOperationsSucceed_ShouldReturnRolePermissions()
    {
        // Arrange
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.CreateQuery();
        RoleEntity role = new()
        {
            Id = query.RoleId,
            RoleName = "TestRole",
            RolePermissions =
            [
                new()
                {
                    RoleId = query.RoleId,
                    PermissionId = Guid.NewGuid(),
                    Role = null!,
                    Permission = new()
                    {
                        Id = Guid.NewGuid(),
                        PermissionName = AuthorizationPermission.CanViewUsers
                    }
                }
            ]
        };

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetByIdAsync(query.RoleId, Arg.Any<CancellationToken>())
            .Returns(role);

        // Act
        ErrorOr<RolePermissionsResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Role.RoleName.Should().Be(role.RoleName);
        result.Value.Permissions.Should().HaveCount(1);
    }
}
