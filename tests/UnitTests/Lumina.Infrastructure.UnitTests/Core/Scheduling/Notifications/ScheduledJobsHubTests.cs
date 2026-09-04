#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Infrastructure.Core.Scheduling.Notifications;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Scheduling.Notifications;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobsHub"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobsHubTests
{
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ScheduledJobsHub _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobsHubTests"/> class.
    /// </summary>
    public ScheduledJobsHubTests()
    {
        Microsoft.Extensions.DependencyInjection.IServiceScopeFactory mockScopeFactory = Substitute.For<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        Microsoft.Extensions.DependencyInjection.IServiceScope mockScope = Substitute.For<Microsoft.Extensions.DependencyInjection.IServiceScope>();
        System.IServiceProvider mockServiceProvider = Substitute.For<System.IServiceProvider>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();

        mockScope.ServiceProvider.Returns(mockServiceProvider);
        mockServiceProvider.GetService(typeof(IAuthorizationService)).Returns(_mockAuthorizationService);
        mockScopeFactory.CreateScope().Returns(mockScope);

        _sut = new ScheduledJobsHub(mockScopeFactory);
    }

    [Fact]
    public async Task OnConnectedAsync_WhenUserClaimIsMissing_ShouldAbortTheConnection()
    {
        // Arrange
        HubCallerContext context = CreateHubCallerContext(new ClaimsPrincipal(new ClaimsIdentity()));
        _sut.Context = context;

        // Act
        await _sut.OnConnectedAsync();

        // Assert
        context.Received(1).Abort();
    }

    [Fact]
    public async Task OnConnectedAsync_WhenUserIdClaimIsNotAGuid_ShouldAbortTheConnection()
    {
        // Arrange
        ClaimsPrincipal user = new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "not-a-guid")]));
        HubCallerContext context = CreateHubCallerContext(user);
        _sut.Context = context;

        // Act
        await _sut.OnConnectedAsync();

        // Assert
        context.Received(1).Abort();
    }

    [Fact]
    public async Task OnConnectedAsync_WhenUserIsNotAdministrator_ShouldAbortTheConnection()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal user = new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())]));
        HubCallerContext context = CreateHubCallerContext(user);
        _sut.Context = context;
        _mockAuthorizationService.IsInRoleAsync(userId, "Admin", System.Threading.CancellationToken.None).Returns(false);

        // Act
        await _sut.OnConnectedAsync();

        // Assert
        context.Received(1).Abort();
    }

    [Fact]
    public async Task OnConnectedAsync_WhenUserIsAdministrator_ShouldNotAbortTheConnection()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal user = new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())]));
        HubCallerContext context = CreateHubCallerContext(user);
        _sut.Context = context;
        _mockAuthorizationService.IsInRoleAsync(userId, "Admin", System.Threading.CancellationToken.None).Returns(true);

        // Act
        await _sut.OnConnectedAsync();

        // Assert
        context.DidNotReceive().Abort();
    }

    /// <summary>
    /// Creates a mocked hub caller context carrying the provided user.
    /// </summary>
    /// <param name="user">The user of the connection.</param>
    /// <returns>The created context.</returns>
    private static HubCallerContext CreateHubCallerContext(ClaimsPrincipal user)
    {
        HubCallerContext context = Substitute.For<HubCallerContext>();
        context.User.Returns(user);
        return context;
    }
}
