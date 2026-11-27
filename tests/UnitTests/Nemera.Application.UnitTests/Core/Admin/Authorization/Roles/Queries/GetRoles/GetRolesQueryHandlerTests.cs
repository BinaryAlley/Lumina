#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.DataAccess.Repositories.Authorization;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Core.Admin.Authorization.Roles.Queries.GetRoles;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Fixtures.Common.DTO.Pagination;
using Nemera.Application.Fixtures.Core.Admin.Authorization.Roles.Queries.GetRoles;
using Nemera.Contracts.Responses.Authorization;
using Nemera.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.UnitTests.Core.Admin.Authorization.Roles.Queries.GetRoles;
/// <summary>
/// Contains unit tests for the <see cref="GetRolesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolesQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IRoleRepository _mockRoleRepository;
    private readonly GetRolesQueryHandler _sut;
    private readonly GetRolesQueryFixture _getRolesQueryFixture = new();
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly PaginatedResultDtoFixture<RoleEntity> _paginatedResultDtoFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolesQueryHandlerTests"/> class.
    /// </summary>
    public GetRolesQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockRoleRepository = Substitute.For<IRoleRepository>();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.RoleRepository.Returns(_mockRoleRepository);

        _sut = new GetRolesQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetRolesQuery query = _getRolesQueryFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IEnumerable<RoleResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockRoleRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetRolesQuery query = _getRolesQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IEnumerable<RoleResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockRoleRepository.DidNotReceive().GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetRolesFails_ShouldReturnError()
    {
        // Arrange
        GetRolesQuery query = _getRolesQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get roles");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IEnumerable<RoleResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenAllOperationsSucceed_ShouldReturnRoles()
    {
        // Arrange
        GetRolesQuery query = _getRolesQueryFixture.Create();
        List<RoleEntity> roles =
        [
            _roleEntityFixture.Create(roleName: "Admin"),
            _roleEntityFixture.Create(roleName: "User")
        ];

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRoleRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create(data: roles, currentPage: 1, perPage: roles.Count, count: roles.Count, numberOfPages: 1)));

        // Act
        Result<IEnumerable<RoleResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
    }
}
