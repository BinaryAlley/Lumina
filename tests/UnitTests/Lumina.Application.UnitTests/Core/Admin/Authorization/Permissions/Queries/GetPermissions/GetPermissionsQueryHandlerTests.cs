#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;
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

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Permissions.Queries.GetPermissions;

/// <summary>
/// Contains unit tests for the <see cref="GetPermissionsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPermissionsQueryHandlerTests
{
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IPermissionRepository _mockPermissionRepository;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly GetPermissionsQueryHandler _sut;
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPermissionsQueryHandlerTests"/> class.
    /// </summary>
    public GetPermissionsQueryHandlerTests()
    {
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockPermissionRepository = Substitute.For<IPermissionRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.GetRepository<IPermissionRepository>().Returns(_mockPermissionRepository);

        _sut = new GetPermissionsQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAdmin_ShouldReturnPermissions()
    {
        // Arrange
        GetPermissionsQuery query = new();
        IEnumerable<PermissionEntity> permissions =
        [
            new PermissionEntity { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanViewUsers },
            new PermissionEntity { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanDeleteUsers }
        ];

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockPermissionRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(permissions));

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
        await _mockAuthorizationService.Received(1).IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>());
        await _mockPermissionRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetPermissionsQuery query = new();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>());
        await _mockPermissionRepository.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllPermissionsReturnsError_ShouldReturnError()
    {
        // Arrange
        GetPermissionsQuery query = new();
        Error error = Error.Failure("Database.Error", "Failed to retrieve permissions");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockPermissionRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockAuthorizationService.Received(1).IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>());
        await _mockPermissionRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationRequested_ShouldStillCompleteOperation()
    {
        // Arrange
        GetPermissionsQuery query = new();
        IEnumerable<PermissionEntity> permissions =
        [
            new PermissionEntity { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanViewUsers }
        ];
        CancellationToken cancellationToken = new(true);

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockPermissionRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.From(permissions));

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        await _mockAuthorizationService.Received(1).IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>());
        await _mockPermissionRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }
}
