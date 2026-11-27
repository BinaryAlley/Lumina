#region ========================================================================= USING =====================================================================================
using System;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
#endregion

namespace Lumina.Infrastructure.Core.Authorization.Policies.Common.Factory;

/// <summary>
/// Interface for the authorization policies factory.
/// </summary>
public interface IAuthorizationPolicyFactory
{
    /// <summary>
    /// Creates a new authorization policy of type <typeparamref name="TAuthorizationPolicy"/>.
    /// </summary>
    /// <typeparam name="TAuthorizationPolicy">The type of authorization policy to create.</typeparam>
    /// <returns>An authorization policy of type <typeparamref name="TAuthorizationPolicy"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type of the requested repository has not been registered.</exception>
    TAuthorizationPolicy CreatePolicy<TAuthorizationPolicy>() where TAuthorizationPolicy : notnull, IAuthorizationPolicy;
}
