#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;
using Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetUserPermissions.Fixtures;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
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
    private readonly GetUserPermissionsQueryFixture _getUserPermissionsQueryFixture;
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
        _getUserPermissionsQueryFixture = new GetUserPermissionsQueryFixture();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);

        _sut = new GetUserPermissionsQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetUserPermissionsQuery query = _getUserPermissionsQueryFixture.CreateQuery();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        ErrorOr<IEnumerable<PermissionResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.NotAuthorized);
        await _mockUserRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetUserByIdFails_ShouldReturnError()
    {
        // Arrange
        GetUserPermissionsQuery query = _getUserPermissionsQueryFixture.CreateQuery();
        Error error = Error.Failure("Database.Error", "Failed to get user");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<PermissionResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnUsernameDoesNotExistError()
    {
        // Arrange
        GetUserPermissionsQuery query = _getUserPermissionsQueryFixture.CreateQuery();

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetByIdAsync(query.UserId!.Value, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        // Act
        ErrorOr<IEnumerable<PermissionResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authentication.UsernameDoesNotExist);
    }

    [Fact]
    public async Task Handle_WhenUserHasPermissions_ShouldReturnPermissions()
    {
        // Arrange
        GetUserPermissionsQuery query = _getUserPermissionsQueryFixture.CreateQuery();
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
        ErrorOr<IEnumerable<PermissionResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.First().PermissionName.Should().Be(AuthorizationPermission.CanViewUsers);
    }
}
