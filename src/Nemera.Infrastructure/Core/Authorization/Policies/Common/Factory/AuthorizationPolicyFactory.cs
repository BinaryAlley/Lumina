#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
#endregion

namespace Lumina.Infrastructure.Core.Authorization.Policies.Common.Factory;

/// <summary>
/// Concrete implementation for the authorization policies factory.
/// </summary>
public class AuthorizationPolicyFactory : IAuthorizationPolicyFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationPolicyFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public AuthorizationPolicyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a new authorization policy of type <typeparamref name="TAuthorizationPolicy"/>.
    /// </summary>
    /// <typeparam name="TAuthorizationPolicy">The type of authorization policy to create.</typeparam>
    /// <returns>An authorization policy of type <typeparamref name="TAuthorizationPolicy"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type of the requested repository has not been registered.</exception>
    public TAuthorizationPolicy CreatePolicy<TAuthorizationPolicy>() where TAuthorizationPolicy : notnull, IAuthorizationPolicy
    {
        return _serviceProvider.GetRequiredService<TAuthorizationPolicy>() ?? throw new ArgumentException();
    }
}
