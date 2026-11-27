#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.DataAccess.Repositories.Authorization;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Fixtures.Common.DTO.Pagination;
using Nemera.Application.Fixtures.Core.Admin.Authorization.Permissions.Queries.GetPermissions;
using Nemera.Contracts.Responses.Authorization;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.SharedKernel.Common.Enums.Authorization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.UnitTests.Core.Admin.Authorization.Permissions.Queries.GetPermissions;

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
    private readonly GetPermissionsQueryFixture _getPermissionsQueryFixture = new();
    private readonly PermissionEntityFixture _permissionEntityFixture = new();
    private readonly PaginatedResultDtoFixture<PermissionEntity> _paginatedResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPermissionsQueryHandlerTests"/> class.
    /// </summary>
    public GetPermissionsQueryHandlerTests()
    {
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockPermissionRepository = Substitute.For<IPermissionRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.PermissionRepository.Returns(_mockPermissionRepository);

        _sut = new GetPermissionsQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAdmin_ShouldReturnPermissions()
    {
        // Arrange
        GetPermissionsQuery query = _getPermissionsQueryFixture.Create();
        List<PermissionEntity> permissions =
        [
            _permissionEntityFixture.Create(permissionName: AuthorizationPermission.CanViewUsers),
            _permissionEntityFixture.Create(permissionName: AuthorizationPermission.CanDeleteUsers)
        ];

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockPermissionRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: permissions, currentPage: 1, perPage: permissions.Count, count: permissions.Count, numberOfPages: 1)));

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
        await _mockAuthorizationService.Received(1).IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>());
        await _mockPermissionRepository.Received(1).GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetPermissionsQuery query = _getPermissionsQueryFixture.Create();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>());
        await _mockPermissionRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetPermissionsQuery query = _getPermissionsQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockPermissionRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllPermissionsReturnsError_ShouldReturnError()
    {
        // Arrange
        GetPermissionsQuery query = _getPermissionsQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to retrieve permissions");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockPermissionRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockAuthorizationService.Received(1).IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>());
        await _mockPermissionRepository.Received(1).GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCancellationRequested_ShouldStillCompleteOperation()
    {
        // Arrange
        GetPermissionsQuery query = _getPermissionsQueryFixture.Create();
        List<PermissionEntity> permissions =
        [
            _permissionEntityFixture.Create(permissionName: AuthorizationPermission.CanViewUsers)
        ];
        CancellationToken cancellationToken = new(true);

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockPermissionRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: permissions, currentPage: 1, perPage: permissions.Count, count: permissions.Count, numberOfPages: 1)));

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, cancellationToken);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        await _mockAuthorizationService.Received(1).IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>());
        await _mockPermissionRepository.Received(1).GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }
}
