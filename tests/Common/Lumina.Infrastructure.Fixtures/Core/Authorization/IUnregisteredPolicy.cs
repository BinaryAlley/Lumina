#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
#endregion

namespace Lumina.Infrastructure.Fixtures.Core.Authorization;

/// <summary>
/// Test policy interface for unregistered policy scenarios.
/// </summary>
public interface IUnregisteredPolicy : IAuthorizationPolicy { }
