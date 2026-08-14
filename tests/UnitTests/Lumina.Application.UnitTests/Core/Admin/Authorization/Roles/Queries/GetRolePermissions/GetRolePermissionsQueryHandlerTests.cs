#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Queries.GetRolePermissions.Fixtures;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
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
        IValidator<GetRolePermissionsQuery> mockValidator = Substitute.For<IValidator<GetRolePermissionsQuery>>();
        mockValidator.Validate(Arg.Any<GetRolePermissionsQuery>())
            .Returns([]);
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockRoleRepository = Substitute.For<IRoleRepository>();
        _getRolePermissionsQueryFixture = new GetRolePermissionsQueryFixture();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.RoleRepository.Returns(_mockRoleRepository);

        _sut = new GetRolePermissionsQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork,
            mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.CreateQuery();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockRoleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetRoleByIdFails_ShouldReturnError()
    {
        // Arrange
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.CreateQuery();
        Error error = Error.Failure("Database.Error", "Failed to get role");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetByIdAsync(query.RoleId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleNotFound_ShouldReturnRoleNotFoundError()
    {
        // Arrange
        GetRolePermissionsQuery query = _getRolePermissionsQueryFixture.CreateQuery();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetByIdAsync(query.RoleId, Arg.Any<CancellationToken>())
            .Returns((RoleEntity?)null);

        // Act
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.RoleNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenAllOperationsSucceed_ShouldReturnRolePermissions()
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
        Result<RolePermissionsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(role.RoleName, result.Value.Role.RoleName);
        Assert.Single(result.Value.Permissions);
    }
}
