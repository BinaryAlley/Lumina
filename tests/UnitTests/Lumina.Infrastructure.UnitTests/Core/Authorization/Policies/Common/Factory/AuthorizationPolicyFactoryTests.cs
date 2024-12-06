#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Infrastructure.Core.Authorization.Policies.Common.Factory;
using Lumina.Infrastructure.UnitTests.Core.Authorization.Policies.Common.Factory.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authorization.Policies.Common.Factory;

/// <summary>
/// Contains unit tests for the <see cref="AuthorizationPolicyFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationPolicyFactoryTests
{
    
    [Fact]
    public void CreatePolicy_WhenCalled_ShouldReturnCorrectPolicy()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddTransient<IAuthorizationPolicy, TestAuthorizationPolicy>();
        services.AddTransient<TestAuthorizationPolicy>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        AuthorizationPolicyFactory policyFactory = new(serviceProvider);

        // Act
        TestAuthorizationPolicy result = policyFactory.CreatePolicy<TestAuthorizationPolicy>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IAuthorizationPolicy>();
        result.Should().BeOfType<TestAuthorizationPolicy>();
    }

    [Fact]
    public void CreatePolicy_WhenUnregisteredPolicyRequested_ShouldThrowException()
    {
        // Arrange
        ServiceCollection services = new();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        AuthorizationPolicyFactory policyFactory = new(serviceProvider);

        // Act & Assert
        Action act = () => policyFactory.CreatePolicy<IUnregisteredPolicy>();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No service for type 'Lumina.Infrastructure.UnitTests.Core.Authorization.Policies.Common.Factory.Fixtures.IUnregisteredPolicy' has been registered.");
    }

    [Fact]
    public void CreatePolicy_WhenServiceProviderReturnsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        IServiceProvider mockServiceProvider = Substitute.For<IServiceProvider>();
        mockServiceProvider.GetService(typeof(TestAuthorizationPolicy))
            .Returns(null);
        AuthorizationPolicyFactory policyFactory = new(mockServiceProvider);

        // Act & Assert
        Action act = () => policyFactory.CreatePolicy<TestAuthorizationPolicy>();
        act.Should().Throw<InvalidOperationException>();
    }
}
