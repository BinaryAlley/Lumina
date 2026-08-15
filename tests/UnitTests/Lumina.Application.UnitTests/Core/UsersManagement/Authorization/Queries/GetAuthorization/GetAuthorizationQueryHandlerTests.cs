#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
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
    private readonly GetAuthorizationQueryFixture _getAuthorizationQueryFixture = new();
    private readonly UserAuthorizationEntityFixture _userAuthorizationEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationQueryHandlerTests"/> class.
    /// </summary>
    public GetAuthorizationQueryHandlerTests()
    {
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        IValidator<GetAuthorizationQuery> mockValidator = Substitute.For<IValidator<GetAuthorizationQuery>>();
        mockValidator.Validate(Arg.Any<GetAuthorizationQuery>())
            .Returns([]);
        _sut = new GetAuthorizationQueryHandler(_mockAuthorizationService, _mockCurrentUserService, mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenRequestingOwnAuthorization_ShouldReturnAuthorization()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetAuthorizationQuery query = _getAuthorizationQueryFixture.Create(userId);
        UserAuthorizationEntity authEntity = _userAuthorizationEntityFixture.Create(userId);

        _mockCurrentUserService.UserId.Returns(userId);
        _mockAuthorizationService.GetUserAuthorizationAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.From(authEntity));

        // Act
        Result<AuthorizationResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(authEntity.Role, result.Value.Role);
        Assert.Equal(authEntity.Permissions, result.Value.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenAdminRequestingOtherUserAuthorization_ShouldReturnAuthorization()
    {
        // Arrange
        Guid adminUserId = Guid.NewGuid();
        Guid targetUserId = Guid.NewGuid();
        GetAuthorizationQuery query = _getAuthorizationQueryFixture.Create(targetUserId);
        UserAuthorizationEntity adminAuthEntity = _userAuthorizationEntityFixture.Create(adminUserId, true);
        UserAuthorizationEntity targetAuthEntity = _userAuthorizationEntityFixture.Create(targetUserId);

        _mockCurrentUserService.UserId.Returns(adminUserId);
        _mockAuthorizationService.GetUserAuthorizationAsync(adminUserId, Arg.Any<CancellationToken>())
            .Returns(Result.From(adminAuthEntity));
        _mockAuthorizationService.GetUserAuthorizationAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns(Result.From(targetAuthEntity));

        // Act
        Result<AuthorizationResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(targetUserId, result.Value.UserId);
        Assert.Equal(targetAuthEntity.Role, result.Value.Role);
        Assert.Equal(targetAuthEntity.Permissions, result.Value.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenNonAdminRequestingOtherUserAuthorization_ShouldReturnError()
    {
        // Arrange
        Guid currentUserId = Guid.NewGuid();
        Guid targetUserId = Guid.NewGuid();
        GetAuthorizationQuery query = _getAuthorizationQueryFixture.Create(targetUserId);
        UserAuthorizationEntity currentUserAuth = _userAuthorizationEntityFixture.Create(currentUserId, false);

        _mockCurrentUserService.UserId.Returns(currentUserId);
        _mockAuthorizationService.GetUserAuthorizationAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(Result.From(currentUserAuth));

        // Act
        Result<AuthorizationResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Authorization.NotAuthorized, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenGetCurrentUserAuthorizationFails_ShouldReturnError()
    {
        // Arrange
        Guid currentUserId = Guid.NewGuid();
        Guid targetUserId = Guid.NewGuid();
        GetAuthorizationQuery query = _getAuthorizationQueryFixture.Create(targetUserId);
        Error error = Error.Failure("Database.Error", "Failed to get user authorization");

        _mockCurrentUserService.UserId.Returns(currentUserId);
        _mockAuthorizationService.GetUserAuthorizationAsync(currentUserId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<AuthorizationResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenGetTargetUserAuthorizationFails_ShouldReturnError()
    {
        // Arrange
        Guid adminUserId = Guid.NewGuid();
        Guid targetUserId = Guid.NewGuid();
        GetAuthorizationQuery query = _getAuthorizationQueryFixture.Create(targetUserId);
        UserAuthorizationEntity adminAuthEntity = _userAuthorizationEntityFixture.Create(adminUserId, true);
        Error error = Error.Failure("Database.Error", "Failed to get target user authorization");

        _mockCurrentUserService.UserId.Returns(adminUserId);
        _mockAuthorizationService.GetUserAuthorizationAsync(adminUserId, Arg.Any<CancellationToken>())
            .Returns(Result.From(adminAuthEntity));
        _mockAuthorizationService.GetUserAuthorizationAsync(targetUserId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<AuthorizationResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }
}
