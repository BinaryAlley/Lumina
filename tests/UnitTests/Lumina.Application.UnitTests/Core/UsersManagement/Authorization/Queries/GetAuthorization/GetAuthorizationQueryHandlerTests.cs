#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetAuthorization.Fixtures;
using Lumina.Contracts.Responses.Authorization;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Queries.GetAuthorization;

/// <summary>
/// Contains unit tests for the <see cref="GetAuthorizationQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationQueryHandlerTests
{
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly GetAuthorizationQueryHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationQueryHandlerTests"/> class.
    /// </summary>
    public GetAuthorizationQueryHandlerTests()
    {
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetAuthorizationQueryHandler(_mockAuthorizationService, _mockCurrentUserService);
    }

    [Fact]
    public async Task Handle_WhenRequestingOwnAuthorization_ShouldReturnAuthorization()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetAuthorizationQuery query = GetAuthorizationQueryFixture.CreateGetAuthorizationQuery(userId);
        UserAuthorizationEntity authEntity = GetAuthorizationQueryFixture.CreateUserAuthorizationEntity(userId);

        _mockCurrentUserService.UserId.Returns(userId);
        _mockAuthorizationService.GetUserAuthorizationAsync(userId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(authEntity));

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(authEntity.Role, result.Value.Role);
        Assert.Equal(authEntity.Permissions, result.Value.Permissions);
    }

    [Fact]
    public async Task Handle_WhenAdminRequestingOtherUserAuthorization_ShouldReturnAuthorization()
    {
        // Arrange
        Guid adminUserId = Guid.NewGuid();
        Guid targetUserId = Guid.NewGuid();
        GetAuthorizationQuery query = GetAuthorizationQueryFixture.CreateGetAuthorizationQuery(targetUserId);
        UserAuthorizationEntity adminAuthEntity = GetAuthorizationQueryFixture.CreateUserAuthorizationEntity(adminUserId, true);
        UserAuthorizationEntity targetAuthEntity = GetAuthorizationQueryFixture.CreateUserAuthorizationEntity(targetUserId);

        _mockCurrentUserService.UserId.Returns(adminUserId);
        _mockAuthorizationService.GetUserAuthorizationAsync(adminUserId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(adminAuthEntity));
        _mockAuthorizationService.GetUserAuthorizationAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(targetAuthEntity));

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(targetUserId, result.Value.UserId);
        Assert.Equal(targetAuthEntity.Role, result.Value.Role);
        Assert.Equal(targetAuthEntity.Permissions, result.Value.Permissions);
    }

    [Fact]
    public async Task Handle_WhenNonAdminRequestingOtherUserAuthorization_ShouldReturnError()
    {
        // Arrange
        Guid currentUserId = Guid.NewGuid();
        Guid targetUserId = Guid.NewGuid();
        GetAuthorizationQuery query = GetAuthorizationQueryFixture.CreateGetAuthorizationQuery(targetUserId);
        UserAuthorizationEntity currentUserAuth = GetAuthorizationQueryFixture.CreateUserAuthorizationEntity(currentUserId, false);

        _mockCurrentUserService.UserId.Returns(currentUserId);
        _mockAuthorizationService.GetUserAuthorizationAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(currentUserAuth));

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenGetCurrentUserAuthorizationFails_ShouldReturnError()
    {
        // Arrange
        Guid currentUserId = Guid.NewGuid();
        Guid targetUserId = Guid.NewGuid();
        GetAuthorizationQuery query = GetAuthorizationQueryFixture.CreateGetAuthorizationQuery(targetUserId);
        Error error = Error.Failure("Database.Error", "Failed to get user authorization");

        _mockCurrentUserService.UserId.Returns(currentUserId);
        _mockAuthorizationService.GetUserAuthorizationAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task Handle_WhenGetTargetUserAuthorizationFails_ShouldReturnError()
    {
        // Arrange
        Guid adminUserId = Guid.NewGuid();
        Guid targetUserId = Guid.NewGuid();
        GetAuthorizationQuery query = GetAuthorizationQueryFixture.CreateGetAuthorizationQuery(targetUserId);
        UserAuthorizationEntity adminAuthEntity = GetAuthorizationQueryFixture.CreateUserAuthorizationEntity(adminUserId, true);
        Error error = Error.Failure("Database.Error", "Failed to get target user authorization");

        _mockCurrentUserService.UserId.Returns(adminUserId);
        _mockAuthorizationService.GetUserAuthorizationAsync(adminUserId, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(adminAuthEntity));
        _mockAuthorizationService.GetUserAuthorizationAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        ErrorOr<AuthorizationResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
    }
}
