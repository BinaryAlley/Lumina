#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;
using Lumina.Application.Fixtures.Core.UsersManagement.Authorization.Queries.GetUserPermissions;
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

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetUserPermissions;

/// <summary>
/// Contains unit tests for the <see cref="GetUserPermissionsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IUserRepository _mockUserRepository;
    private readonly GetUserPermissionsQueryHandler _sut;
    private readonly GetUserPermissionsQueryFixture _getUserPermissionsQueryFixture = new();
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPermissionsQueryHandlerTests"/> class.
    /// </summary>
    public GetUserPermissionsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.UserRepository.Returns(_mockUserRepository);

        IValidator<GetUserPermissionsQuery> mockValidator = Substitute.For<IValidator<GetUserPermissionsQuery>>();
        mockValidator.Validate(Arg.Any<GetUserPermissionsQuery>())
            .Returns([]);
        _sut = new GetUserPermissionsQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork,
            mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetUserPermissionsQuery query = _getUserPermissionsQueryFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
        await _mockUserRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetUserByIdFails_ShouldReturnError()
    {
        // Arrange
        GetUserPermissionsQuery query = _getUserPermissionsQueryFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get user");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ShouldReturnUsernameDoesNotExistError()
    {
        // Arrange
        GetUserPermissionsQuery query = _getUserPermissionsQueryFixture.Create();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authentication.UsernameDoesNotExist, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasPermissions_ShouldReturnPermissions()
    {
        // Arrange
        GetUserPermissionsQuery query = _getUserPermissionsQueryFixture.Create();
        UserEntity user = new()
        {
            Id = query.UserId!.Value,
            Username = "testUser",
            Password = "hashedPassword",
            Libraries = [],
            UserPermissions =
            [
                new()
                {
                    UserId = query.UserId!.Value,
                    PermissionId = Guid.NewGuid(),
                    User = null!,
                    Permission = new() { Id = Guid.NewGuid(), PermissionName = AuthorizationPermission.CanViewUsers }
                }
            ],
            UserRole = null,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = query.UserId!.Value
        };

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        Result<IEnumerable<PermissionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal(AuthorizationPermission.CanViewUsers, result.Value.First().PermissionName);
    }
}
