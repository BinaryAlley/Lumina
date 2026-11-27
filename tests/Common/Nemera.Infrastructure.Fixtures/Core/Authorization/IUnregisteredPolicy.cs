#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
#endregion

namespace Nemera.Infrastructure.Fixtures.Core.Authorization;

/// <summary>
/// Test policy interface for unregistered policy scenarios.
/// </summary>
public interface IUnregisteredPolicy : IAuthorizationPolicy { }
