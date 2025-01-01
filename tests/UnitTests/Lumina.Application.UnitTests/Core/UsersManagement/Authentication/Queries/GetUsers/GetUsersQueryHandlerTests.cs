#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.GetUsers;
using Lumina.Application.UnitTests.Common.Mapping.UserManagement.Users.Fixtures;
using Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.GetUsers.Fixtures;
using Lumina.Contracts.Responses.UsersManagement.Users;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authentication.Queries.GetUsers;

/// <summary>
/// Contains unit tests for the <see cref="GetUsersQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUsersQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IUserRepository _mockUserRepository;
    private readonly GetUsersQueryHandler _sut;
    private readonly GetUsersQueryFixture _getUsersQueryFixture;
    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersQueryHandlerTests"/> class.
    /// </summary>
    public GetUsersQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockUserRepository = Substitute.For<IUserRepository>();
        _getUsersQueryFixture = new GetUsersQueryFixture();
        _userId = Guid.NewGuid();

        _mockCurrentUserService.UserId.Returns(_userId);
        _mockUnitOfWork.GetRepository<IUserRepository>().Returns(_mockUserRepository);

        _sut = new GetUsersQueryHandler(
            _mockAuthorizationService,
            _mockCurrentUserService,
            _mockUnitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetUsersQuery query = _getUsersQueryFixture.CreateQuery();
        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        ErrorOr<IEnumerable<UserResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Authorization.NotAuthorized);
        await _mockUserRepository.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGetUsersFails_ShouldReturnError()
    {
        // Arrange
        GetUsersQuery query = _getUsersQueryFixture.CreateQuery();
        Error error = Error.Failure("Database.Error", "Failed to get users");

        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<IEnumerable<UserResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(error);
    }

    [Fact]
    public async Task Handle_WhenAllOperationsSucceed_ShouldReturnUsers()
    {
        // Arrange
        GetUsersQuery query = _getUsersQueryFixture.CreateQuery();
        IEnumerable<UserEntity> users = UserEntityFixture.CreateMany(2);


        _mockAuthorizationService.IsInRoleAsync(_userId, "Admin", Arg.Any<CancellationToken>())
            .Returns(true);
        _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(users));

        // Act
        ErrorOr<IEnumerable<UserResponse>> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
    }
}
