#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Fixtures.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Fixtures.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Infrastructure.Core.Authorization.Policies.ConsumptionActivity;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authorization.Policies.ConsumptionActivity;

/// <summary>
/// Contains unit tests for the <see cref="ConsumptionActivityAccessPolicy"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionActivityAccessPolicyTests
{
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ConsumptionActivityAccessPolicy _sut;
    private readonly ConsumptionActivityAccessPolicyContextFixture _consumptionActivityAccessPolicyContextFixture = new();
    private readonly LibraryOwnershipPolicyContextFixture _libraryOwnershipPolicyContextFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumptionActivityAccessPolicyTests"/> class.
    /// </summary>
    public ConsumptionActivityAccessPolicyTests()
    {
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();

        _sut = new ConsumptionActivityAccessPolicy(_mockAuthorizationService);
    }

    [Fact]
    public async Task EvaluateAsync_WhenUserIdIsEmpty_ShouldReturnFalseWithoutCheckingRole()
    {
        // Arrange
        Guid userId = Guid.Empty;
        ConsumptionActivityAccessPolicyContext context = _consumptionActivityAccessPolicyContextFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        // A user in the admin role would make the policy return true, so a false result can only come from the empty user Id guard.
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, cancellationToken);

        // Assert
        Assert.False(result);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenContextIsNull_ShouldReturnFalseWithoutCheckingRole()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        bool result = await _sut.EvaluateAsync(userId, null, cancellationToken);

        // Assert
        Assert.False(result);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenContextIsNotConsumptionActivityContext_ShouldReturnFalseWithoutCheckingRole()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        LibraryOwnershipPolicyContext context = _libraryOwnershipPolicyContextFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, cancellationToken);

        // Assert
        Assert.False(result);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenTargetUserIdIsEmpty_ShouldReturnFalseWithoutCheckingRole()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ConsumptionActivityAccessPolicyContext context = _consumptionActivityAccessPolicyContextFixture.Create(targetUserId: Guid.Empty);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, cancellationToken);

        // Assert
        Assert.False(result);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenTargetUserIsTheSameUser_ShouldReturnTrueWithoutCheckingRole()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ConsumptionActivityAccessPolicyContext context = _consumptionActivityAccessPolicyContextFixture.Create(targetUserId: userId);
        CancellationToken cancellationToken = CancellationToken.None;
        // The user is deliberately not an admin, so an implementation that still consulted the roles would return false.
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, cancellationToken);

        // Assert
        Assert.True(result);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateAsync_WhenTargetUserIsDifferentAndUserIsInAdminRole_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ConsumptionActivityAccessPolicyContext context = _consumptionActivityAccessPolicyContextFixture.Create(targetUserId: Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, cancellationToken);

        // Assert
        Assert.True(result);
        await _mockAuthorizationService.Received(1).IsInRoleAsync(
            Arg.Is(userId),
            Arg.Is(AuthorizationRoles.ADMIN),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task EvaluateAsync_WhenTargetUserIsDifferentAndUserIsNotInAdminRole_ShouldReturnFalse()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ConsumptionActivityAccessPolicyContext context = _consumptionActivityAccessPolicyContextFixture.Create(targetUserId: Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        bool result = await _sut.EvaluateAsync(userId, context, cancellationToken);

        // Assert
        Assert.False(result);
        await _mockAuthorizationService.Received(1).IsInRoleAsync(
            Arg.Is(userId),
            Arg.Is(AuthorizationRoles.ADMIN),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task EvaluateAsync_WhenRoleCheckIsCancelled_ShouldPropagateCancellation()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ConsumptionActivityAccessPolicyContext context = _consumptionActivityAccessPolicyContextFixture.Create(targetUserId: Guid.NewGuid());
        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        cancellationTokenSource.Cancel();
        _mockAuthorizationService.IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new OperationCanceledException(cancellationToken)));

        // Act
        Task<bool> operation = _sut.EvaluateAsync(userId, context, cancellationToken);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
    }
}
